using Microsoft.AspNetCore.Components;
using NLDMAP.Application.Administration;
namespace NLDMAP.Server.Components.Administration;

public abstract class AdminPageBase : ComponentBase
{
    [Inject] protected AdministrationService Admin { get; set; } = default!;
    [Inject] private ILogger<AdminPageBase> Logger { get; set; } = default!;
    protected AdminAccess? Access { get; set; }
    protected bool Busy { get; private set; }
    protected string? Error { get; private set; }
    protected string? Notice { get; private set; }
    protected override Task OnInitializedAsync() => RunAsync(async () => { Access = await Admin.GetAccessAsync(); await LoadAsync(); });
    protected virtual Task LoadAsync() => Task.CompletedTask;
    protected async Task RunAsync(Func<Task> action, string? success = null)
    {
        if (Busy) return;
        Busy = true; Error = null; Notice = null;
        try { await action(); if (success is not null) Notice = success; }
        catch (AdministrationException ex) { Error = ex.Message; }
        catch (Exception ex) { Logger.LogError(ex, "Error en administración"); Error = "No se pudo completar la operación. Actualiza e inténtalo nuevamente."; }
        finally { Busy = false; }
    }
    protected void SetNotice(string message) => Notice = message;
}
