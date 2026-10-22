using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController(HabitacionRepositorio repositorio) : Controller
{
    public async Task<IActionResult> Index(int? tipoId)
    {
        await CargarTiposAsync(tipoId);
        return View(await repositorio.ActivasAsync(tipoId));
    }

    public async Task<IActionResult> Details(int id)
    {
        var habitacion = await repositorio.PorIdAsync(id);
        return habitacion is null ? NotFound() : View(habitacion);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTiposAsync();
        return View(new Habitacion { Activo = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        LimpiarNavegacion();
        await ValidarAsync(habitacion);
        if (!ModelState.IsValid)
        {
            await CargarTiposAsync();
            return View(habitacion);
        }
        await repositorio.CrearAsync(habitacion);
        TempData["Mensaje"] = "Habitación creada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var habitacion = await repositorio.PorIdAsync(id);
        if (habitacion is null || !habitacion.Activo) return NotFound();
        await CargarTiposAsync();
        return View(habitacion);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipoHabitacion")] Habitacion habitacion)
    {
        if (id != habitacion.IdHabitacion) return BadRequest();
        LimpiarNavegacion();
        await ValidarAsync(habitacion, id);
        if (!ModelState.IsValid)
        {
            await CargarTiposAsync();
            return View(habitacion);
        }
        if (!await repositorio.EditarAsync(habitacion)) return NotFound();
        TempData["Mensaje"] = "Habitación actualizada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var habitacion = await repositorio.PorIdAsync(id);
        return habitacion is null || !habitacion.Activo ? NotFound() : View(habitacion);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (!await repositorio.DesactivarAsync(id)) return NotFound();
        TempData["Mensaje"] = "Habitación desactivada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarTiposAsync(int? seleccionado = null) =>
        ViewBag.Tipos = new SelectList(await repositorio.TiposActivosAsync(), "IdTipoHabitacion", "Nombre", seleccionado);

    private void LimpiarNavegacion() => ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));

    private async Task ValidarAsync(Habitacion habitacion, int? excluirId = null)
    {
        if (string.IsNullOrWhiteSpace(habitacion.Numero))
            ModelState.AddModelError(nameof(Habitacion.Numero), "Ingresa el número.");
        else if (await repositorio.NumeroExisteAsync(habitacion.Numero, excluirId))
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número ya existe.");
        if (habitacion.Piso < 1)
            ModelState.AddModelError(nameof(Habitacion.Piso), "El piso debe ser mayor que cero.");
        if (!await repositorio.TipoActivoAsync(habitacion.IdTipoHabitacion))
            ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "Selecciona un tipo activo.");
    }
}
