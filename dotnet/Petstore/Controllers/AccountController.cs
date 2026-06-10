using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petstore.Accounts;
using Petstore.Data;
using Petstore.Data.Entities;
using Petstore.Models;

namespace Petstore.Controllers;

[ApiController]
[Route("api/account")]
public sealed class AccountController(
    IAccountRepository accountRepository,
    IPasswordHasher passwordHasher) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<AccountDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountDto>> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            missing.Add("userId");
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            missing.Add("password");
        }

        missing.AddRange(ContactValidation.MissingContactFields(request.Contact));
        if (missing.Count > 0)
        {
            return BadRequest(new ApiErrorDto(
                "account.validation",
                $"Missing or invalid fields: {string.Join(", ", missing)}."));
        }

        if (await accountRepository.UserExistsAsync(request.UserId!, cancellationToken))
        {
            return Conflict(new ApiErrorDto("account.duplicate_user", "An account with this user id already exists."));
        }

        var (hash, salt) = passwordHasher.Hash(request.Password!);
        var contact = request.Contact!;
        var user = new UserEntity
        {
            UserId = request.UserId!,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = AccountModelConstants.Roles.Customer,
            CreatedAt = DateTime.UtcNow,
            Contact = new CustomerContactEntity
            {
                UserId = request.UserId!,
                FamilyName = contact.FamilyName,
                GivenName = contact.GivenName,
                Street1 = contact.Street1,
                Street2 = contact.Street2,
                City = contact.City,
                State = contact.State,
                Zip = contact.Zip,
                Country = contact.Country,
                Email = contact.Email,
                Phone = contact.Phone
            }
        };

        await accountRepository.CreateAccountAsync(user, cancellationToken);

        return Created("/api/account", new AccountDto(user.UserId, contact));
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountDto>> GetAccountAsync(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        var contact = await accountRepository.GetContactAsync(userId, cancellationToken);

        return Ok(new AccountDto(userId, contact is null ? null : ToDto(contact)));
    }

    [HttpPut("contact")]
    [Authorize]
    [ProducesResponseType<AccountDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AccountDto>> UpdateContactAsync(
        ContactInfoDto request,
        CancellationToken cancellationToken)
    {
        var missing = ContactValidation.MissingContactFields(request);
        if (missing.Count > 0)
        {
            return BadRequest(new ApiErrorDto(
                "account.validation",
                $"Missing or invalid fields: {string.Join(", ", missing)}."));
        }

        var userId = CurrentUserId();
        await accountRepository.UpdateContactAsync(new CustomerContactEntity
        {
            UserId = userId,
            FamilyName = request.FamilyName,
            GivenName = request.GivenName,
            Street1 = request.Street1,
            Street2 = request.Street2,
            City = request.City,
            State = request.State,
            Zip = request.Zip,
            Country = request.Country,
            Email = request.Email,
            Phone = request.Phone
        }, cancellationToken);

        return Ok(new AccountDto(userId, request));
    }

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated request without a user id claim.");
    }

    private static ContactInfoDto ToDto(CustomerContactEntity contact)
    {
        return new ContactInfoDto(
            contact.FamilyName,
            contact.GivenName,
            contact.Street1,
            contact.Street2,
            contact.City,
            contact.State,
            contact.Zip,
            contact.Country,
            contact.Email,
            contact.Phone);
    }
}
