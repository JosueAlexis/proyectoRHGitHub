using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using ProyectoRH2025.Data;
using ProyectoRH2025.MODELS;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Diagnostics;

namespace ProyectoRH2025.Pages.Liquidaciones
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public IndexModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IList<LiquidacionViewModel> Liquidaciones { get; set; } = new List<LiquidacionViewModel>();

        [BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaFin { get; set; }
        public byte? StatusFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ClienteFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? EvidenciasFiltro { get; set; }

        public DateTime FechaMinimaDisponible { get; set; }
        public DateTime FechaMaximaDisponible { get; set; }
        public int TotalRegistros { get; set; }
        public int RegistrosMostrados { get; set; }
        public bool MostrandoSoloEstaSemana { get; set; }

        // Opciones para los dropdowns
        public List<ClienteOpcion> ClientesDisponibles { get; set; } = new List<ClienteOpcion>();

        // Para mostrar filtros activos
        public string StatusFiltroTexto { get; set; } = "";
        public string EvidenciasFiltroTexto { get; set; } = "";

        // DIAGNÓSTICO
        public string DiagnosticoTiempos { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var stopwatchTotal = Stopwatch.StartNew();
            var diagnostico = new List<string>();

            // ---- VERIFICACIÓN DE ROL ----
            var rolId = HttpContext.Session.GetInt32("idRol");
            var rolesITPermitidos = new[] { 5, 7, 1007 };
            var idRolLiquidacionesPermitido = 1009;

            bool esLiquidaciones = rolId.HasValue && rolId.Value == idRolLiquidacionesPermitido;
            bool esAdministrativoIT = rolId.HasValue && rolesITPermitidos.Contains(rolId.Value);

            if (!esLiquidaciones && !esAdministrativoIT)
            {
                return RedirectToPage("/Login");
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                // CARGAR OPCIONES DE FILTROS
                var sw0 = Stopwatch.StartNew();
                await CargarOpcionesFiltrosAsync(connectionString);
                sw0.Stop();
                diagnostico.Add($"Opciones filtros: {sw0.ElapsedMilliseconds}ms");

                // STORED PROCEDURE 1: Estadísticas rápidas
                var sw1 = Stopwatch.StartNew();
                await GetEstadisticasRapidoAsync(connectionString);
                sw1.Stop();
                diagnostico.Add($"Estadísticas: {sw1.ElapsedMilliseconds}ms");

                // Configurar fechas por defecto basadas en tus datos reales
                if (!FechaInicio.HasValue || !FechaFin.HasValue)
                {
                    FechaInicio = new DateTime(2025, 6, 17);
                    FechaFin = new DateTime(2025, 6, 23);
                    MostrandoSoloEstaSemana = true;
                }

                // STORED PROCEDURE 2: Liquidaciones con filtros
                var sw2 = Stopwatch.StartNew();
                await GetLiquidacionesConFiltrosAsync(connectionString);
                sw2.Stop();
                diagnostico.Add($"Liquidaciones: {sw2.ElapsedMilliseconds}ms");

                RegistrosMostrados = Liquidaciones.Count;

                // Preparar textos para mostrar filtros activos
                PrepararFiltrosActivos();

                stopwatchTotal.Stop();
                diagnostico.Add($"TOTAL: {stopwatchTotal.ElapsedMilliseconds}ms");
                DiagnosticoTiempos = string.Join(" | ", diagnostico);

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                DiagnosticoTiempos = $"ERROR: {ex.Message}";
                Liquidaciones = new List<LiquidacionViewModel>();
                return Page();
            }
        }

        private async Task CargarOpcionesFiltrosAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand("SP_GetFiltroOpciones", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 5
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                ClientesDisponibles.Add(new ClienteOpcion
                {
                    Nombre = reader.GetString("Valor"),
                    Cantidad = reader.GetInt32("Cantidad")
                });
            }
        }

        private async Task GetEstadisticasRapidoAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand("SP_GetLiquidacionesEstadisticas", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 10
            };

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                TotalRegistros = reader.GetInt32("TotalRegistros");
                FechaMinimaDisponible = reader.IsDBNull("FechaMinimaDisponible")
                    ? DateTime.Today.AddDays(-30)
                    : reader.GetDateTime("FechaMinimaDisponible");
                FechaMaximaDisponible = reader.IsDBNull("FechaMaximaDisponible")
                    ? DateTime.Today
                    : reader.GetDateTime("FechaMaximaDisponible");
            }
        }

        private async Task GetLiquidacionesConFiltrosAsync(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand("SP_GetLiquidacionesConFiltros", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 10
            };

            // Determinar límite dinámico
            int maxRecords = 500; // Por defecto
            if (FechaInicio.HasValue && FechaFin.HasValue)
            {
                var diasDiferencia = (FechaFin.Value - FechaInicio.Value).Days;

                if (diasDiferencia <= 1) maxRecords = 100;
                else if (diasDiferencia <= 7) maxRecords = 1000;
                else if (diasDiferencia <= 30) maxRecords = 2000;
                else maxRecords = 5000;
            }

            // Parámetros básicos
            command.Parameters.Add(new SqlParameter("@SearchString", SqlDbType.NVarChar, 255)
            {
                Value = string.IsNullOrEmpty(SearchString) ? DBNull.Value : SearchString
            });
            command.Parameters.Add(new SqlParameter("@FechaInicio", SqlDbType.DateTime)
            {
                Value = FechaInicio ?? (object)DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@FechaFin", SqlDbType.DateTime)
            {
                Value = FechaFin ?? (object)DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@MaxRecords", SqlDbType.Int) { Value = maxRecords });

            // NUEVOS PARÁMETROS DE FILTROS
            command.Parameters.Add(new SqlParameter("@StatusFiltro", SqlDbType.TinyInt)
            {
                Value = StatusFiltro.HasValue ? StatusFiltro.Value : DBNull.Value
            });
            command.Parameters.Add(new SqlParameter("@ClienteFiltro", SqlDbType.NVarChar, 255)
            {
                Value = string.IsNullOrEmpty(ClienteFiltro) ? DBNull.Value : ClienteFiltro
            });
            command.Parameters.Add(new SqlParameter("@EvidenciasFiltro", SqlDbType.NVarChar, 20)
            {
                Value = string.IsNullOrEmpty(EvidenciasFiltro) ? DBNull.Value : EvidenciasFiltro
            });

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var liquidaciones = new List<LiquidacionViewModel>();

            while (await reader.ReadAsync())
            {
                var liquidacion = new LiquidacionViewModel
                {
                    PodId = reader.GetInt32("POD_ID"),
                    Folio = reader.IsDBNull("Folio") ? null : reader.GetString("Folio"),
                    Cliente = reader.IsDBNull("Cliente") ? null : reader.GetString("Cliente"),
                    Tractor = reader.IsDBNull("Tractor") ? null : reader.GetString("Tractor"),
                    Remolque = reader.IsDBNull("Remolque") ? null : reader.GetString("Remolque"),
                    FechaSalida = reader.IsDBNull("FechaSalida") ? null : reader.GetDateTime("FechaSalida"),
                    Origen = reader.IsDBNull("Origen") ? null : reader.GetString("Origen"),
                    Destino = reader.IsDBNull("Destino") ? null : reader.GetString("Destino"),
                    DriverName = reader.IsDBNull("DriverName") ? null : reader.GetString("DriverName"),
                    Status = ConvertStatusToString(reader.IsDBNull("Status") ? null : reader.GetByte("Status")),
                    PodRecordCaptureDate = reader.IsDBNull("CaptureDate") ? null : reader.GetDateTime("CaptureDate"),
                    PodRecordImageUrl = reader.IsDBNull("ImageUrl") ? null : reader.GetString("ImageUrl"),
                    Evidencias = new List<EvidenciaViewModel>()
                };

                // Agregar evidencias simplificadas (solo conteos)
                var totalEvidencias = reader.GetInt32("TotalEvidencias");
                var evidenciasConImagen = reader.GetInt32("EvidenciasConImagen");

                // Crear evidencias "fake" solo para que la vista funcione
                for (int i = 0; i < totalEvidencias && i < 5; i++)
                {
                    liquidacion.Evidencias.Add(new EvidenciaViewModel
                    {
                        EvidenciaId = 0,
                        FileName = $"Evidencia {i + 1}",
                        HasImageData = i < evidenciasConImagen
                    });
                }

                liquidaciones.Add(liquidacion);
            }

            Liquidaciones = liquidaciones;
        }

        private void PrepararFiltrosActivos()
        {
            if (StatusFiltro.HasValue)
            {
                StatusFiltroTexto = StatusFiltro.Value switch
                {
                    0 => "En Tránsito",
                    1 => "Entregado",
                    2 => "Pendiente",
                    _ => "Desconocido"
                };
            }

            if (!string.IsNullOrEmpty(EvidenciasFiltro))
            {
                EvidenciasFiltroTexto = EvidenciasFiltro switch
                {
                    "sin_evidencias" => "Sin evidencias",
                    "con_evidencias" => "Con evidencias",
                    "solo_imagenes" => "Solo con imágenes",
                    _ => EvidenciasFiltro
                };
            }
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
    }

    // DTO para opciones de filtros
    public class ClienteOpcion
    {
        public string Nombre { get; set; } = "";
        public int Cantidad { get; set; }
    }
}