namespace OPS.Application.Dtos;

public record AuthenticateResponse(string Token, DateTimeOffset Expiration);