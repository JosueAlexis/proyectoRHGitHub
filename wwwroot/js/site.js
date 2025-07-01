// Función para alternar los submodules
function toggleSubmodules(parent) {
    const module = document.querySelector(`.module:has(.sidebar-link[onclick*="${parent}"])`);
    const submodule = module.querySelector('.submodule');

    // Si el submodule está visible, lo ocultamos
    if (submodule.style.display === 'block') {
        submodule.style.display = 'none';
        module.classList.remove('active');
    } else {
        // Cerrar todos los otros submodules primero
        document.querySelectorAll('.submodule').forEach(sub => {
            sub.style.display = 'none';
            sub.closest('.module').classList.remove('active');
        });

        // Abrir el seleccionado
        submodule.style.display = 'block';
        module.classList.add('active');
    }
}

// Mantener el submenú activo según la ruta actual
document.addEventListener('DOMContentLoaded', function () {
    const currentPath = window.location.pathname.toLowerCase();
    const links = document.querySelectorAll('.submodule a');

    links.forEach(link => {
        const href = link.getAttribute('href').toLowerCase();
        if (currentPath.includes(href)) {
            const parentModule = link.closest('.submodule').getAttribute('data-parent');
            if (parentModule) {
                toggleSubmodules(parentModule);
            }
        }
    });
});