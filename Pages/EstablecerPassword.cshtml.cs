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
    public class EstablecerPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EstablecerPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        [BindProperty]
        [Required]
        public string NuevaPassword { get; set; }

        [BindProperty]
        [Required]
        public string ConfirmarPassword { get; set; }

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                MensajeError = "Token inválido o faltante.";
                return Page();
            }

            var usuario = await _context.TblUsuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == Token && u.Status == 1);

            if (usuario == null)
            {
                MensajeError = "Token inválido o usuario no encontrado.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Token))
            {
                MensajeError = "Token inválido.";
                return Page();
            }

            var usuario = await _context.TblUsuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == Token && u.Status == 1);

            if (usuario == null)
            {
                MensajeError = "Token inválido o expirado.";
                return Page();
            }

            if (NuevaPassword != ConfirmarPassword)
            {
                MensajeError = "Las contraseñas no coinciden.";
                return Page();
            }

            if (!EsPasswordValida(NuevaPassword))
            {
                MensajeError = "La contraseña debe tener al menos 8 caracteres, incluir letras, números y símbolos.";
                return Page();
            }

            // Hashear la nueva contraseña
            byte[] nuevaHash;
            using (var sha1 = SHA1.Create())
            {
                nuevaHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(NuevaPassword));
            }

            // ? Verificar que no se repita alguna de las últimas 5
            var historico = await _context.TblHistoricoPass
                .Where(h => h.idUsuario == usuario.idUsuario)
                .OrderByDescending(h => h.ID)
                .Select(h => h.PassAnterior)
                .Take(5)
                .ToListAsync();

            if (historico.Any(p => p.SequenceEqual(nuevaHash)))
            {
                MensajeError = "No puedes usar una contraseña que ya has utilizado recientemente.";
                return Page();
            }

            // Guardar la actual en el histórico
            _context.TblHistoricoPass.Add(new TblHistoricoPass
            {
                idUsuario = usuario.idUsuario,
                PassAnterior = usuario.pass
            });

            // Actualizar nueva contraseña
            usuario.pass = nuevaHash;
            usuario.TokenRecuperacion = null;
            usuario.DefaultPassw = 0;
            usuario.CambioPass = null;

            await _context.SaveChangesAsync();

            MensajeExito = "Contraseña actualizada correctamente. Ahora puedes iniciar sesión.";
            return Page();
        }

        private bool EsPasswordValida(string pass)
        {
            return pass.Length >= 8 &&
                   pass.Any(char.IsLetter) &&
                   pass.Any(char.IsDigit) &&
                   pass.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
