using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;

namespace ProyectoRH2025.Pages.Sellos
{
    public class InventarioModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InventarioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<TblSellos> SellosDisponibles { get; set; } = new();
        public List<TblSellos> SellosAsignados { get; set; } = new();
        public List<SelectListItem> Supervisores { get; set; } = new();

        [BindProperty]
        public int SupervisorId { get; set; }

        [BindProperty]
        public int Cantidad { get; set; }

        public string? Mensaje { get; set; }

        public async Task OnGetAsync()
        {
            await CargarDatos();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarDatos();

            if (SupervisorId <= 0 || Cantidad <= 0)
            {
                Mensaje = "Debes seleccionar un supervisor y una cantidad válida.";
                return Page();
            }

            var tienePendientes = await _context.TblSellos
                .AnyAsync(s => s.SupervisorId == SupervisorId && s.Status == 4 && s.FechaAsignacion <= DateTime.Now.AddDays(-4));

            if (tienePendientes)
            {
                Mensaje = "Este supervisor tiene sellos en status 4 sin cerrar por más de 4 días.";
                return Page();
            }

            var disponibles = await _context.TblSellos
                .Where(s => s.Status == 1 && s.SupervisorId == null)
                .OrderBy(x => Guid.NewGuid())
                .ToListAsync();

            var asignados = new List<TblSellos>();

            foreach (var sello in disponibles)
            {
                if (asignados.Count >= Cantidad)
                    break;

                if (asignados.Any(s => Math.Abs(int.Parse(s.Sello) - int.Parse(sello.Sello)) <= 1))
                    continue;

                asignados.Add(sello);
            }

            if (asignados.Count < Cantidad)
            {
                Mensaje = $"Solo se pudieron asignar {asignados.Count} sellos (no consecutivos suficientes).";
                return Page();
            }

            foreach (var s in asignados)
            {
                s.SupervisorId = SupervisorId;
                s.FechaAsignacion = DateTime.Now;
                // Status permanece en 1 (disponible asignado)
            }

            await _context.SaveChangesAsync();

            Mensaje = $"Se asignaron correctamente {asignados.Count} sellos.";
            await CargarDatos();

            return Page();
        }

        private async Task CargarDatos()
        {
            Supervisores = await _context.TblUsuarios
                .Where(u => u.idRol == 2 && u.Status == 1)
                .Select(u => new SelectListItem
                {
                    Value = u.idUsuario.ToString(),
                    Text = u.UsuarioNombre
                })
                .ToListAsync();

            SellosDisponibles = await _context.TblSellos
                .Where(s => s.Status == 1 && s.SupervisorId == null)
                .OrderBy(s => s.Sello)
                .ToListAsync();

            SellosAsignados = await _context.TblSellos
                .Include(s => s.Supervisor)                    // ? aquí
                .Where(s => s.Status == 1 && s.SupervisorId != null)
                .OrderByDescending(s => s.FechaAsignacion)
                .ToListAsync();
        }


    }
}
