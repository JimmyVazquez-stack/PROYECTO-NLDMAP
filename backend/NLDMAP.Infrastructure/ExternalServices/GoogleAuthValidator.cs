using Google.Apis.Auth;
using NLDMAP.Domain.Interfaces;
using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Infrastructure.ExternalServices;

public class GoogleAuthValidator : IExternalAuthValidator
{
    public async Task<ExternalUserInfo?> ValidateTokenAsync(string provider, string idToken)
    {
        if (provider.ToLower() != "google") return null;

        try
        {
            // Verifica la firma del token con servidores de google
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

            return new ExternalUserInfo
            {
                Email = payload.Email,
                FullName = payload.Name,
                
                ProviderId = payload.Subject,
                ProviderName = "Google"
            };
        }
        catch (InvalidJwtException)
        {
            return  null;  // Token expirado o modificado
        }
    }
}