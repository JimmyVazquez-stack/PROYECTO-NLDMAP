namespace NLDMAP.Web.Administration.Caching;

public sealed record CachedResponse<T>(T Value, DateTimeOffset LoadedAt, long ChangeVersion = 0);

public sealed class CacheInvalidatedException()
    : Exception("La información cambió durante la consulta. Vuelve a actualizarla.");
