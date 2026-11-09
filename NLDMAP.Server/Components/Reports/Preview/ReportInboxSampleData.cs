using NLDMAP.Server.Components.Reports.Models;

namespace NLDMAP.Server.Components.Reports.Preview;

public static class ReportInboxSampleData
{
    public static IReadOnlyList<ReportInboxRow> Create() => [
        new(Guid.Parse("11111111-1111-4111-8111-111111111111"), "DEMO-001", "Persona de ejemplo A", new(2026, 11, 9), "Sin validar", "No publicado", InboxStage.New, "Información ficticia para revisar la presentación de un reporte recién recibido."),
        new(Guid.Parse("22222222-2222-4222-8222-222222222222"), "DEMO-002", "Persona de ejemplo B", new(2026, 11, 8), "Sin validar", "No publicado", InboxStage.New, "Ejemplo sin revisión iniciada. Abrir esta información no cambia su estado."),
        new(Guid.Parse("33333333-3333-4333-8333-333333333333"), "DEMO-003", "Persona de ejemplo C", new(2026, 11, 7), "Necesita corrección", "No publicado", InboxStage.InProgress, "Ejemplo pendiente de aclaración de la información recibida."),
        new(Guid.Parse("44444444-4444-4444-8444-444444444444"), "DEMO-004", "Persona de ejemplo D", new(2026, 11, 6), "Validado", "No publicado", InboxStage.InProgress, "Ejemplo validado con una solicitud de corrección pendiente. La bandeja es distinta del estado de revisión."),
        new(Guid.Parse("55555555-5555-4555-8555-555555555555"), "DEMO-005", "Persona de ejemplo E", new(2026, 11, 5), "Validado", "Publicado", InboxStage.Reviewed, "Ejemplo revisado y publicado. Esta vista no modifica su contenido."),
        new(Guid.Parse("66666666-6666-4666-8666-666666666666"), "DEMO-006", "Persona de ejemplo F", new(2026, 11, 4), "No aprobado", "No publicado", InboxStage.Reviewed, "Ejemplo revisado sin publicación.")
    ];
}
