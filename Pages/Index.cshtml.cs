using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoRH2025.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            var idUsuario = HttpContext.Session.GetInt32("idUsuario");

            if (!idUsuario.HasValue)
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

    }
}