using Microsoft.AspNetCore.Mvc;
using NLDMAP.Application.DTOs.Auth;
using NLDMAP.Application.UseCases.Auth;


namespace NLDMAP.API.Controllers
{
    [ApiController] //validacion por parte de .NET los datos entrantes
    [Route("api/[controller]")] //ruta base /api/Auth
    public class AuthController : ControllerBase
    {
        //Casos de uso
        //private readonly IUserRepository _userRepository
        //private readonly AuthenticateExternalUserUseCase _authenticateUseCase;

        //Constructor
        //public AuthController(AuthenticateExternalUserUseCase authenticateUseCase)
        
        public AuthController()
        {
           // _authenticateUseCase = authenticateUseCase;
           //_userRepository = userRepository
        }
    
        // Endpoint al que se intenta acceder
        [HttpGet("test-db")]
        public IActionResult TestDatabase()
        {
            try
            {
                //Aqui ira llamada real a _userRepository
                //var user = await _userRepository.CrearOObtenerAsync(...);
                
                //Wrapper estandar para interfaz
                var response = new
                {
                    succesfull = true,
                    message = "POSTGRESQL : OPERACION EXITOSA",
                    data = new
                    {
                        id = Guid.NewGuid(),
                        role = "Ciudadano",
                        responseTimeMs = 15
                    }
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    succesfull = false,
                    message = $"Error en modulo 1: {ex.Message}",
                });
            }
        }

        /* [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalAuthDto request)
        {
            try
            {
                //Validar caso de uso y registrar al ciudadano si es nuevo
                var userEmail = await _authenticateUseCase.ExecuteAsync(request);
                //Implementar logica de signin de openiddict

                return Ok(new { Message = " Autenticación exitosa", Token = "JWT_AQUI" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }*/

    }   
}