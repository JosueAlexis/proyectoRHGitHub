using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.ComponentModel.DataAnnotations;

namespace ProyectoRH2025.Pages
{
    public class RecuperarPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RecuperarPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string NombreUsuario { get; set; }

        public string? MensajeError { get; set; }
        public string? TokenGenerado { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var usuario = await _context.TblUsuarios
                .FirstOrDefaultAsync(u => u.UsuarioNombre == NombreUsuario && u.Status == 1);

            if (usuario == null)
            {
                MensajeError = "Usuario no encontrado o inactivo.";
                return Page();
            }

            // Generar token aleatorio único
            var token = Guid.NewGuid().ToString("N"); // sin guiones

            // Guardarlo en la base de datos
            usuario.TokenRecuperacion = token;
            await _context.SaveChangesAsync();

            // Mostrar enlace (simulado)
            TokenGenerado = token;

            return Page();
        }
    }
}
