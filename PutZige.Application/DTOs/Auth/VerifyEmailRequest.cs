namespace PutZige.Application.DTOs.Auth;

public sealed record VerifyEmailRequest(string Email, string Token);
