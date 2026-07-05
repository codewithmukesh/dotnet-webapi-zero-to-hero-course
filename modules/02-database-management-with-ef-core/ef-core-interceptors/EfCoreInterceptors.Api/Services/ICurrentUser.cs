namespace EfCoreInterceptors.Api.Services;

public interface ICurrentUser
{
    string? UserId { get; }
}
