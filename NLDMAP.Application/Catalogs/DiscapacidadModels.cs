namespace NLDMAP.Application.Catalogs;

public sealed record DiscapacidadQuery(string? Search = null, int Page = 1);
public sealed record DiscapacidadRow(Guid Id, string NombreDiscapacidad);
public sealed record DiscapacidadInput(Guid? Id, string NombreDiscapacidad);
