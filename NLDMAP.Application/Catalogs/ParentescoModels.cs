namespace NLDMAP.Application.Catalogs;

public sealed record ParentescoQuery(string? Search = null, int Page = 1);
public sealed record ParentescoRow(Guid Id, string NombreParentesco);
public sealed record ParentescoInput(Guid? Id, string NombreParentesco);
