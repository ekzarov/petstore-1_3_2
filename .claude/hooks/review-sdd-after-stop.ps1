param()

$ErrorActionPreference = "Stop"

function Write-ReviewFile {
    param(
        [string] $Path,
        [string] $Content
    )

    $directory = Split-Path -Parent $Path
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $Content | Out-File -FilePath $Path -Encoding utf8
}

try {
    $hookInput = [Console]::In.ReadToEnd()
    if (-not [string]::IsNullOrWhiteSpace($hookInput)) {
        try {
            $payload = $hookInput | ConvertFrom-Json
            if ($payload.stop_hook_active -eq $true) {
                exit 0
            }
        }
        catch {
            # Ignore malformed hook input and keep the hook non-blocking.
        }
    }

    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    $repoRoot = Resolve-Path (Join-Path $scriptPath "..\..")
    Set-Location $repoRoot

    $flagPath = Join-Path $repoRoot ".claude\run-sdd-review"
    if (-not (Test-Path $flagPath)) {
        exit 0
    }

    $reviewsDir = Join-Path $repoRoot ".claude\reviews"
    $reviewPath = Join-Path $reviewsDir "sdd-review-last.md"
    $inputPath = Join-Path $reviewsDir "sdd-review-input-last.md"
    $childSettingsPath = Join-Path $reviewsDir "child-settings.json"

    $featureDir = $null
    $flagContent = (Get-Content $flagPath -Raw -ErrorAction SilentlyContinue).Trim()
    if (-not [string]::IsNullOrWhiteSpace($flagContent)) {
        $candidate = $flagContent -split "`r?`n" | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1
        if ($candidate) {
            $candidatePath = Join-Path $repoRoot $candidate
            if (Test-Path $candidatePath) {
                $featureDir = $candidate.Replace("\", "/")
            }
        }
    }

    if (-not $featureDir -and (Test-Path ".specify\feature.json")) {
        try {
            $featureJson = Get-Content ".specify\feature.json" -Raw | ConvertFrom-Json
            if ($featureJson.feature_directory -and (Test-Path $featureJson.feature_directory)) {
                $featureDir = $featureJson.feature_directory
            }
        }
        catch {
            # Leave featureDir empty; the reviewer will report that context is missing.
        }
    }

    if (-not $featureDir) {
        $featureDir = "UNKNOWN_FEATURE_DIR"
    }

    $baseRef = "origin/master"
    git rev-parse --verify $baseRef *> $null
    if ($LASTEXITCODE -ne 0) {
        $baseRef = "master"
    }

    $branch = (git branch --show-current).Trim()
    $head = (git rev-parse --short HEAD).Trim()
    $changedCommitted = git diff --name-only "$baseRef...HEAD"
    $changedStaged = git diff --cached --name-only
    $changedUnstaged = git diff --name-only
    $changedUntracked = git ls-files --others --exclude-standard
    $changedFiles = @($changedCommitted; $changedStaged; $changedUnstaged; $changedUntracked) |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Sort-Object -Unique

    $committedDiff = git diff --binary "$baseRef...HEAD"
    $stagedDiff = git diff --cached --binary
    $unstagedDiff = git diff --binary

    $reviewInput = @"
# SDD Compliance Review Request

Feature directory: $featureDir
Branch: $branch
HEAD: $head
Base ref: $baseRef

## Reviewer Instructions

Read the SDD artifacts in the feature directory and compare them with the code
changes below. Report whether the implementation satisfies the SDD, what is
missing, and what questions remain. Do not edit files.

## Changed Files

$($changedFiles -join "`n")

## Committed Diff ($baseRef...HEAD)

```diff
$committedDiff
```

## Staged Diff

```diff
$stagedDiff
```

## Unstaged Diff

```diff
$unstagedDiff
```

## Untracked Files

$($changedUntracked -join "`n")
"@

    if (-not (Test-Path $reviewsDir)) {
        New-Item -ItemType Directory -Path $reviewsDir -Force | Out-Null
    }
    $reviewInput | Out-File -FilePath $inputPath -Encoding utf8
    '{"hooks":{}}' | Out-File -FilePath $childSettingsPath -Encoding utf8

    $claude = Get-Command claude -ErrorAction SilentlyContinue
    if (-not $claude) {
        $content = @(
            "# SDD Compliance Review Not Run",
            "",
            "Claude CLI was not found on PATH, so the Stop hook could not run the sdd-compliance-reviewer.",
            "",
            "Feature directory: $featureDir",
            "Input prepared at: .claude/reviews/sdd-review-input-last.md",
            "",
            "Run manually from the repository root:",
            "",
            '```powershell',
            'Get-Content .claude/reviews/sdd-review-input-last.md -Raw | claude --agent sdd-compliance-reviewer -p "Review this implementation against the supplied SDD artifacts."',
            '```'
        ) -join "`n"
        Write-ReviewFile -Path $reviewPath -Content $content
        Remove-Item $flagPath -Force -ErrorAction SilentlyContinue
        exit 0
    }

    $prompt = "Review this implementation against the supplied SDD artifacts. Return only the review report in Markdown."
    $output = Get-Content $inputPath -Raw |
        & $claude.Source --agent sdd-compliance-reviewer -p --permission-mode acceptEdits --settings $childSettingsPath $prompt 2>&1

    if ($LASTEXITCODE -ne 0) {
        $content = @(
            "# SDD Compliance Review Failed",
            "",
            "Claude CLI exited with code $LASTEXITCODE.",
            "",
            "Feature directory: $featureDir",
            "Input prepared at: .claude/reviews/sdd-review-input-last.md",
            "",
            "## Output",
            "",
            '```text',
            ($output -join "`n"),
            '```'
        ) -join "`n"
        Write-ReviewFile -Path $reviewPath -Content $content
    }
    else {
        Write-ReviewFile -Path $reviewPath -Content ($output -join "`n")
    }

    Remove-Item $flagPath -Force -ErrorAction SilentlyContinue
    exit 0
}
catch {
    try {
        $fallbackPath = Join-Path (Get-Location) ".claude\reviews\sdd-review-last.md"
        $content = @(
            "# SDD Compliance Review Hook Error",
            "",
            "The hook failed, but it is configured as non-blocking.",
            "",
            '```text',
            $_.Exception.Message,
            '```'
        ) -join "`n"
        Write-ReviewFile -Path $fallbackPath -Content $content
    }
    catch {
        # Last-resort no-op: Stop hooks should not break the main Claude session.
    }

    exit 0
}
