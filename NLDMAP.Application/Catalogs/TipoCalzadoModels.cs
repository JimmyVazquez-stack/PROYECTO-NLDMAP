namespace NLDMAP.Application.Catalogs;

public sealed record TipoCalzadoQuery(string? Search = null, int Page = 1);
public sealed record TipoCalzadoRow(Guid Id, string NombreCalzado);
public sealed record TipoCalzadoInput(Guid? Id, string NombreCalzado);
