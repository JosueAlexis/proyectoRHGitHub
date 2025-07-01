using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Security.Cryptography;

namespace ProyectoRH2025.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public LoginInputModel LoginData { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _context.TblUsuarios
                .FirstOrDefaultAsync(u =>
                    u.UsuarioNombre == LoginData.Usuario &&
                    u.Status == 1);

            if (user == null)
            {
                ErrorMessage = "Usuario o contraseña inválidos.";
                return Page();
            }

            using (var sha1 = SHA1.Create())
            {
                var passwordHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(LoginData.Password));

                if (!user.pass.SequenceEqual(passwordHash))
                {
                    ErrorMessage = "Usuario o contraseña inválidos.";
                    return Page();
                }
            }

            // Guardar usuario en sesión
            HttpContext.Session.SetInt32("idUsuario", user.idUsuario);
            HttpContext.Session.SetInt32("idRol", user.idRol);
            HttpContext.Session.SetString("usuarioNombre", user.UsuarioNombre);

            // Verificar si debe cambiar la contraseña
            if ((user.DefaultPassw ?? 0) == 1 || user.CambioPass == null) // ? corregido
            {
                return RedirectToPage("/CambioPassword");
            }

            return RedirectToPage("/Index");
        }


        public class LoginInputModel
        {
            [Required]
            public string Usuario { get; set; }

            [Required]
            public string Password { get; set; }
        }
    }
}
