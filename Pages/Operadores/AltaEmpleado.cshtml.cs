using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ProyectoRH2025.Pages.Operadores
{
    public class AltaEmpleadoModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AltaEmpleadoModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Empleado Empleado { get; set; } = new Empleado();

        [BindProperty]
        public IFormFile Foto { get; set; }

        [TempData]
        public string Mensaje { get; set; } = string.Empty;

        public List<ProyectoRH2025.Models.PuestoEmpleado> Puestos { get; set; } = new List<ProyectoRH2025.Models.PuestoEmpleado>();

        public void OnGet()
        {
            CargarPuestos();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CargarPuestos();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Empleado.Names) || string.IsNullOrWhiteSpace(Empleado.Apellido) ||
                string.IsNullOrWhiteSpace(Empleado.Email) || string.IsNullOrWhiteSpace(Empleado.Rfc) ||
                string.IsNullOrWhiteSpace(Empleado.Curp) || string.IsNullOrWhiteSpace(Empleado.NumSSocial) ||
                Empleado.Fingreso == null)
            {
                ModelState.AddModelError(string.Empty, "Todos los campos obligatorios deben estar completos.");
                return Page();
            }

            if (Foto == null || Foto.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "La fotografía es obligatoria.");
                return Page();
            }

            using (var ms = new MemoryStream())
            {
                await Foto.CopyToAsync(ms);
                var bytes = ms.ToArray();
                var base64 = Convert.ToBase64String(bytes);

                Empleado.FechaAlta = DateTime.Now;
                Empleado.IdUsuarioAlta = HttpContext.Session.GetInt32("idUsuario") ?? 0;
                Empleado.Editor = Empleado.IdUsuarioAlta;
                Empleado.Status = 1;

                // Buscar el ID del puesto según el nombre enviado desde el formulario
                string puestoNombre = Request.Form["PuestoNombre"];
                var puestoSeleccionado = Puestos.FirstOrDefault(p => p.Puesto.Equals(puestoNombre, StringComparison.OrdinalIgnoreCase));
                if (puestoSeleccionado == null)
                {
                    ModelState.AddModelError(string.Empty, "El puesto seleccionado no es válido.");
                    return Page();
                }
                Empleado.Puesto = puestoSeleccionado.id;

                _context.Empleados.Add(Empleado);
                await _context.SaveChangesAsync();

                var img = new ImagenEmpleado
                {
                    idEmpleado = Empleado.Id,
                    Imagen = base64
                };
                _context.ImagenesEmpleados.Add(img);
                await _context.SaveChangesAsync();
            }

            Mensaje = "Empleado registrado exitosamente.";
            return RedirectToPage();
        }

        private void CargarPuestos()
        {
            Puestos = _context.PuestoEmpleados.OrderBy(p => p.Puesto).ToList();
        }
    }
}
