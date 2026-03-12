using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentroAcademicoSQLite.Models;
using CentroAcademicoSQLite.Data;

namespace CentroAcademicoSQLite.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var citas = _context.Citas
                .Include(c => c.Usuario)
                .OrderBy(c => c.FechaHora)
                .ToList();

            ViewBag.TotalAlumnos = _userManager.Users.Count();
            ViewBag.TotalCitas = citas.Count;

            return View(citas);
        }

        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> Alumno()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var citas = _context.Citas
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == user.Id)
                .OrderBy(c => c.FechaHora)
                .ToList();

            return View(citas);
        }

        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> Profesor()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var citas = _context.Citas
                .Include(c => c.Usuario)
                .Where(c => c.Profesional.Contains(user.Nombre))
                .OrderBy(c => c.FechaHora)
                .ToList();

            ViewBag.TotalAlumnos = _userManager.Users.Count();
            ViewBag.TotalCitas = citas.Count;

            return View(citas);
        }

        [Authorize(Roles = "Psicologo")]
        public async Task<IActionResult> Psicologo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var citas = _context.Citas
                .Include(c => c.Usuario)
                .Where(c => c.Profesional.Contains(user.Nombre))
                .OrderBy(c => c.FechaHora)
                .ToList();

            ViewBag.TotalCitas = citas.Count;

            return View(citas);
        }

        [Authorize(Roles = "Asesor")]
        public IActionResult Asesor()
        {
            var citas = _context.Citas
                .Include(c => c.Usuario)
                .OrderBy(c => c.FechaHora)
                .ToList();

            ViewBag.TotalCitas = citas.Count;

            return View(citas);
        }
    }
}