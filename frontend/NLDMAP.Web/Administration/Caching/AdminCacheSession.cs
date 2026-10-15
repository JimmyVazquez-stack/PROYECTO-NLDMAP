namespace NLDMAP.Web.Administration.Caching;

public sealed class AdminCacheSession : IDisposable
{
    private CancellationTokenSource lifetime = new();
    public long Generation { get; private set; }
    public string? OwnerId { get; private set; }
    public bool ReuseAllowed { get; set; } = true;
    public CancellationToken Token => lifetime.Token;
    public event Action? Resetting;

    public void Reset()
    {
        Generation++;
        OwnerId = null;
        var previous = lifetime;
        lifetime = new();
        previous.Cancel();
        previous.Dispose();
        Resetting?.Invoke();
    }

    public void Establish(string? ownerId) => OwnerId = ownerId;

    public void Dispose()
    {
        lifetime.Cancel();
        lifetime.Dispose();
    }
}
