using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace ProyectoRH2025.Pages.Sellos
{
    public class ImportarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public string? Mensaje { get; set; }

        public ImportarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync(IFormFile ArchivoExcel)
        {
            if (ArchivoExcel == null || ArchivoExcel.Length == 0)
            {
                Mensaje = "Selecciona un archivo válido.";
                return Page();
            }

            var sellosNuevos = new List<TblSellos>();
            using (var stream = new MemoryStream())
            {
                await ArchivoExcel.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var hoja = workbook.Worksheet(1);

                foreach (var fila in hoja.RowsUsed().Skip(1)) // Saltamos encabezado
                {
                    var numeroSello = fila.Cell(1).GetString().Trim();
                    var fechaTexto = fila.Cell(2).GetString().Trim();
                    var recibidoPor = fila.Cell(3).GetString().Trim();

                    if (string.IsNullOrWhiteSpace(numeroSello)) continue;

                    // Validar si ya existe en la BD
                    if (_context.TblSellos.Any(s => s.Sello == numeroSello)) continue;

                    if (!DateTime.TryParse(fechaTexto, out DateTime fechaEntrega))
                        fechaEntrega = DateTime.Now;

                    sellosNuevos.Add(new TblSellos
                    {
                        Sello = numeroSello,
                        Fentrega = fechaEntrega,
                        Recibio = recibidoPor,
                        Status = 1,
                        SupervisorId = null,
                        FechaAsignacion = null,
                        Alta = HttpContext.Session.GetInt32("idUsuario")
                    });
                }

                if (sellosNuevos.Count > 0)
                {
                    _context.TblSellos.AddRange(sellosNuevos);
                    await _context.SaveChangesAsync();
                    Mensaje = $"Se importaron {sellosNuevos.Count} sellos correctamente.";
                }
                else
                {
                    Mensaje = "No se importaron sellos (puede que ya existan o el archivo esté vacío).";
                }
            }

            return Page();
        }
    }
}
