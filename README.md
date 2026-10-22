# Hotel.Web · EF Core Database First

Aplicación ASP.NET Core MVC para consultar habitaciones y gestionar reservas de hotel. Proyecto de la actividad **Semana 10: EF Core Database First**.

## Funciones

- Habitaciones activas con filtro por tipo, detalle, creación, edición y desactivación lógica.
- Búsqueda de habitaciones disponibles por fechas, noches y total estimado.
- Creación de reservas con validaciones de fechas y cruces; listado y cancelación de reservas permitidas.
- Vistas Razor fuertemente tipadas y diseño adaptable a móvil y escritorio.

## Tecnologías

.NET 10, ASP.NET Core MVC, Entity Framework Core 10 y SQL Server. El esquema proviene de `HotelDB.sql`, entregado en la actividad; esta aplicación no usa migraciones ni procedimientos almacenados.

## Preparación

1. Crea `HotelDB` ejecutando el script del docente en SQL Server. **El script elimina y recrea la base si ya existe; comprueba antes si hay datos que conservar.**
2. Configura la conexión en tu equipo. `appsettings.json` contiene el servidor y la base de ejemplo, sin contraseña. Una variable de entorno puede reemplazarla:

   ```zsh
   read -s 'SQL_PASSWORD?Contraseña SQL: '; echo
   export ConnectionStrings__HotelDB="Server=127.0.0.1,1433;Database=HotelDB;User Id=sa;Password=${SQL_PASSWORD};TrustServerCertificate=True"
   unset SQL_PASSWORD
   ```

3. En la misma terminal:

   ```zsh
   dotnet restore
   dotnet build
   dotnet run
   ```

El proyecto incluye las entidades y `HotelContext` generados desde la base. Si cambia el esquema, vuelve a generarlos con:

```zsh
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:HotelDB' Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --no-onconfiguring --force
```

No edites manualmente los archivos generados en `Models`. La columna `Reserva.Observaciones` (`NVARCHAR(200) NULL`) ya forma parte del modelo final; asegúrate de tenerla en tu base de datos.

## Organización

- `Models/`: entidades y contexto generados mediante scaffold.
- `Repositorios/`: consultas LINQ y operaciones de guardado.
- `Controllers/`: acciones MVC y validaciones.
- `Views/` y `wwwroot/css/site.css`: interfaz de usuario.

La documentación de la ejecución local de la actividad está en [CONTINUAR-SEMANA10.md](CONTINUAR-SEMANA10.md).
