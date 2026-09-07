using OPS.Application.Dtos;

namespace OPS.Application.Interfaces;

public interface ISecurityService
{
    Task<AuthenticateResponse?> Authenticate(AuthenticateRequest request, CancellationToken cancellationToken = default);
}