using System.Globalization;

namespace Petstore.Models;

public sealed record SalesAnalyticsDateRangeRequest(string? StartDate, string? EndDate)
{
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Parses and validates the inclusive date range. Returns null on success
    /// or a human-readable validation problem naming the offending input.
    /// </summary>
    public string? TryParse(out DateOnly start, out DateOnly end)
    {
        start = default;
        end = default;

        if (string.IsNullOrWhiteSpace(StartDate) || string.IsNullOrWhiteSpace(EndDate))
        {
            return "Both startDate and endDate are required (yyyy-MM-dd).";
        }

        if (!DateOnly.TryParseExact(StartDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
        {
            return "startDate is not a valid yyyy-MM-dd date.";
        }

        if (!DateOnly.TryParseExact(EndDate, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
        {
            return "endDate is not a valid yyyy-MM-dd date.";
        }

        if (start > end)
        {
            return "startDate must not be after endDate.";
        }

        return null;
    }
}
