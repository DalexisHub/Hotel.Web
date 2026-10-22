using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio(HotelContext context)
{
    public async Task<List<Reserva>> TodasAsync() =>
        await context.Set<Reserva>().AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaIngreso).ToListAsync();

    public async Task<List<Huesped>> HuespedesActivosAsync() =>
        await context.Set<Huesped>().AsNoTracking().Where(h => h.Activo)
            .OrderBy(h => h.Apellidos).ThenBy(h => h.Nombres).ToListAsync();

    public async Task<List<Habitacion>> HabitacionesActivasAsync() =>
        await context.Set<Habitacion>().AsNoTracking().Where(h => h.Activo)
            .Include(h => h.IdTipoHabitacionNavigation).OrderBy(h => h.Numero).ToListAsync();

    public async Task<bool> HuespedActivoAsync(int id) =>
        await context.Set<Huesped>().AsNoTracking().AnyAsync(h => h.IdHuesped == id && h.Activo);

    public async Task<bool> HabitacionActivaAsync(int id) =>
        await context.Set<Habitacion>().AsNoTracking().AnyAsync(h => h.IdHabitacion == id && h.Activo);

    public async Task<List<Habitacion>> DisponiblesAsync(DateOnly ingreso, DateOnly salida) =>
        await context.Set<Habitacion>().AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo && !context.Set<Reserva>().Any(r =>
                r.IdHabitacion == h.IdHabitacion && r.Estado != "Cancelada" &&
                r.FechaIngreso < salida && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero).ToListAsync();

    public async Task<bool> ExisteCruceAsync(int habitacionId, DateOnly ingreso, DateOnly salida) =>
        await context.Set<Reserva>().AsNoTracking().AnyAsync(r =>
            r.IdHabitacion == habitacionId && r.Estado != "Cancelada" &&
            r.FechaIngreso < salida && r.FechaSalida > ingreso);

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        context.Set<Reserva>().Add(reserva);
        await context.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await context.Set<Reserva>().FirstOrDefaultAsync(r => r.IdReserva == id);
        if (reserva is null || reserva.Estado is not ("Pendiente" or "Confirmada")) return false;
        reserva.Estado = "Cancelada";
        await context.SaveChangesAsync();
        return true;
    }
}
