namespace NLDMAP.Application.Catalogs;

public sealed record OcupacionQuery(string? Search = null, int Page = 1);
public sealed record OcupacionRow(Guid Id, string NombreOcupacion, string? DescripcionOcupacion);
public sealed record OcupacionInput(Guid? Id, string NombreOcupacion, string? DescripcionOcupacion);
