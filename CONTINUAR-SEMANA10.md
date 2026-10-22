# Continuar Semana 10

## Estado comprobado

`hotel-sql-limpio` funciona en `127.0.0.1:1433`. El usuario ejecutó `HotelDB.sql` y verificó los conteos iniciales: `TipoHabitacion=4`, `Habitacion=9`, `Huesped=7`, `Reserva=10`. **No vuelvas a ejecutar el script**: elimina y recrea la base.

Se generaron `Models` y `HotelContext` con `dotnet ef dbcontext scaffold`, `--no-onconfiguring` y, después de agregar `Observaciones`, con `--force`. No se editaron manualmente los archivos generados. La columna se verificó como `NVARCHAR(200) NULL`. `dotnet build --no-restore` terminó con cero advertencias y cero errores. Pasaron las pruebas HTTP de lectura, validación de número duplicado, fecha pasada, disponibilidad y formulario de Observaciones. También pasaron altas, edición, baja lógica y cancelación en los repositorios dentro de una transacción revertida; no quedaron registros nuevos.

## Preparar la base y el modelo

Para regenerar el modelo en otra sesión, captura la contraseña en una terminal local de VS Code sin incluirla en el historial:

   ```zsh
   read -s 'SQL_PASSWORD?Contraseña SQL: '; echo
   export ConnectionStrings__HotelDB="Server=127.0.0.1,1433;Database=HotelDB;User Id=sa;Password=${SQL_PASSWORD};TrustServerCertificate=True"
   unset SQL_PASSWORD
   dotnet ef dbcontext scaffold 'Name=ConnectionStrings:HotelDB' Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --no-onconfiguring --force
   dotnet build
   ```

La clave `ConnectionStrings:HotelDB` está en `appsettings.json`; la variable de entorno la completa en esta sesión sin escribir la contraseña en el proyecto. Verifica que las clases generadas estén en `Models` y que el contexto no incluya `OnConfiguring`.

Para iniciar la app usa la misma terminal, que conserva la variable de entorno: `dotnet run`. Las acciones HTTP de guardado válidas aún pueden recorrerse manualmente; las pruebas de guardado de los repositorios ya se ejecutaron con rollback para preservar los datos de prueba.

## Cambio final de esquema ya aplicado

Primero comprueba la columna en `HotelDB`:

```sql
SELECT COL_LENGTH(N'dbo.Reserva', N'Observaciones') AS LongitudObservaciones;
```

La columna ya existe. Si se reproduce la actividad en otra base y la consulta devuelve `NULL`, ejecuta **una vez**:

```sql
ALTER TABLE dbo.Reserva ADD Observaciones NVARCHAR(200) NULL;
```

Luego, en la terminal con `ConnectionStrings__HotelDB` configurada:

```zsh
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:HotelDB' Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --no-onconfiguring --force
dotnet build
```

Comprueba que `Models/Reserva.cs` incluya `Observaciones` y abre el formulario de nueva reserva. No modifiques manualmente las clases generadas.
