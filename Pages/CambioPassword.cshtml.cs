using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoRH2025.Pages
{
    public class CambioPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CambioPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public string PasswordActual { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        public string PasswordNueva { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Confirma la nueva contraseña.")]
        [Compare(nameof(PasswordNueva), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; }

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                MensajeError = "Por favor completa todos los campos correctamente.";
                return Page();
            }

            var idUsuario = HttpContext.Session.GetInt32("idUsuario");
            if (idUsuario == null)
            {
                MensajeError = "Sesión expirada. Por favor inicia sesión nuevamente.";
                return Page();
            }

            var usuario = await _context.TblUsuarios.FindAsync(idUsuario);
            if (usuario == null)
            {
                MensajeError = "Usuario no encontrado.";
                return Page();
            }

            using var sha1 = SHA1.Create();
            var hashActual = sha1.ComputeHash(Encoding.UTF8.GetBytes(PasswordActual));

            if (!usuario.pass.SequenceEqual(hashActual))
            {
                MensajeError = "La contraseña actual es incorrecta.";
                return Page();
            }

            if (!EsContraseñaSegura(PasswordNueva))
            {
                MensajeError = "La nueva contraseña debe tener al menos 8 caracteres e incluir letras, números y símbolos.";
                return Page();
            }

            var hashNuevo = sha1.ComputeHash(Encoding.UTF8.GetBytes(PasswordNueva));

            var historico = await _context.TblHistoricoPass
                .Where(h => h.idUsuario == idUsuario)
                .OrderByDescending(h => h.ID)
                .Take(5)
                .Select(h => h.PassAnterior)
                .ToListAsync();

            if (historico.Any(h => h.SequenceEqual(hashNuevo)))
            {
                MensajeError = "No puedes reutilizar una de tus últimas 5 contraseñas.";
                return Page();
            }

            _context.TblHistoricoPass.Add(new TblHistoricoPass
            {
                idUsuario = usuario.idUsuario,
                PassAnterior = usuario.pass
            });

            usuario.pass = hashNuevo;
            usuario.DefaultPassw = 0;
            usuario.CambioPass = DateTime.Now;

            await _context.SaveChangesAsync();

            // Limpiar contraseñas viejas (dejar solo 5)
            var contraseñasViejas = await _context.TblHistoricoPass
                .Where(h => h.idUsuario == usuario.idUsuario)
                .OrderByDescending(h => h.ID)
                .Skip(5)
                .ToListAsync();

            if (contraseñasViejas.Any())
            {
                _context.TblHistoricoPass.RemoveRange(contraseñasViejas);
                await _context.SaveChangesAsync();
            }

            MensajeExito = "La contraseña fue actualizada exitosamente.";
            return Page();
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
