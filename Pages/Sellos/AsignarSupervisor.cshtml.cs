using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System;

namespace ProyectoRH2025.Pages.Sellos
{
    public class AsignarSupervisorModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public AsignarSupervisorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int SupervisorId { get; set; }

        [BindProperty]
        public int Cantidad { get; set; }

        public List<SelectListItem> Supervisores { get; set; } = new();
        public string? Mensaje { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarSupervisores();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await CargarSupervisores();

            if (SupervisorId <= 0 || Cantidad <= 0)
            {
                Mensaje = "Selecciona un supervisor y una cantidad válida.";
                return Page();
            }

            // Verificar si el supervisor tiene sellos status 4 con más de 4 días
            var sellosPendientes = await _context.TblSellos
                .Where(s => s.SupervisorId == SupervisorId && s.Status == 4 && s.FechaAsignacion <= DateTime.Now.AddDays(-4))
                .ToListAsync();

            if (sellosPendientes.Any())
            {
                Mensaje = "Este supervisor tiene sellos pendientes (Status 4) sin cerrar desde hace más de 4 días. No se puede asignar nuevos sellos.";
                return Page();
            }

            // Obtener sellos disponibles (status 1)
            var disponibles = await _context.TblSellos
                .Where(s => s.Status == 1)
                .OrderBy(x => Guid.NewGuid()) // aleatorio
                .ToListAsync();

            if (disponibles.Count < Cantidad)
            {
                Mensaje = $"No hay suficientes sellos disponibles (hay {disponibles.Count}).";
                return Page();
            }

            // Tomar aleatoriamente sin consecutivos
            var asignados = new List<TblSellos>();
            var usados = new HashSet<string>();

            foreach (var sello in disponibles)
            {
                if (asignados.Count >= Cantidad)
                    break;

                // Verifica si no es consecutivo con los ya seleccionados
                if (asignados.Any(s =>
                    Math.Abs(Convert.ToInt32(s.Sello) - Convert.ToInt32(sello.Sello)) <= 1))
                {
                    continue;
                }

                asignados.Add(sello);
                usados.Add(sello.Sello);
            }

            if (asignados.Count < Cantidad)
            {
                Mensaje = $"No se pudieron seleccionar {Cantidad} sellos sin consecutivos. Solo se asignaron {asignados.Count}.";
                return Page();
            }

            // Asignar sellos
            foreach (var s in asignados)
            {
                s.Status = 2; // Marcado como asignado
                s.SupervisorId = SupervisorId;
                s.FechaAsignacion = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            Mensaje = $"Se asignaron correctamente {asignados.Count} sellos al supervisor.";
            return Page();
        }

        private async Task CargarSupervisores()
        {
            Supervisores = await _context.TblUsuarios
                .Where(u => u.idRol == 2 && u.Status == 1)
                .Select(u => new SelectListItem
                {
                    Value = u.idUsuario.ToString(),
                    Text = u.UsuarioNombre
                })
                .ToListAsync();
        }
    }
}
