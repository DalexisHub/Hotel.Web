using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Hotel.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController(ReservaRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Index() => View(await repositorio.TodasAsync());

    [HttpGet]
    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        var modelo = new DisponibilidadViewModel { Ingreso = ingreso, Salida = salida };
        if (ingreso.HasValue || salida.HasValue)
        {
            if (!ingreso.HasValue || !salida.HasValue || salida <= ingreso)
                ModelState.AddModelError(string.Empty, "Indica un ingreso y una salida posterior.");
            else
            {
                modelo.Noches = salida.Value.DayNumber - ingreso.Value.DayNumber;
                modelo.Habitaciones = await repositorio.DisponiblesAsync(ingreso.Value, salida.Value);
            }
        }
        return View(modelo);
    }

    public async Task<IActionResult> Create(int? habitacionId, DateOnly? ingreso, DateOnly? salida)
    {
        await CargarListasAsync();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var fechaIngreso = ingreso >= hoy ? ingreso.Value : hoy;
        var fechaSalida = salida > fechaIngreso ? salida.Value : fechaIngreso.AddDays(1);
        var habitacionSeleccionada = habitacionId.HasValue && await repositorio.HabitacionActivaAsync(habitacionId.Value)
            ? habitacionId.Value : 0;
        return View(new Reserva { IdHabitacion = habitacionSeleccionada, FechaIngreso = fechaIngreso, FechaSalida = fechaSalida });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida,Observaciones")] Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        ModelState.Remove(nameof(Reserva.Estado));
        ModelState.Remove(nameof(Reserva.FechaRegistro));
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        if (reserva.FechaSalida <= reserva.FechaIngreso)
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La salida debe ser posterior al ingreso.");
        if (reserva.FechaIngreso < hoy)
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "El ingreso no puede ser anterior a hoy.");
        if (!await repositorio.HuespedActivoAsync(reserva.IdHuesped))
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "Selecciona un huésped activo.");
        if (!await repositorio.HabitacionActivaAsync(reserva.IdHabitacion))
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "Selecciona una habitación activa.");
        if (reserva.FechaSalida > reserva.FechaIngreso && reserva.IdHabitacion > 0 &&
            await repositorio.ExisteCruceAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango.");
        if (!ModelState.IsValid)
        {
            await CargarListasAsync();
            return View(reserva);
        }
        await repositorio.CrearAsync(reserva);
        TempData["Mensaje"] = "Reserva creada como Pendiente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (!await repositorio.CancelarAsync(id)) return NotFound();
        TempData["Mensaje"] = "Reserva cancelada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Huespedes = new SelectList(
            (await repositorio.HuespedesActivosAsync()).Select(h => new { h.IdHuesped, Nombre = $"{h.Apellidos}, {h.Nombres}" }),
            "IdHuesped", "Nombre");
        ViewBag.Habitaciones = new SelectList(
            (await repositorio.HabitacionesActivasAsync()).Select(h => new { h.IdHabitacion, Nombre = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre}" }),
            "IdHabitacion", "Nombre");
    }
}
