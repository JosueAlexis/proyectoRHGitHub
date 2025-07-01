using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRH2025.Pages.Sellos
{
    public class AsignarOperadorModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AsignarOperadorModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<SelectListItem> Sellos { get; set; } = new();
        public List<SelectListItem> Operadores { get; set; } = new();
        public List<SelectListItem> Unidades { get; set; } = new();
        public List<SelectListItem> Coordinadores { get; set; } = new();
        public List<SelectListItem> TiposAsignacion { get; set; } = new();

        [BindProperty, Required] public int IdSello { get; set; }
        [BindProperty, Required] public int IdOperador { get; set; }
        [BindProperty] public int? IdOperador2 { get; set; }
        [BindProperty, Required] public int IdUnidad { get; set; }
        [BindProperty] public int TipoAsignacion { get; set; }
        [BindProperty, Required] public int idCoordinador { get; set; }
        [BindProperty, Required] public string? Ruta { get; set; }
        [BindProperty, Required] public string? Caja { get; set; }
        [BindProperty] public string? Comentarios { get; set; }

        public string? Mensaje { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await CargarDatosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
            {
                Mensaje = "?? Sesión expirada.";
                await CargarDatosAsync();
                return Page();
            }

            await CargarDatosAsync();

            var sello = await _context.TblSellos
                .FirstOrDefaultAsync(s => s.Id == IdSello && s.Status == 1 && s.SupervisorId == idUsuario);

            if (sello == null)
            {
                Mensaje = "? El sello no está disponible o no está asignado a usted.";
                return Page();
            }

            var asignacion = new TblAsigSellos
            {
                idSello = IdSello,
                idUsuario = idUsuario.Value,
                Fentrega = DateTime.Now,
                idOperador = IdOperador,
                idOperador2 = TipoAsignacion == 1 ? IdOperador2 : null,
                TipoAsignacion = TipoAsignacion,
                idUnidad = IdUnidad,
                Ruta = Ruta,
                Caja = Caja,
                Comentarios = Comentarios,
                Status = 4, // Estado Trámite
                idSeAsigno = idCoordinador, // ? Campo correcto
                FechaStatus4 = DateTime.Now
            };

            _context.TblAsigSellos.Add(asignacion);

            sello.Status = 4; // También marcar el sello como Trámite

            await _context.SaveChangesAsync();

            // ? Limpia los campos del formulario
            IdSello = 0;
            IdOperador = 0;
            IdOperador2 = null;
            IdUnidad = 0;
            TipoAsignacion = 0;
            idCoordinador = 0;
            Ruta = "";
            Caja = "";
            Comentarios = "";

            Mensaje = "? Sello asignado correctamente.";

            await CargarDatosAsync();
            return Page();
        }

        private async Task CargarDatosAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");

            // ? 1. Sellos disponibles que el supervisor tiene asignados (Status = 1)
            Sellos = await _context.TblSellos
                .Where(s => s.Status == 1 && s.SupervisorId == idUsuario)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Sello
                })
                .ToListAsync();

            // ? 2. Operadores activos (Puesto = 1 y Status = 1) con búsqueda avanzada
            Operadores = await _context.Empleados
                .Where(e => e.Puesto == 1 && e.Status == 1)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Reloj} - {e.Names} {e.Apellido} {e.Apellido2}"
                }).ToListAsync();

            // ? 3. Unidades, mostrando solo número (pero se puede ajustar si deseas más datos)
            Unidades = await _context.TblUnidades
                .Select(u => new SelectListItem
                {
                    Value = u.id.ToString(),
                    Text = u.NumUnidad.ToString()
                }).ToListAsync();

            // ? 4. Coordinadores (rol 6) con usuario y nombre completo visibles
            Coordinadores = await _context.TblUsuarios
                .Where(u => u.idRol == 6)
                .Select(u => new SelectListItem
                {
                    Value = u.idUsuario.ToString(),
                    Text = $"{u.UsuarioNombre} - {u.NombreCompleto}"
                }).ToListAsync();

            // ? 5. Tipos de asignación (Individual y Comboy)
            TiposAsignacion = await _context.TblTipoAsignacion
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Nombre
                }).ToListAsync();
        }

    }
}
