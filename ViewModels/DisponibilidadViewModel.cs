using Hotel.Web.Models;

namespace Hotel.Web.ViewModels;

public class DisponibilidadViewModel
{
    public DateOnly? Ingreso { get; set; }
    public DateOnly? Salida { get; set; }
    public int Noches { get; set; }
    public List<Habitacion> Habitaciones { get; set; } = [];
}
