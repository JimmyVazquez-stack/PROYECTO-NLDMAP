using NLDMAP.Application.DTOs.Auth;
using NLDMAP.Domain.Interfaces;
using NLDMAP.Domain.Entities;
using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Application.UseCases.Auth
{
    public class AuthenticateExternalUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IExternalAuthValidator _externalAuthValidator;
        
        public AuthenticateExternalUserUseCase(
            IUserRepository userRepository,
            IExternalAuthValidator externalAuthValidator)
        {
            _userRepository = userRepository;
            _externalAuthValidator = externalAuthValidator;
        }

        public async Task<string> ExecuteAsync(ExternalAuthDto request)
        {
            //Validar token externo google/facebook
            var userInfo = await _externalAuthValidator.ValidateTokenAsync(request.Provider, request.IdToken);

            if (userInfo == null)
                throw new UnauthorizedAccessException("Token externo invalido.");

            //Buscar si usuario existe en Postgresql por email
            var user = await _userRepository.GetByEmailAsync(userInfo.Email);

            //si no existe, es un ciudadano nuevo 
            if (user == null)
            {
                user = new User
                {
                    Email = userInfo.Email,
                    FullName = userInfo.FullName,
                    Role = "Ciudadano",
                    
                    SsoProvider = userInfo.ProviderName,
                    SsoProviderId = userInfo.ProviderId
                };
                await _userRepository.AddSync(user);
            }

            // controlador se encarga de generar JWT de openiddict
            return user.Email;

        }
    }
}