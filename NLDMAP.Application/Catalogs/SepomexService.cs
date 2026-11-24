using System.Text;
using NLDMAP.Application.Administration;
using NLDMAP.Application.Security;
using NLDMAP.Domain.Catalogs;

namespace NLDMAP.Application.Catalogs;

public sealed class SepomexService(
    ICurrentActor currentActor, IAdministrationStore administration, ISepomexStore store)
{
    public const int MaxFileBytes = 32 * 1024 * 1024;

    public async Task<PageResult<AsentamientoPostal>> GetPageAsync(string? search, int page)
    {
        await AuthorizeAsync(write: false);
        var text = (search ?? "").Trim();
        return await store.GetPageAsync(text.Length > 100 ? text[..100] : text, page);
    }

    public async Task<ResultadoImportacionPostal> ImportAsync(Stream file)
    {
        var actor = await AuthorizeAsync(write: true);
        var rows = await LeerArchivoAsync(file);
        return await store.ImportAsync(rows, actor.UserId);
    }

    internal static async Task<IReadOnlyList<AsentamientoPostal>> LeerArchivoAsync(Stream file)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[65536];
        int read;
        while ((read = await file.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > MaxFileBytes)
                throw new AdministrationException("El TXT supera el tamaño máximo de 32 MB.");
            buffer.Write(chunk, 0, read);
        }

        var bytes = buffer.GetBuffer().AsSpan(0, (int)buffer.Length);
        string text;
        try
        {
            text = new UTF8Encoding(false, true).GetString(bytes);
        }
        catch (DecoderFallbackException)
        {
            text = CodePagesEncodingProvider.Instance.GetEncoding(1252)!.GetString(bytes);
        }

        using var reader = new StringReader(text);
        return LeerFilas(reader);
    }

    private static IReadOnlyList<AsentamientoPostal> LeerFilas(TextReader reader)
    {
        string[] columns =
        [
            "d_codigo", "d_asenta", "d_tipo_asenta", "D_mnpio", "d_estado", "d_ciudad", "d_CP",
            "c_estado", "c_oficina", "c_tipo_asenta", "c_mnpio", "id_asenta_cpcons", "d_zona", "c_cve_ciudad"
        ];
        var rows = new Dictionary<string, AsentamientoPostal>(StringComparer.Ordinal);
        int[]? positions = null;
        var columnCount = 0;
        var lineNumber = 0;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            line = line.Trim().TrimStart('\uFEFF');
            if (line.Length == 0)
                continue;

            if (positions is null)
            {
                // El TXT nacional incluye un aviso antes del encabezado.
                if (!line.StartsWith("d_codigo|", StringComparison.OrdinalIgnoreCase))
                    continue;

                var headers = line.Split('|', StringSplitOptions.TrimEntries);
                columnCount = headers.Length;
                positions = columns.Select(name =>
                    Array.FindIndex(headers, header => header.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .ToArray();
                if (positions.Contains(-1) || headers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != columnCount)
                    throw new AdministrationException("El encabezado del TXT no coincide con el formato de SEPOMEX.");
                continue;
            }

            var values = line.Split('|', StringSplitOptions.TrimEntries);
            if (values.Length != columnCount)
                throw InvalidRow(lineNumber);

            var row = positions.Select(index => values[index]).ToArray();
            if (!Digits(row[0], 5) || !Digits(row[7], 2) || !Digits(row[10], 3) ||
                !Digits(row[11], 4) || row[1].Length == 0 || row[2].Length == 0 ||
                row[3].Length == 0 || row[4].Length == 0)
                throw InvalidRow(lineNumber);

            var entry = new AsentamientoPostal(row[0], row[1], row[2], row[3], row[4], row[5], row[7],
                row[10], row[11], row[12], row[6], row[8], row[9], row[13]);
            if (!rows.TryAdd(entry.Clave, entry) && rows[entry.Clave] != entry)
                throw new AdministrationException($"La línea {lineNumber} repite una clave de asentamiento con datos distintos.");
        }

        if (rows.Count == 0)
            throw new AdministrationException("El TXT debe contener el encabezado de SEPOMEX y al menos un asentamiento.");
        return rows.Values.ToArray();
    }

    private static bool Digits(string value, int length) =>
        value.Length == length && value.All(char.IsAsciiDigit);

    private static AdministrationException InvalidRow(int lineNumber) =>
        new($"La línea {lineNumber} del TXT de SEPOMEX tiene datos inválidos.");

    private async Task<ActorSession> AuthorizeAsync(bool write)
    {
        var actor = await currentActor.GetAsync();
        var access = await administration.GetAccessAsync(actor);
        AdministrationRules.RequirePermission(access, PermissionNames.CatalogsRead);
        if (write)
            AdministrationRules.RequirePermission(access, PermissionNames.CatalogsManage);
        return actor;
    }
}
