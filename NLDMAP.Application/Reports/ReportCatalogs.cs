using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Application.Reports;

public sealed class ReportCatalogs(
    IGeneroStore sexos, IOrientacionSexualStore orientaciones, INacionalidadStore nacionalidades,
    IOcupacionStore ocupaciones, ITezStore tonos, ICircunstanciaStore circunstancias,
    ITipoCabelloStore tiposCabello, ITipoPrendaStore tiposPrenda, ITipoCalzadoStore tiposCalzado,
    ITipoSenaStore tiposSena, IRegionCorporalStore regionesCorporales, IColoresCatalogoStore colores, IParentescoStore parentescos,
    IDiscapacidadStore discapacidades, IComplexionStore complexiones)
{
    internal async Task<ReportCatalogOptions> GetAsync()
    {
        var sex = ReadAsync<GeneroRow>(p => sexos.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreSexo));
        var orientation = ReadAsync<OrientacionSexualRow>(p => orientaciones.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreOrientacionSexual));
        var nationality = ReadAsync<NacionalidadRow>(p => nacionalidades.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreNacionalidad));
        var occupation = ReadAsync<OcupacionRow>(p => ocupaciones.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreOcupacion));
        var tone = ReadAsync<TezRow>(p => tonos.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreColor, r.ColorHex));
        var circumstance = ReadAsync<CircunstanciaRow>(p => circunstancias.GetPageAsync(new(Page: p)), r => new(r.Id, r.Tipo));
        var hair = ReadAsync<TipoCabelloRow>(p => tiposCabello.GetPageAsync(new(Page: p)),
            r => new(r.Id, $"{r.NombreCabello} · {r.LargoCabello}"));
        var clothing = ReadAsync<TipoPrendaRow>(p => tiposPrenda.GetPageAsync(new(Page: p)), r => new(r.Id, r.Nombre));
        var footwear = ReadAsync<TipoCalzadoRow>(p => tiposCalzado.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreCalzado));
        var mark = ReadAsync<TipoSenaRow>(p => tiposSena.GetPageAsync(new(Page: p)), r => new(r.Id, r.Nombre));
        var region = ReadAsync<RegionCorporalRow>(p => regionesCorporales.GetPageAsync(new(Page: p)), r => new(r.Id, r.Region));
        var color = ReadAsync<ColoresCatalogoRow>(p => colores.GetPageAsync(new(Page: p)), r => new(r.Id, r.ColorNombre, r.ColorHex));
        var relationship = ReadAsync<ParentescoRow>(p => parentescos.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreParentesco));
        var disability = ReadAsync<DiscapacidadRow>(p => discapacidades.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreDiscapacidad));
        var build = ReadAsync<ComplexionRow>(p => complexiones.GetPageAsync(new(Page: p)), r => new(r.Id, r.NombreComplexion));
        await Task.WhenAll(sex, orientation, nationality, occupation, tone, circumstance,
            hair, clothing, footwear, mark, region, color, relationship, disability, build);
        return new(await sex, await orientation, await nationality, await occupation, await tone, await circumstance,
            await hair, await clothing, await footwear, await mark, await region, await color, await relationship, await disability, await build);
    }

    private static async Task<IReadOnlyList<ReportCatalogOption>> ReadAsync<T>(
        Func<int, Task<PageResult<T>>> read, Func<T, ReportCatalogOption> map)
    {
        var options = new Dictionary<Guid, ReportCatalogOption>();
        for (var page = 1; ; page++)
        {
            var result = await read(page);
            foreach (var item in result.Items)
            {
                var option = map(item);
                options.TryAdd(option.Id, option);
            }
            if (page >= result.TotalPages)
                return options.Values.ToArray();
        }
    }
}
