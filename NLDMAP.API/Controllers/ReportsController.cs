using Microsoft.AspNetCore.Mvc;
using NLDMAP.Application.DTOs;
using NLDMAP.Application.UseCases;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace NLDMAP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        //Inyectar caso de uso real
        private readonly CreateReportUseCase _createReportUseCase;
        
        //Inyeccion de dependencias en el constructor
        public ReportsController(CreateReportUseCase createReportUseCase)
        {
            _createReportUseCase = createReportUseCase;
        }
        

        //get : api/reports/map-points
        [HttpGet("map-points")]
        public ActionResult<IEnumerable<MapPointResponse>> GetMapPoints()
        {
            return Ok(new List<MapPointResponse>());
        }

        //Post: api/reports
        //endpoint que recibe datos reales de flutter
        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request)
        {
            //validacion
            if (string.IsNullOrEmpty(request.MissingPersonName) ||
                request.LastSightingLatitude == 0 ||
                request.LastSightingLongitude == 0)
            {
                return BadRequest(new
                {
                    Exito = false,
                    Mensaje = "Faltan datos del nucleo minimo."
                });
            }

            //Ejecutar caso de uso con la creacion de entidades y guardado
            var nuevoFolioId = await _createReportUseCase.ExecuteAsync(request);
            
            //responder a flutter 
            return Ok(new
            {
                Exito= true,
                Mensaje = "Reporte creado exitosamente.",
                Folio = nuevoFolioId
            });
        }
    }
}