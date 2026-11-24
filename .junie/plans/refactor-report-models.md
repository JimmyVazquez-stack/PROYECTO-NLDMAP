---
sessionId: session-261123-205204-1lrs
---

# Requirements

### Overview
`Reporte.cs` contiene todos los tipos de reporte en un solo archivo. `CatalogosReporte` agrupa valores de catálogo (sexo, orientación, tez, circunstancia, municipio, etc.) que pertenecen a secciones distintas del reporte. El objetivo es separar cada tipo en su propio archivo y repartir los valores de `CatalogosReporte` en las secciones correspondientes, mientras MongoDB sigue guardando un solo documento por reporte.

### Goals
- Cada modelo de Domain en su propio archivo en `NLDMAP.Domain/Reports/`.
- Los valores de catálogo residen en la sección que les corresponde (no en `CatalogosReporte`).
- Los nuevos reportes guardan los valores capturados directamente, sin UUID de catálogo en el BSON.
- Un reporte antiguo con referencias sigue siendo legible mediante compatibilidad existente.
- Cambiar una etiqueta del catálogo conserva el valor capturado en reportes ya guardados.
- Una selección inválida se rechaza antes de guardar.

### Scope

**In Scope**
- Separar tipos de `Reporte.cs` en archivos individuales.
- Repartir propiedades de `CatalogosReporte` en `HechosReporte`, `DescripcionPersonaReporte`, `UbicacionReporte` y `SenaParticularReporte`.
- Eliminar `CatalogosReporte` de Domain cuando todos los consumidores estén adaptados.
- Actualizar `ReporteService` para construir las secciones con valores de catálogo.
- Actualizar `MongoReporteDocument` y `MongoReporteLectura` para la nueva estructura.
- Adaptar `MongoMigracionCatalogosReportes` y `MongoReporteCatalogosLegados`.
- Adaptar `ClasificacionReporteAdmin` y consumidores en Server.

**Out of Scope**
- Cambios en la estructura de colecciones de MongoDB.
- Modificaciones al flujo de autenticación o permisos.
- Cambios en la forma BSON del subdocumento `catalogos` (se conserva para compatibilidad).

### Acceptance Criteria
- `dotnet build NLDMAP.slnx` compila sin errores.
- Cada modelo de Domain está en su propio archivo.
- Los nuevos reportes guardan los valores capturados sin UUID de catálogo en el BSON.
- Una ficha nueva y una migrada se cargan con una sola consulta a reportes.
- Un reporte antiguo con referencias sigue siendo legible.
- Cambiar una etiqueta del catálogo conserva el valor capturado.
- Una selección inválida se rechaza antes de guardar.

# Technical Design

### Current Implementation

**Domain layer** (`NLDMAP.Domain/Reports/Reporte.cs`):
- `Reporte` contiene `CatalogosReporte` como propiedad central.
- `CatalogosReporte` agrupa: Sexo, OrientacionSexual, Nacionalidad, Ocupacion, Tez, Circunstancia, Municipio, RegionCorporal, TipoSena.
- Estos valores pertenecen a: `HechosReporte` (circunstancia), `DescripcionPersonaReporte` (sexo, orientación, nacionalidad, ocupación, tez), `UbicacionReporte` (municipio), `SenaParticularReporte` (región corporal, tipo de seña).

**Application layer** (`ReporteService.cs`, `ReporteModels.cs`):
- `CrearReporteInput` recibe UUIDs de catálogo (IdSexo, IdTez, etc.) y las secciones de Domain directamente.
- `ReporteService.CapturarCatalogosAsync` resuelve los UUIDs a nombres y construye `CatalogosReporte`.

**Infrastructure layer**:
- `MongoReporteDocument.From()` serializa `CatalogosReporte` a un subdocumento `catalogos` en BSON.
- `MongoReporteLectura` lee `catalogos` y reconstruye `CatalogosReporte`.
- `ClasificacionReporteAdmin` usa los valores de `CatalogosReporte` para mostrar detalles.
- `MongoMigracionCatalogosReportes` y `MongoReporteCatalogosLegados` migran reportes antiguos.

**Server layer**:
- `CreateReport.razor.cs` construye `CrearReporteInput` con UUIDs y secciones.
- `MobileReportsController` expone la API.
- `ReporteAdminFicha.razor.cs` consume `ReporteAdminDetalle`.

### Key Decisions

1. **Los valores de catálogo se mueven a las secciones correspondientes**: Sexo, OrientacionSexual, Nacionalidad, Ocupacion y Tez van a `DescripcionPersonaReporte`. Circunstancia va a `HechosReporte`. Municipio va a `UbicacionReporte`. RegionCorporal y TipoSena van a `SenaParticularReporte`.

