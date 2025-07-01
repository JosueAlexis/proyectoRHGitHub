using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;

namespace ProyectoRH2025.Pages.IT
{
    public class UsuariosModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UsuariosModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<UsuarioVista> Usuarios { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Solo IT puede acceder (ajusta el idRol si es diferente)
            var rol = HttpContext.Session.GetInt32("idRol");
            if (rol != 5 && rol != 7) // Solo Admin o IT
            {
                return RedirectToPage("/Login");
            }

            var query = _context.TblUsuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                query = query.Where(u =>
                    u.UsuarioNombre.Contains(Filtro) ||
                    u.NombreCompleto.Contains(Filtro));
            }

            Usuarios = await query
                .Select(u => new UsuarioVista
                {
                    idUsuario = u.idUsuario,
                    UsuarioNombre = u.UsuarioNombre,
                    NombreCompleto = u.NombreCompleto,
                    CorreoElectronico = u.CorreoElectronico,
                    Status = u.Status,
                    NombreRol = u.Rol.RolNombre
                })
                .ToListAsync();

            return Page();
        }

        public class UsuarioVista
        {
            public int idUsuario { get; set; }
            public string UsuarioNombre { get; set; }
            public string? NombreCompleto { get; set; }
            public string? CorreoElectronico { get; set; }
            public int Status { get; set; }
            public string NombreRol { get; set; }
        }
    }
}
