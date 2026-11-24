namespace NLDMAP.Application.Catalogs;

public sealed record TipoCabelloQuery(string? Search = null, int Page = 1);
public sealed record TipoCabelloRow(Guid Id, string NombreCabello, string LargoCabello);
public sealed record TipoCabelloInput(Guid? Id, string NombreCabello, string LargoCabello);