2. **Se conserva el subdocumento `catalogos` en BSON**: Para compatibilidad con reportes existentes y migración. Se mantiene un tipo interno en Infrastructure para representar la lectura de `catalogos` en BSON.

3. **`CrearReporteInput` conserva los UUIDs de catálogo**: El contrato JSON de la API no cambia. `ReporteService` sigue validando y resolviendo UUIDs a valores oficiales.

4. **`ClasificacionReporteAdmin` se construye desde las secciones**: En lugar de leer `CatalogosReporte`, se extraen los valores de las secciones donde ahora residen.

### Proposed Changes

#### Step 1: Separate Domain types into individual files
- `EstadoRevisionReporte.cs` — código y nombre.
- `TezCapturadaReporte.cs` — nombre y color hexadecimal.
- `HechosReporte.cs` — descripción, fechas, hora, **circunstancia**.
- `SenaParticularReporte.cs` — tipo de seña, región corporal, descripción.
- `DescripcionPersonaReporte.cs` — identificación, rasgos, **sexo, orientación, nacionalidad, ocupación, tez**.
- `UbicacionReporte.cs` — estado, **municipio**, código postal, asentamiento, calle, referencias.
- `VehiculoReporte.cs` — marca, placa, color, modelo.
- `PersonaInvolucradaReporte.cs` — nombre, descripción.
- `Reporte.cs` — identidad, folio, fecha, autor, estados, consentimiento, anonimato, concurrencia, secciones. **Sin CatalogosReporte**.

#### Step 2: Move catalog values to section models
- `HechosReporte` gains `string? Circunstancia`.
- `DescripcionPersonaReporte` gains `string? Sexo`, `string? OrientacionSexual`, `string? Nacionalidad`, `string? Ocupacion`, `TezCapturadaReporte? Tez`.
- `UbicacionReporte` gains `string? Municipio`.
- `SenaParticularReporte` gains `string? RegionCorporal`, `string? TipoSena`.

#### Step 3: Update ReporteService
- `CapturarCatalogosAsync` se elimina. En su lugar, cada sección se construye con sus valores de catálogo directamente.
- `NormalizarHechos` añade la circunstancia.
- `NormalizarPersona` añade sexo, orientación, nacionalidad, ocupación, tez.
- `NormalizarUbicacion` añade municipio.
- `SenaParticularReporte` se construye con región y tipo de seña.

#### Step 4: Update Infrastructure
- `MongoReporteDocument.From()` serializa los valores de catálogo desde las secciones donde residen.
- `MongoReporteLectura` lee los valores desde las secciones.
- Se conserva un tipo interno `CatalogosBson` para leer el subdocumento `catalogos` en reportes antiguos.
- `MongoMigracionCatalogosReportes` y `MongoReporteCatalogosLegados` se adaptan.
- `ClasificacionReporteAdmin` se construye desde las secciones.

#### Step 5: Update Server consumers
- `CreateReport.razor.cs`: sin cambios en la construcción de `CrearReporteInput`.
- `MobileReportsController`: sin cambios en el contrato.
- `ReporteAdminFicha.razor.cs`: sin cambios en el consumo.

### Architecture Diagram

``` mermaid
graph LR
  A[CreateReport.razor.cs] -->|CrearReporteInput| B[ReporteService]
  C[MobileReportsController] -->|CrearReporteInput| B
  B -->|valida UUIDs| D[CatalogoReferenciaStore]
  B -->|valida UUIDs| E[TezStore]
  B -->|valida UUIDs| F[RegionCorporalStore]
  B -->|valida UUIDs| G[TipoSenaStore]
  B -->|Reporte con valores| H[MongoReporteStore]
  H -->|BSON| I[(MongoDB reportes)]
  J[MongoConsultaReportesAdminStore] -->|lee BSON| I
  J -->|ReporteAdminDetalle| K[ReporteAdminFicha.razor.cs]
```

### File Structure

