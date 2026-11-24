namespace NLDMAP.Application.Catalogs;

public sealed record ColoresCatalogoQuery(string? Search = null, int Page = 1);
public sealed record ColoresCatalogoRow(Guid Id, string ColorNombre, string ColorHex);
public sealed record ColoresCatalogoInput(Guid? Id, string ColorNombre, string ColorHex);
