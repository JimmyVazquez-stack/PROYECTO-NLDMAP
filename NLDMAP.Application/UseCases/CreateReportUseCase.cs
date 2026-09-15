using NLDMAP.Application.DTOs;
using NLDMAP.Domain.Entities;
using NLDMAP.Domain.Repositories;
using NLDMAP.Domain.ValueObjects;

namespace NLDMAP.Application.UseCases;

public class CreateReportUseCase
{
    private readonly IReportRepository _reportRepository;

    public CreateReportUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<string> ExecuteAsync(CreateReportRequest request)
    {
        // Instanciar la entidad de la persona
        var person = new MissingPerson(
            request.MissingPersonName,
            request.MissingPersonAge,
            request.MissingPersonGender,
            request.MissingPersonHeight
        );

        // Instanciar el value object completo
        var events = new DisappearanceEvents(

            request.DisappearanceDate,
            request.DisappearanceHour,
            request.Circumstance,
            request.FactsDescription,
            request.ReporterPresent,
            request.Street,
            request.City,
            request.State,
            request.Municipality,
            request.LastSightingLatitude,
            request.LastSightingLongitude
        );

        // Instanciar la entidad Report
        var report = new Report(
            person.Id,
            request.CreatorId,
            request.MissingPersonRelation,
            events,
            request.ConsentUseExclusive,
            request.RequestInformationPublic
            );
        
        // 4. Guardar en base de datos de manera asíncrona usando la abstracción
        await _reportRepository.AddAsync(report);

        // 5. Retornar el folio generado al cliente móvil
        return report.Id.ToString();
    }
}