# Bandeja de reportes — primera implementación visual

## Alcance

Ruta: `/gestion/reportes`. Vista InteractiveServer con MudBlazor y datos ficticios en memoria. No hay escrituras, seeders, endpoints de reportes, conexión a MongoDB/Neo4j ni nuevos permisos. Se conserva la política existente `Administration.Access` como puerta provisional a la demo; no representa la autorización final de Gestor.

## Cómo abrirla

1. Conservar la configuración local existente de PostgreSQL e Identity.
2. Ejecutar desde la raíz: `dotnet run --project NLDMAP.Server`.
3. Abrir la dirección HTTPS indicada en la terminal e iniciar sesión con una cuenta institucional existente.
4. Entrar a **Bandeja de reportes · Demo** en el menú de administración, o navegar a `/gestion/reportes`.

No activar `MigrateDatabase`, `SeedRoles` o `SeedIdentity` para probar esta pantalla: no necesita migraciones ni inserciones. El inicio de sesión sí utiliza la infraestructura existente.

## Archivos y responsabilidades

- `Components/Layout/OperationsLayout.razor`: cabecera y navegación. Solo Inicio y Reportes tienen destinos; los módulos posteriores están deshabilitados.
- `Components/Pages/Reports/ReportInbox.razor`: formulario, pestañas, tabla y paginación.
- `ReportInbox.razor.cs`: estado local, búsqueda, ordenación y selección. La etapa operativa no se deduce del estado de revisión.
- `ReportInbox.razor.css`: presentación adaptable y colores.
- `Components/Reports/Models/ReportInboxRow.cs`: modelo exclusivamente visual.
- `Components/Reports/Preview/ReportInboxSampleData.cs`: seis ejemplos inmutables; dos por bandeja.
- `Components/Reports/ReportPreview.razor`: detalle inline de solo lectura. No simula una revisión válida.
- `AdministrationLayout.razor`: enlace de acceso a la bandeja.

## Comprobación manual

1. Sin sesión, la ruta no debe mostrar la bandeja. Mantener el acceso existente.
2. Al entrar aparecen dos reportes nuevos y conteos 2/2/2.
3. Buscar `DEMO-003`: los conteos pasan a 0/1/0. Seleccionar En proceso muestra C.
4. Buscar un texto inexistente: mostrar vacío, sin filas anteriores ni detalle seleccionado.
5. Limpiar la búsqueda y pulsar Buscar: restablecer resultados.
6. Cambiar el orden invierte las fechas. Cambiar Por página a 1 permite Anterior/Siguiente.
7. Cambiar filtro, pestaña o tamaño devuelve a la primera página.
8. Ver reporte abre el detalle y le da foco; Cerrar lo retira. No cambia estados ni conteos.
9. Actualizar consulta conserva filtros y recarga los ejemplos; no consulta una base de reportes.
10. Revisar a 390px y en escritorio: filtros apilados, tabla adaptable y navegación sin enlaces rotos.

## Próxima etapa

Sustituir los ejemplos por un servicio de consultas, definir permisos y ámbito del Gestor, implementar responsable/inicio de revisión y solicitudes pendientes. No reutilizar automáticamente el acceso administrativo a contenido sensible. El contrato visual no es una entidad persistente.
