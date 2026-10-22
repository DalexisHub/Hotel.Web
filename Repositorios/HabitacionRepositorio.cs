using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio(HotelContext context)
{
    public async Task<List<TipoHabitacion>> TiposActivosAsync() =>
        await context.Set<TipoHabitacion>().AsNoTracking()
            .Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync();

    public async Task<List<Habitacion>> ActivasAsync(int? tipoId) =>
        await context.Set<Habitacion>().AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo && (!tipoId.HasValue || h.IdTipoHabitacion == tipoId.Value))
            .OrderBy(h => h.Numero).ToListAsync();

    public async Task<Habitacion?> PorIdAsync(int id) =>
        await context.Set<Habitacion>().AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id);

    public async Task<bool> NumeroExisteAsync(string numero, int? excluirId = null) =>
        await context.Set<Habitacion>().AsNoTracking()
            .AnyAsync(h => h.Numero == numero && (!excluirId.HasValue || h.IdHabitacion != excluirId.Value));

    public async Task<bool> TipoActivoAsync(int id) =>
        await context.Set<TipoHabitacion>().AsNoTracking().AnyAsync(t => t.IdTipoHabitacion == id && t.Activo);

    public async Task CrearAsync(Habitacion habitacion)
    {
        habitacion.Activo = true;
        context.Set<Habitacion>().Add(habitacion);
        await context.SaveChangesAsync();
    }

    public async Task<bool> EditarAsync(Habitacion entrada)
    {
        var actual = await context.Set<Habitacion>().FirstOrDefaultAsync(h => h.IdHabitacion == entrada.IdHabitacion);
        if (actual is null || !actual.Activo) return false;
        actual.Numero = entrada.Numero;
        actual.Piso = entrada.Piso;
        actual.IdTipoHabitacion = entrada.IdTipoHabitacion;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DesactivarAsync(int id)
    {
        var actual = await context.Set<Habitacion>().FirstOrDefaultAsync(h => h.IdHabitacion == id);
        if (actual is null || !actual.Activo) return false;
        actual.Activo = false;
        await context.SaveChangesAsync();
        return true;
    }
}
