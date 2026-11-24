using NLDMAP.Application.Administration;
using NLDMAP.Application.Catalogs;

namespace NLDMAP.Server.Components.Pages.Catalogs;

internal sealed record CampoCatalogo(string Clave, string Etiqueta, bool Obligatorio = true,
    bool EsHexadecimal = false, bool EsEntero = false, int Maximo = 120);

internal sealed record FilaCatalogo(Guid Id, IReadOnlyDictionary<string, string> Valores);

internal sealed record CatalogoVista(
    string Ruta, string Nombre, string Descripcion, IReadOnlyList<CampoCatalogo> Campos,
    Func<IServiceProvider, string, int, Task<PageResult<FilaCatalogo>>> GetPageAsync,
    Func<IServiceProvider, Guid?, IReadOnlyDictionary<string, string>, Task> SaveAsync,
    Func<IServiceProvider, Guid, Task> DeleteAsync);

internal static class CatalogoViews
{
    internal static IReadOnlyList<CatalogoVista> Todos { get; } =
    [
        Crear<GeneroService, GeneroRow, GeneroInput>(
            "generos", "Géneros", "Opciones de sexo para la ficha de identificación.",
            (s, search, page) => s.GetPageAsync(new GeneroQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new GeneroInput(id, v["nombreSexo"]),
            r => Fila(r.Id, ("nombreSexo", r.NombreSexo)),
            new CampoCatalogo("nombreSexo", "Nombre del sexo")),
        Crear<OrientacionSexualService, OrientacionSexualRow, OrientacionSexualInput>(
            "orientaciones-sexuales", "Orientaciones sexuales", "Opciones de orientación sexual y sus descripciones.",
            (s, search, page) => s.GetPageAsync(new OrientacionSexualQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new OrientacionSexualInput(id, v["nombreOrientacionSexual"], v["descripcionOrientacionSexual"]),
            r => Fila(r.Id, ("nombreOrientacionSexual", r.NombreOrientacionSexual), ("descripcionOrientacionSexual", r.DescripcionOrientacionSexual)),
            new("nombreOrientacionSexual", "Orientación sexual"),
            new("descripcionOrientacionSexual", "Descripción", Obligatorio: false, Maximo: 500)),
        Crear<CircunstanciaService, CircunstanciaRow, CircunstanciaInput>(
            "circunstancias", "Circunstancias", "Tipos de circunstancias en que ocurrieron los hechos.",
            (s, search, page) => s.GetPageAsync(new CircunstanciaQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new CircunstanciaInput(id, v["tipo"], v["descripcionCircunstancia"]),
            r => Fila(r.Id, ("tipo", r.Tipo), ("descripcionCircunstancia", r.DescripcionCircunstancia)),
            new("tipo", "Tipo"),
            new("descripcionCircunstancia", "Descripción", Obligatorio: false, Maximo: 500)),
        Crear<NacionalidadService, NacionalidadRow, NacionalidadInput>(
            "nacionalidades", "Nacionalidades", "Nacionalidades para identificar a las personas del reporte.",
            (s, search, page) => s.GetPageAsync(new NacionalidadQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new NacionalidadInput(id, v["nombreNacionalidad"]),
            r => Fila(r.Id, ("nombreNacionalidad", r.NombreNacionalidad)),
            new CampoCatalogo("nombreNacionalidad", "Nacionalidad")),
        Crear<ParentescoService, ParentescoRow, ParentescoInput>(
            "parentescos", "Parentescos y vínculos", "Relaciones con la persona reportada, como madre, hermano, pareja o amistad.",
            (s, search, page) => s.GetPageAsync(new ParentescoQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new ParentescoInput(id, v["nombreParentesco"]),
            r => Fila(r.Id, ("nombreParentesco", r.NombreParentesco)),
            new CampoCatalogo("nombreParentesco", "Parentesco o vínculo")),
        Crear<ComplexionService, ComplexionRow, ComplexionInput>(
            "complexiones", "Complexiones", "Opciones para describir la complexión física de la persona reportada.",
            (s, search, page) => s.GetPageAsync(new ComplexionQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new ComplexionInput(id, v["nombreComplexion"]),
            r => Fila(r.Id, ("nombreComplexion", r.NombreComplexion)),
            new CampoCatalogo("nombreComplexion", "Complexión")),
        Crear<DiscapacidadService, DiscapacidadRow, DiscapacidadInput>(
            "discapacidades", "Discapacidades", "Tipos de discapacidad que se pueden registrar en un reporte.",
            (s, search, page) => s.GetPageAsync(new DiscapacidadQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new DiscapacidadInput(id, v["nombreDiscapacidad"]),
            r => Fila(r.Id, ("nombreDiscapacidad", r.NombreDiscapacidad)),
            new CampoCatalogo("nombreDiscapacidad", "Discapacidad")),
        Crear<OcupacionService, OcupacionRow, OcupacionInput>(
            "ocupaciones", "Ocupaciones", "Oficios, profesiones y otras actividades.",
            (s, search, page) => s.GetPageAsync(new OcupacionQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new OcupacionInput(id, v["nombreOcupacion"], v["descripcionOcupacion"]),
            r => Fila(r.Id, ("nombreOcupacion", r.NombreOcupacion), ("descripcionOcupacion", r.DescripcionOcupacion)),
            new("nombreOcupacion", "Ocupación"),
            new("descripcionOcupacion", "Descripción", Obligatorio: false, Maximo: 500)),
        Crear<VehiculoService, VehiculoRow, VehiculoInput>(
            "vehiculos", "Vehículos", "Marcas de vehículos.",
            (s, search, page) => s.GetPageAsync(new VehiculoQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new VehiculoInput(id, v["marca"]),
            r => Fila(r.Id, ("marca", r.Marca)),
            new CampoCatalogo("marca", "Marca")),
        Crear<TipoCabelloService, TipoCabelloRow, TipoCabelloInput>(
            "tipos-cabello", "Tipos de cabello", "Opciones para describir el tipo y el largo del cabello.",
            (s, search, page) => s.GetPageAsync(new TipoCabelloQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new TipoCabelloInput(id, v["nombreCabello"], v["largoCabello"]),
            r => Fila(r.Id, ("nombreCabello", r.NombreCabello), ("largoCabello", r.LargoCabello)),
            new("nombreCabello", "Tipo de cabello"),
            new("largoCabello", "Largo del cabello")),
        Crear<ColoresCatalogoService, ColoresCatalogoRow, ColoresCatalogoInput>(
            "colores", "Colores", "Paleta de colores con nombre y código hexadecimal.",
            (s, search, page) => s.GetPageAsync(new ColoresCatalogoQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new ColoresCatalogoInput(id, v["colorNombre"], v["colorHex"]),
            r => Fila(r.Id, ("colorNombre", r.ColorNombre), ("colorHex", r.ColorHex)),
            new("colorNombre", "Nombre del color"),
            new("colorHex", "Color HEX", EsHexadecimal: true, Maximo: 7)),
        Crear<TipoCalzadoService, TipoCalzadoRow, TipoCalzadoInput>(
            "tipos-calzado", "Tipos de calzado", "Clases de calzado que se pueden registrar en un reporte.",
            (s, search, page) => s.GetPageAsync(new TipoCalzadoQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new TipoCalzadoInput(id, v["nombreCalzado"]),
            r => Fila(r.Id, ("nombreCalzado", r.NombreCalzado)),
            new CampoCatalogo("nombreCalzado", "Tipo de calzado")),
        Crear<TipoPrendaService, TipoPrendaRow, TipoPrendaInput>(
            "tipos-vestimenta", "Tipos de vestimenta", "Clases de prendas para describir la vestimenta.",
            (s, search, page) => s.GetPageAsync(new TipoPrendaQuery(search, page)),
            (s, input) => s.SaveAsync(input), (s, id) => s.DeleteAsync(id),
            (id, v) => new TipoPrendaInput { Id = id, Nombre = v["nombre"] },
            r => Fila(r.Id, ("nombre", r.Nombre)),
            new CampoCatalogo("nombre", "Tipo de vestimenta", Maximo: 80))
    ];

    internal static CatalogoVista? Buscar(string ruta) => Todos.FirstOrDefault(catalogo => catalogo.Ruta == ruta);

    private static CatalogoVista Crear<TService, TRow, TInput>(
        string ruta, string nombre, string descripcion,
        Func<TService, string, int, Task<PageResult<TRow>>> getPage,
        Func<TService, TInput, Task> save, Func<TService, Guid, Task> delete,
        Func<Guid?, IReadOnlyDictionary<string, string>, TInput> input,
        Func<TRow, FilaCatalogo> row, params CampoCatalogo[] campos)
        where TService : notnull =>
        new(ruta, nombre, descripcion, campos,
            async (services, search, page) =>
            {
                var result = await getPage(services.GetRequiredService<TService>(), search, page);
                return new PageResult<FilaCatalogo>(result.Items.Select(row).ToArray(),
                    result.Total, result.Page, result.PageSize);
            },
            (services, id, values) => save(services.GetRequiredService<TService>(), input(id, values)),
            (services, id) => delete(services.GetRequiredService<TService>(), id));

    private static FilaCatalogo Fila(Guid id, params (string Clave, string? Valor)[] campos) =>
        new(id, campos.ToDictionary(campo => campo.Clave, campo => campo.Valor ?? ""));
}
