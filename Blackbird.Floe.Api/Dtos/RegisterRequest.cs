using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Blackbird.Floe.Api.Dtos;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, PasswordPropertyText] string Password);