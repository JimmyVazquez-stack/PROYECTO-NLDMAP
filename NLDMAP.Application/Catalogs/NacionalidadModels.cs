namespace NLDMAP.Application.Catalogs;

public sealed record NacionalidadQuery(string? Search = null, int Page = 1);
public sealed record NacionalidadRow(Guid Id, string NombreNacionalidad);
public sealed record NacionalidadInput(Guid? Id, string NombreNacionalidad);
