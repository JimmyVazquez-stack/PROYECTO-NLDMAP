namespace NLDMAP.Application.Catalogs;

public sealed record CircunstanciaQuery(string? Search = null, int Page = 1);
public sealed record CircunstanciaRow(Guid Id, string Tipo, string? DescripcionCircunstancia);
public sealed record CircunstanciaInput(Guid? Id, string Tipo, string? DescripcionCircunstancia);
