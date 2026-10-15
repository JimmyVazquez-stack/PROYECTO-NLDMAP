using Microsoft.JSInterop;

namespace NLDMAP.Web.Administration.Caching;

public sealed class AdminDataChangeNotifier(IJSRuntime javascript) : IAsyncDisposable
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private Task? startup;
    private IJSObjectReference? module;
    private DotNetObjectReference<AdminDataChangeNotifier>? reference;
    private string? knownRevision;
    private bool disposed;

    public long Version { get; private set; }
    public bool IsAvailable { get; private set; } = true;
    public string? WarningMessage { get; private set; }
    public event Action? Changed;

    public async Task CheckForUpdatesAsync()
    {
        await EnsureStartedAsync();
        if (disposed || module is null) { return; }
        await gate.WaitAsync();
        try
        {
            var revision = await module.InvokeAsync<string>("getRevision");
            IsAvailable = true;
            if (knownRevision is not null && knownRevision != revision)
            {
                knownRevision = revision;
                Version++;
                // Clear stores before another check can accept the new revision.
                Changed?.Invoke();
            }
            else { knownRevision = revision; }
        }
        catch (JSException) { MarkUnavailable(); }
        finally { gate.Release(); }
    }

    // A failed browser notification must never turn a confirmed API save into a failed save.
    public async Task PublishAsync()
    {
        await EnsureStartedAsync();
        if (disposed || module is null) { MarkUnavailable(); return; }
        await gate.WaitAsync();
        try
        {
            // Observe a concurrent revision before replacing it with our own publication.
            var current = await module.InvokeAsync<string>("getRevision");
            if (knownRevision is not null && knownRevision != current)
            {
                knownRevision = current;
                Version++;
                Changed?.Invoke();
            }
            knownRevision = await module.InvokeAsync<string>("publish");
            IsAvailable = true;
            WarningMessage = null;
        }
        catch (JSException) { MarkUnavailable(); }
        finally { gate.Release(); }
    }

    [JSInvokable]
    public Task OnBrowserDataChanged() => CheckForUpdatesAsync();

    private Task EnsureStartedAsync() => startup ??= StartAsync();

    private async Task StartAsync()
    {
        try
        {
            module = await javascript.InvokeAsync<IJSObjectReference>("import", "./js/adminDataSync.js");
            reference = DotNetObjectReference.Create(this);
            IsAvailable = await module.InvokeAsync<bool>("start", reference);
            if (!IsAvailable) { MarkUnavailable(); }
        }
        catch (JSException) { MarkUnavailable(); }
    }

    private void MarkUnavailable()
    {
        IsAvailable = false;
        WarningMessage = "No se pudieron sincronizar los avisos de datos entre pestañas. Usa Actualizar para consultar la información. Los cambios confirmados por la API siguen guardados.";
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (startup is not null) { await startup; }
        await gate.WaitAsync();
        try
        {
            if (module is not null)
            {
                try
                {
                    await module.InvokeVoidAsync("stop");
                    await module.DisposeAsync();
                }
                catch (JSException) { }
            }
            reference?.Dispose();
        }
        finally { gate.Release(); }
    }
}
