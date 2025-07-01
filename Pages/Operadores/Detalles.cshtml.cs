using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;

namespace ProyectoRH2025.Pages.Operadores
{
    public class DetallesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetallesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Empleado? Empleado { get; set; }
        public string? ImagenBase64 { get; set; }
        public string? NombrePuesto { get; set; }
        public string? FechaIngresoFormateada { get; set; }
        // Propiedades para documentos
        public class DocumentoLicencia
        {
            public string? NumeroLicencia { get; set; }
            public string? Clasificacion { get; set; }
            public string? Clasificacion2 { get; set; }
            public DateTime? FechaAntiguedad { get; set; }
            public int? AnosAntiguedad { get; set; }
            public DateTime? Refrendo { get; set; }
            public DateTime? Vigencia { get; set; }
        }

        public class DocumentoAPTO
        {
            public string? NumeroPreventiva { get; set; }
            public DateTime? Refrendo { get; set; }
            public DateTime? Vigencia { get; set; }
        }

        public class DocumentoGUI
        {
            public string? NumeroGUI { get; set; }
            public string? Estatus { get; set; }
            public DateTime? Vigencia { get; set; }
        }

        public DocumentoLicencia? Licencia { get; set; }
        public DocumentoAPTO? APTO { get; set; }
        public DocumentoGUI? GUI { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            Empleado = await _context.Empleados
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Empleado == null)
                return NotFound();

            // Formatear la fecha de ingreso
            FechaIngresoFormateada = Empleado.Fingreso?.ToString("dddd, dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));

            // Obtener el puesto
            if (Empleado.Puesto.HasValue)
            {
                var puesto = await _context.PuestoEmpleados
                    .Where(p => p.id == Empleado.Puesto)
                    .Select(p => p.Puesto)
                    .FirstOrDefaultAsync();

                NombrePuesto = puesto;
            }

            // Obtener la imagen
            var imagen = await _context.ImagenesEmpleados
                .Where(i => i.idEmpleado == id)
                .Select(i => i.Imagen)
                .FirstOrDefaultAsync();

            ImagenBase64 = imagen;

            return Page();
        }
    }
}