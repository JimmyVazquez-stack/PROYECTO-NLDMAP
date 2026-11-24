namespace NLDMAP.Application.Catalogs;

public sealed record VehiculoQuery(string? Search = null, int Page = 1);
public sealed record VehiculoRow(Guid Id, string Marca);
public sealed record VehiculoInput(Guid? Id, string Marca);