**New files in NLDMAP.Domain/Reports/**:
- `EstadoRevisionReporte.cs`
- `TezCapturadaReporte.cs`
- `HechosReporte.cs`
- `SenaParticularReporte.cs`
- `DescripcionPersonaReporte.cs`
- `UbicacionReporte.cs`
- `VehiculoReporte.cs`
- `PersonaInvolucradaReporte.cs`

**Modified files**:
- `NLDMAP.Domain/Reports/Reporte.cs` — elimina tipos movidos, elimina `CatalogosReporte`.
- `NLDMAP.Application/Reports/ReporteService.cs` — reemplaza `CapturarCatalogosAsync` por integración directa en normalizadores.
- `NLDMAP.Infrastructure/Reports/MongoReporteDocument.cs` — serializa valores desde secciones.
- `NLDMAP.Infrastructure/Reports/MongoReporteLectura.cs` — lee valores desde secciones, tipo interno para BSON legacy.
- `NLDMAP.Infrastructure/Reports/MongoMigracionCatalogosReportes.cs` — adapta migración.
- `NLDMAP.Infrastructure/Reports/MongoReporteCatalogosLegados.cs` — adapta lectura legacy.
- `NLDMAP.Infrastructure/Reports/MongoConsultaReportesAdminStore.cs` — construye `ClasificacionReporteAdmin` desde secciones.
- `NLDMAP.Application/Reports/ConsultaReportesAdmin.cs` — `ClasificacionReporteAdmin` se construye desde secciones.

### Data Models

**HechosReporte** (adds circunstancia):
```csharp
public sealed record HechosReporte(
    string? DescripcionHechos,
    DateOnly? FechaHechos = null,
    DateOnly? FechaPercato = null,
    TimeOnly? HoraHechos = null,
    string? Circunstancia = null);
```

**DescripcionPersonaReporte** (adds catalog values):
```csharp
public sealed record DescripcionPersonaReporte(
    string? Nombres = null,
    string? PrimerApellido = null,
    string? SegundoApellido = null,
    string? DescripcionIdentificativa = null,
    DateOnly? FechaNacimiento = null,
    int? EdadAproximada = null,
    string? Curp = null,
    string? UrlFoto = null,
    decimal? Estatura = null,
    decimal? Peso = null,
    string? Apodos = null,
    string? Cabello = null,
    string? Calzado = null,
    string? Vestimenta = null,
    string? Sexo = null,
    string? OrientacionSexual = null,
    string? Nacionalidad = null,
    string? Ocupacion = null,
    TezCapturadaReporte? Tez = null,
    SenaParticularReporte? SenaParticular = null);
```

**UbicacionReporte** (adds municipio):
```csharp
public sealed record UbicacionReporte(
    string? CodigoPostal = null,
    string? Estado = null,
    Guid? IdMunicipio = null,
    string? Municipio = null,
    string? Asentamiento = null,
    string? Calle = null,
    string? Referencias = null);
```

**SenaParticularReporte** (adds region and tipo values):
```csharp
public sealed record SenaParticularReporte(
    Guid? IdRegionCorporal = null,
    Guid? IdTipoSena = null,
    string? Descripcion = null,
    string? RegionCorporal = null,
    string? TipoSena = null);
```

**Reporte** (removes CatalogosReporte):
```csharp
public sealed record Reporte(
    Guid Id,
    string FolioInterno,
    DateTime FechaRegistroUtc,
    Guid IdUsuario,
    EstadoRevisionReporte Revision,
    HechosReporte Hechos,
    DescripcionPersonaReporte DescripcionPersona,
    UbicacionReporte Ubicacion,
    VehiculoReporte? Vehiculo,
    PersonaInvolucradaReporte? PersonaInvolucrada,
    bool Consentimiento,
    bool Anonimo,
    Guid ClaveConcurrencia);
```

### Risks
- **Orden de parámetros en records**: Al añadir propiedades a `DescripcionPersonaReporte` y `HechosReporte`, los constructores posicionales requieren que todos los consumidores usen nombres de parámetros. Se usarán nombres explícitos.
- **Compatibilidad BSON legacy**: Los reportes antiguos sin las nuevas propiedades en las secciones deben seguir siendo legibles. `MongoReporteLectura` debe manejar valores nulos.
- **Migración**: `MongoReporteCatalogosLegados` debe seguir funcionando para reportes que aún tienen UUIDs en lugar de valores.

# Testing

### Validation Approach
Después de cada etapa, ejecutar `dotnet build NLDMAP.slnx` para verificar que la solución compila sin errores.

### Key Scenarios
1. **Separación de tipos**: Después de mover cada tipo a su archivo, la solución compila sin cambios en comportamiento.
2. **Reparto de CatalogosReporte**: `ReporteService` construye secciones con valores de catálogo. `MongoReporteDocument` serializa correctamente.
3. **Compatibilidad legacy**: `MongoReporteLectura` lee reportes antiguos con el subdocumento `catalogos` y reportes nuevos con valores en secciones.
4. **ClasificacionReporteAdmin**: Se construye desde las secciones donde residen los valores.

### Edge Cases
- Un reporte antiguo sin `catalogos` ni valores en secciones sigue siendo legible.
- `SenaParticularReporte` nulo no causa errores al leer región o tipo de seña.
- `CrearReporteInput` con UUID inválido es rechazado por `ReporteService`.
- `TezCapturadaReporte` nulo se serializa como `BsonNull`.

### Test Changes
- Se ejecutará `dotnet build NLDMAP.slnx` al final para verificar compilación.
- No se agregarán tests nuevos; se verifica que los existentes (si los hay) no se rompen.

# Delivery Steps

### ✓ Step 1: Separar tipos de Domain en archivos individuales
Cada tipo de reporte se mueve a su propio archivo en `NLDMAP.Domain/Reports/`, manteniendo namespace y firmas actuales.
- Crear `EstadoRevisionReporte.cs` con `EstadoRevisionReporte`.
- Crear `TezCapturadaReporte.cs` con `TezCapturadaReporte`.
- Crear `HechosReporte.cs` con `HechosReporte`.
- Crear `SenaParticularReporte.cs` con `SenaParticularReporte`.
- Crear `DescripcionPersonaReporte.cs` con `DescripcionPersonaReporte`.
- Crear `UbicacionReporte.cs` con `UbicacionReporte`.
- Crear `VehiculoReporte.cs` con `VehiculoReporte`.
- Crear `PersonaInvolucradaReporte.cs` con `PersonaInvolucradaReporte`.
- Dejar `Reporte.cs` solo con `Reporte`.
- Ejecutar `dotnet build NLDMAP.slnx` para verificar compilación.

### ✓ Step 2: Repartir propiedades de CatalogosReporte en las secciones correspondientes
Los valores de catálogo se mueven a las secciones donde pertenecen y se elimina `CatalogosReporte` de Domain.
- `HechosReporte` gana `string? Circunstancia`.
- `DescripcionPersonaReporte` gana `Sexo`, `OrientacionSexual`, `Nacionalidad`, `Ocupacion`, `Tez`.
- `UbicacionReporte` gana `string? Municipio`.
- `SenaParticularReporte` gana `RegionCorporal` y `TipoSena`.
- `Reporte` elimina la propiedad `Catalogos`.
- Eliminar `CatalogosReporte` de `Reporte.cs`.
- Actualizar `ReporteService.CrearAsync` para pasar secciones sin `CatalogosReporte`.
- Actualizar `CrearReporteInput` si es necesario.
- Ejecutar `dotnet build NLDMAP.slnx`.

### ✓ Step 3: Actualizar ReporteService para construir secciones con valores de catálogo
ReporteService integra los valores de catálogo directamente en cada sección en lugar de construir `CatalogosReporte`.
- Eliminar `CapturarCatalogosAsync`.
- Integrar la resolución de UUIDs en `NormalizarHechosAsync` (circunstancia), `NormalizarPersonaAsync` (sexo, orientación, nacionalidad, ocupación, tez), `NormalizarUbicacionAsync` (municipio), y la construcción de `SenaParticularReporte` (región, tipo).
- Hacer los normalizadores `async` y aceptar `CancellationToken`.
- Inyectar los stores de catálogo en `ReporteService`.
- Ejecutar `dotnet build NLDMAP.slnx`.

### ✓ Step 4: Actualizar Infrastructure: serialización, lectura y migración
MongoReporteDocument, MongoReporteLectura y los tipos de migración se adaptan a la nueva estructura.
- `MongoReporteDocument.From()` serializa valores de catálogo desde las secciones donde residen.
- `MongoReporteLectura` lee valores desde las secciones; añade un tipo interno para leer `catalogos` en BSON legacy.
- `MongoMigracionCatalogosReportes` y `MongoReporteCatalogosLegados` se adaptan.
- `MongoConsultaReportesAdminStore` construye `ClasificacionReporteAdmin` desde las secciones.
- `ClasificacionReporteAdmin` se construye desde las secciones en lugar de `CatalogosReporte`.
- Ejecutar `dotnet build NLDMAP.slnx`.

### ✓ Step 5: Verificar consumidores en Server y compilación final
Verificar que los consumidores en Server funcionan con la nueva estructura y compilar la solución completa.
- Revisar `CreateReport.razor.cs`: el contrato de `CrearReporteInput` se conserva.
- Revisar `MobileReportsController`: sin cambios en el contrato JSON.
- Revisar `ReporteAdminFicha.razor.cs`: `ReporteAdminDetalle` se construye correctamente.
- Ejecutar `dotnet build NLDMAP.slnx` y resumir archivos cambiados y verificaciones.