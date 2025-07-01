using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoRH2025.Data;
using ProyectoRH2025.Models;

namespace ProyectoRH2025.Pages.Operadores
{
    public class BuscarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public BuscarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Empleado> Empleados { get; set; } = new List<Empleado>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        private int? _selectedCompany = null;

        [BindProperty(SupportsGet = true)]
        public int SelectedCompany
        {
            get { return _selectedCompany ?? 0; }
            set
            {
                if (value == 1)
                {
                    _selectedCompany = 1;
                }
                else if (value == 2)
                {
                    _selectedCompany = 2;
                }
                else
                {
                    _selectedCompany = null;
                }
            }
        }

        // Nuevas propiedades para mensajes
        public string SearchMessage { get; set; }
        public string SearchType { get; set; }

        private void SetSearchMessage()
        {
            if (string.IsNullOrEmpty(SearchTerm) && !Empleados.Any())
            {
                SearchMessage = "Ingrese un término de búsqueda para comenzar";
                SearchType = "info";
            }
            else if (!string.IsNullOrEmpty(SearchTerm) && !Empleados.Any())
            {
                SearchMessage = $"No se encontraron empleados que coincidan con '{SearchTerm}'";
                if (_selectedCompany == null)
                {
                    SearchMessage += ". Seleccione al menos una compañía";
                }
                SearchType = "warning";
            }
            else if (Empleados.Any())
            {
                SearchMessage = $"Se encontraron {Empleados.Count} empleados";
                SearchType = "success";
            }
        }

        public async Task OnGetAsync(int? selectedCompany = null)
        {
            if (selectedCompany.HasValue)
            {
                SelectedCompany = selectedCompany.Value;
            }

            var query = _context.Empleados.AsQueryable();

            query = query.Where(e => e.Status == 1);

            if (_selectedCompany == 1)
            {
                query = query.Where(e => e.CodClientes == "1");
            }
            else if (_selectedCompany == 2)
            {
                query = query.Where(e => e.CodClientes == "2");
            }

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(e =>
                    (e.Reloj.HasValue && e.Reloj.Value.ToString().Contains(SearchTerm)) ||
                    (e.Names != null && e.Names.Contains(SearchTerm)) ||
                    (e.Apellido != null && e.Apellido.Contains(SearchTerm)) ||
                    (e.Apellido2 != null && e.Apellido2.Contains(SearchTerm)) ||
                    (e.Email != null && e.Email.Contains(SearchTerm))
                );
            }

            Empleados = await query.ToListAsync();
            SetSearchMessage();
        }
    }
}