namespace NLDMAP.Application.Catalogs;

public sealed record GeneroQuery(string? Search = null, int Page = 1);
public sealed record GeneroRow(Guid Id, string NombreSexo);
public sealed record GeneroInput(Guid? Id, string NombreSexo);
