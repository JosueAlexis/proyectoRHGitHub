using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.MODELS;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace ProyectoRH2025.Pages.Liquidaciones
{
    public class DetallesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetallesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // MANTENER LA MISMA INTERFAZ PÚBLICA
        public PodRecord LiquidacionDetalle { get; set; }
        public List<PodEvidenciaInfo> EvidenciasInfo { get; set; } = new List<PodEvidenciaInfo>();
        public string ErrorMessage { get; set; }
        public string StatusText { get; private set; }

        // NUEVO: Diagnóstico de rendimiento
        public string DiagnosticoTiempos { get; set; } = "";

        // Clase para manejar solo la información básica de las evidencias (IGUAL QUE ANTES)
        public class PodEvidenciaInfo
        {
            public int EvidenciaID { get; set; }
            public string FileName { get; set; }
            public bool HasImage { get; set; }
            public DateTime? CaptureDate { get; set; }
            public int? ImageSequence { get; set; }
        }

        private string ConvertStatusToString(byte? statusValue)
        {
            if (!statusValue.HasValue) return "Desconocido";
            return statusValue.Value switch
            {
                0 => "En Tránsito",
                1 => "Entregado",
                2 => "Pendiente",
                _ => $"Código: {statusValue.Value}"
            };
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var stopwatchTotal = Stopwatch.StartNew();
            var diagnostico = new List<string>();

            // ---- VERIFICACIÓN DE ROL (IGUAL QUE ANTES) ----
            var rolIdSession = HttpContext.Session.GetInt32("idRol");
            var rolesITPermitidos = new[] { 5,1007 };
            var idRolLiquidacionesPermitido = 1009;

            bool esLiquidaciones = rolIdSession.HasValue && rolIdSession.Value == idRolLiquidacionesPermitido;
            bool esAdministrativoIT = rolIdSession.HasValue && rolesITPermitidos.Contains(rolIdSession.Value);

            if (!esLiquidaciones && !esAdministrativoIT)
            {
                return RedirectToPage("/Login");
            }

            if (id == null)
            {
                ErrorMessage = "No se proporcionó un ID para la liquidación.";
                return Page();
            }

            try
            {
                // OPTIMIZACIÓN BACKEND 1: Consulta separada SIN imágenes
                var sw1 = Stopwatch.StartNew();
                LiquidacionDetalle = await _context.PodRecords
                    .AsNoTracking() // Mejora rendimiento
                    .Where(p => p.POD_ID == id.Value)
                    .Select(p => new PodRecord
                    {
                        POD_ID = p.POD_ID,
                        Folio = p.Folio,
                        Cliente = p.Cliente,
                        Tractor = p.Tractor,
                        Remolque = p.Remolque,
                        FechaSalida = p.FechaSalida,
                        Origen = p.Origen,
                        Destino = p.Destino,
                        DriverName = p.DriverName,
                        Status = p.Status,
                        CaptureDate = p.CaptureDate
                    })
                    .FirstOrDefaultAsync();
                sw1.Stop();
                diagnostico.Add($"Detalle principal: {sw1.ElapsedMilliseconds}ms");

                if (LiquidacionDetalle == null)
                {
                    ErrorMessage = $"No se encontró la liquidación con ID {id}.";
                    return Page();
                }

                // OPTIMIZACIÓN BACKEND 2: Evidencias SIN ImageData
                var sw2 = Stopwatch.StartNew();
                EvidenciasInfo = await _context.PodEvidenciasImagenes
                    .AsNoTracking()
                    .Where(e => e.POD_ID_FK == id.Value)
                    .Select(e => new PodEvidenciaInfo
                    {
                        EvidenciaID = e.EvidenciaID,
                        FileName = e.FileName ?? "Sin nombre",
                        HasImage = e.ImageData != null && e.ImageData.Length > 0,
                        CaptureDate = e.CaptureDate,
                        ImageSequence = e.ImageSequence
                    })
                    .OrderBy(e => e.ImageSequence ?? 999)
                    .ThenBy(e => e.CaptureDate)
                    .ToListAsync();
                sw2.Stop();
                diagnostico.Add($"Evidencias ({EvidenciasInfo.Count}): {sw2.ElapsedMilliseconds}ms");

                StatusText = ConvertStatusToString(LiquidacionDetalle.Status);

                stopwatchTotal.Stop();
                diagnostico.Add($"TOTAL: {stopwatchTotal.ElapsedMilliseconds}ms");
                DiagnosticoTiempos = string.Join(" | ", diagnostico);

                return Page();
            }
            catch (Exception ex)
            {
                stopwatchTotal.Stop();
                ErrorMessage = $"Error al cargar los datos: {ex.Message}";
                DiagnosticoTiempos = $"ERROR en {stopwatchTotal.ElapsedMilliseconds}ms: {ex.Message}";
                return Page();
            }
        }

        // MÉTODO OPTIMIZADO: Servir imágenes individuales con cache
        public async Task<IActionResult> OnGetImageAsync(int evidenciaId)
        {
            try
            {
                // OPTIMIZACIÓN: Consulta solo la imagen específica necesaria
                var evidencia = await _context.PodEvidenciasImagenes
                    .AsNoTracking()
                    .Where(e => e.EvidenciaID == evidenciaId)
                    .Select(e => new { e.ImageData, e.MimeType, e.FileName })
                    .FirstOrDefaultAsync();

                if (evidencia == null || evidencia.ImageData == null || string.IsNullOrEmpty(evidencia.MimeType))
                {
                    return NotFound();
                }

                // OPTIMIZACIÓN: Headers de cache para evitar descargas repetidas
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // 1 hora
                Response.Headers.Add("ETag", $"\"{evidenciaId}_{evidencia.ImageData.Length}\"");

                return File(evidencia.ImageData, evidencia.MimeType, evidencia.FileName ?? "imagen");
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}