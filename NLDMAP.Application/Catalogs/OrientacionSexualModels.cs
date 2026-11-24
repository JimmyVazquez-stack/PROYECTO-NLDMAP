namespace NLDMAP.Application.Catalogs;

public sealed record OrientacionSexualQuery(string? Search = null, int Page = 1);
public sealed record OrientacionSexualRow(Guid Id, string NombreOrientacionSexual, string? DescripcionOrientacionSexual);
public sealed record OrientacionSexualInput(Guid? Id, string NombreOrientacionSexual, string? DescripcionOrientacionSexual);
