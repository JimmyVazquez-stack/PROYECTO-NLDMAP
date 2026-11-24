namespace NLDMAP.Domain.Catalogs;

public sealed record AsentamientoPostal(
    string CodigoPostal,
    string Asentamiento,
    string TipoAsentamiento,
    string Municipio,
    string Estado,
    string Ciudad,
    string ClaveEstado,
    string ClaveMunicipio,
    string ClaveAsentamiento,
    string Zona,
    string CodigoPostalAdministracion,
    string ClaveOficina,
    string ClaveTipoAsentamiento,
    string ClaveCiudad)
{
    public string Clave => $"{ClaveEstado}:{ClaveMunicipio}:{ClaveAsentamiento}";
}
