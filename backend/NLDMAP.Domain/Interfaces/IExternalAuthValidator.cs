using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Domain.Interfaces
{
    public interface IExternalAuthValidator
    {
        Task<ExternalUserInfo?> ValidateTokenAsync(string provider, string idToken);
    }
}