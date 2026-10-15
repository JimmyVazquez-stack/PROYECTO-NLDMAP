namespace NLDMAP.Web.Administration;

public static class PermissionLabels
{
    private static readonly IReadOnlyDictionary<string, string> Labels =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Users.Read"] = "Consultar cuentas",
            ["Users.Create"] = "Crear cuentas",
            ["Users.Update"] = "Actualizar cuentas",
            ["Users.AssignRoles"] = "Asignar y retirar roles",
            ["Roles.Read"] = "Consultar roles",
            ["Roles.UpdatePermissions"] = "Configurar permisos",
            ["Settings.Read"] = "Consultar configuración",
            ["Settings.Update"] = "Actualizar configuración",
            ["Reports.Create"] = "Crear reportes",
            ["Reports.ReadOwn"] = "Consultar reportes propios",
            ["Reports.RespondToCorrections"] = "Atender correcciones de reportes propios",
            ["Reports.AddOwnUpdates"] = "Comunicar novedades de reportes propios",
            ["Contributions.Create"] = "Registrar avistamientos o información complementaria",
            ["Contributions.ReadOwn"] = "Consultar aportaciones propias",
            ["Reports.ReadAssigned"] = "Consultar reportes asignados",
            ["Contributions.ReadAssigned"] = "Consultar aportaciones asignadas",
            ["Reports.RequestCorrectionsAssigned"] = "Solicitar correcciones de reportes asignados",
            ["Contributions.RequestCorrectionsAssigned"] = "Solicitar correcciones de aportaciones asignadas",
            ["Reports.DecidePublicationAssigned"] = "Decidir publicación de reportes asignados",
            ["Contributions.DecidePublicationAssigned"] = "Decidir publicación de aportaciones asignadas",
            ["Reports.AddJustifiedUpdatesAssigned"] = "Registrar actualizaciones justificadas de reportes asignados",
            ["Information.ReadAuthorized"] = "Consultar información autorizada",
            ["Analysis.RecordObservations"] = "Registrar observaciones de análisis",
            ["Statistics.ReadAuthorized"] = "Consultar estadísticas autorizadas"
        };

    public static string Get(string permission) =>
        Labels.TryGetValue(permission, out var label) ? label : permission;
}
