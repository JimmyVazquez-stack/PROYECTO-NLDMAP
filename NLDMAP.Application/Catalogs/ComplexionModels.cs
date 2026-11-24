namespace NLDMAP.Application.Catalogs;

public sealed record ComplexionQuery(string? Search = null, int Page = 1);
public sealed record ComplexionRow(Guid Id, string NombreComplexion);
public sealed record ComplexionInput(Guid? Id, string NombreComplexion);
