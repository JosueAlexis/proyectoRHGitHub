// Pages/Sellos/InventarioCoordinador.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace ProyectoRH2025.Pages.Sellos
{
    public class InventarioCoordinadorModel : PageModel
    {
        public class CapturaHuellaResponse
        {
            public int retcode { get; set; }
            public string template { get; set; }
        }

        public async Task<IActionResult> OnPostLeerHuellaAsync()
        {
            using var client = new HttpClient();
            var url = "http://localhost:8082/api/ScanBiomini?enroll_quality=100&template_type=2001";

            var requestBody = new
            {
                noblockui = false,
                ScanFingerprintOption = new
                {
                    enroll_quality = 0,
                    raw_image = false
                }
            };

            try
            {
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                {
                    return new JsonResult(new { success = false, error = "No se pudo conectar al lector." });
                }

                var body = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<CapturaHuellaResponse>(body);

                if (resultado != null && resultado.retcode == 1)
                {
                    return new JsonResult(new { success = true, template = resultado.template });
                }

                return new JsonResult(new { success = false, error = "Huella inválida o no leída correctamente." });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = "Error: " + ex.Message });
            }
        }

        private readonly ApplicationDbContext _context;
        public List<SelloAsignadoViewModel> SellosAsignados { get; set; } = new();

        public InventarioCoordinadorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            int? idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
            {
                Response.Redirect("/Login");
                return;
            }

            var asignaciones = await _context.TblAsigSellos
.Where(a => a.Status == 4).ToListAsync();

            var empleados = await _context.Empleados
                .Select(e => new
                {
                    e.Id,
                    e.Names,
                    e.Apellido,
                    e.Apellido2
                })
                .ToListAsync();

            var unidades = await _context.TblUnidades
                .Select(u => new
                {
                    u.id,
                    u.NumUnidad
                })
                .ToListAsync();

            SellosAsignados = asignaciones.Select(a => new SelloAsignadoViewModel
            {
                id = a.id,
                Sello = a.idSello,
                Fentrega = a.Fentrega,
                Ruta = a.Ruta ?? "",
                TipoAsignacion = a.TipoAsignacion,
                OperadorNombre = empleados.FirstOrDefault(e => e.Id == a.idOperador) is var op && op != null
                    ? $"{op.Names} {op.Apellido} {op.Apellido2}"
                    : "(sin nombre)",
                Unidad = unidades.FirstOrDefault(u => u.id == a.idUnidad)?.NumUnidad.ToString() ?? "(sin unidad)",
            }).ToList();
        }

        public class IdRequest
        {
            public int id { get; set; }
            public string huellaOperador { get; set; }
            public string huellaCoordinador { get; set; }
        }

        public async Task<IActionResult> OnPostActualizarEstadoAsync([FromBody] IdRequest data)
        {
            int? idUsuarioSesion = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuarioSesion == null)
                return new JsonResult(new { success = false, error = "No hay sesión." });

            var asignacion = await _context.TblAsigSellos.FirstOrDefaultAsync(x => x.id == data.id);
            if (asignacion == null)
                return new JsonResult(new { success = false, error = "Asignación no encontrada." });

            var huellaOperadorDB = await _context.TblHuellasEmpleados
                .Where(h => h.idEmpleado == asignacion.idOperador)
                .Select(h => h.Huella)
                .FirstOrDefaultAsync();

            var huellaCoordinadorDB = await _context.TblHuellasEmpleados
                .Where(h => h.idUsuario == idUsuarioSesion)
                .Select(h => h.Huella)
                .FirstOrDefaultAsync();

            // ?? Validación si falta alguna huella
            if (huellaOperadorDB == null)
                return new JsonResult(new { registrar = "operador", id = asignacion.idOperador });

            if (huellaCoordinadorDB == null)
                return new JsonResult(new { registrar = "coordinador", id = idUsuarioSesion });

            // ?? Validación normal
            if (huellaOperadorDB == data.huellaOperador && huellaCoordinadorDB == data.huellaCoordinador)
            {
                asignacion.Status = 3;
                asignacion.FechaStatus4 = DateTime.Now;
                asignacion.editor = idUsuarioSesion.ToString();

                var sello = await _context.TblSellos.FirstOrDefaultAsync(s => s.Id == asignacion.idSello);
                if (sello != null)
                    sello.Status = 3;

                await _context.SaveChangesAsync();
                return new JsonResult(new { success = true });
            }

            return new JsonResult(new { success = false, error = "Las huellas no coinciden." });
        }


        public class SelloAsignadoViewModel
        {
            public int id { get; set; }
            public int Sello { get; set; }
            public DateTime Fentrega { get; set; }
            public string Ruta { get; set; }
            public string Unidad { get; set; }
            public int TipoAsignacion { get; set; }
            public string OperadorNombre { get; set; }
        }
    }
}
