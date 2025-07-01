using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoRH2025.Pages.IT
{
    public class CrearUsuarioModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CrearUsuarioModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Usuario Usuario { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string PasswordInicial { get; set; }

        [BindProperty]
        public bool ForzarCambio { get; set; }

        public List<SelectListItem> Roles { get; set; } = new();

        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var rol = HttpContext.Session.GetInt32("idRol");
            if (rol != 5 && rol != 1007)
                return RedirectToPage("/Login");

            await LoadRolesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Debug.WriteLine("?? El método OnPostAsync se ejecutó");
            await LoadRolesAsync();

            if (!ModelState.IsValid)
            {
                // ?? Mostrar en consola los errores de validación
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        Debug.WriteLine($"?? Error en el campo: {entry.Key}");
                        foreach (var error in entry.Value.Errors)
                        {
                            Debug.WriteLine($"    ? {error.ErrorMessage}");
                        }
                    }
                }

                MensajeError = "Formulario inválido.";
                return Page();
            }

            try
            {
                using var sha1 = SHA1.Create();
                Usuario.pass = sha1.ComputeHash(Encoding.UTF8.GetBytes(PasswordInicial));
                Usuario.DefaultPassw = ForzarCambio ? 1 : 0;
                Usuario.CambioPass = null;
                Usuario.TokenRecuperacion = null;

                _context.TblUsuarios.Add(Usuario);
                var filas = await _context.SaveChangesAsync();

                if (filas == 0)
                {
                    MensajeError = "No se guardó ningún registro.";
                    return Page();
                }

                return RedirectToPage("/IT/Usuarios");
            }
            catch (Exception ex)
            {
                MensajeError = "Error al guardar: " + ex.Message;
                return Page();
            }
        }


        private async Task LoadRolesAsync()
        {
            Roles = await _context.TblRolusuario
                .Select(r => new SelectListItem
                {
                    Value = r.idRol.ToString(),
                    Text = r.RolNombre
                })
                .ToListAsync();
        }
        private bool EsContraseñaSegura(string contraseña)
        {
            if (string.IsNullOrEmpty(contraseña)) return false;

            bool tieneLetra = contraseña.Any(char.IsLetter);
            bool tieneNumero = contraseña.Any(char.IsDigit);
            bool tieneSimbolo = contraseña.Any(c => !char.IsLetterOrDigit(c));
            bool longitudValida = contraseña.Length >= 8;

            return tieneLetra && tieneNumero && tieneSimbolo && longitudValida;
        }

    }
}
