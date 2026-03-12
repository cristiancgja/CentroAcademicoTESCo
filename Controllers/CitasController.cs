using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentroAcademicoSQLite.Data;
using CentroAcademicoSQLite.Models;

namespace CentroAcademicoSQLite.Controllers;

[Authorize]
public class CitasController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CitasController(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    [HttpGet]
    public async Task<IActionResult> BuscarAlumno(string term)
    {
        var lista = new List<object>();

        foreach (var user in _userManager.Users.ToList())
        {
            if (await _userManager.IsInRoleAsync(user, "Alumno") &&
                !string.IsNullOrEmpty(user.Nombre) &&
                user.Nombre.ToLower().Contains(term?.ToLower() ?? ""))
            {
                lista.Add(new
                {
                    id = user.Id,
                    nombre = user.Nombre + " " + user.Apellidos,
                    matricula = user.Matricula,
                    carrera = user.Carrera
                });
            }
        }

        return Json(lista.Take(10));
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    public async Task<IActionResult> Create()
    {
        var profesionales = new List<ApplicationUser>();

        foreach (var user in _userManager.Users.ToList())
        {
            if (await _userManager.IsInRoleAsync(user, "Profesor") ||
                await _userManager.IsInRoleAsync(user, "Psicologo"))
            {
                profesionales.Add(user);
            }
        }

        ViewBag.Profesionales = profesionales;

        return View();
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        string usuarioId,
        string profesionalId,
        DateTime fechaHora,
        string lugar,
        string estatusProceso,
        string comentarios,
        IFormFile archivo)
    {
        if (string.IsNullOrEmpty(usuarioId))
        {
            ViewBag.Error = "Debes seleccionar un alumno";
            return View();
        }

        string profesionalNombre;

        if (User.IsInRole("Profesor") || User.IsInRole("Psicologo"))
        {
            var profesionalUser = await _userManager.GetUserAsync(User);
            profesionalNombre = profesionalUser.Nombre + " " + profesionalUser.Apellidos;
        }
        else
        {
            var profesionalUser = await _userManager.FindByIdAsync(profesionalId);

            if (profesionalUser == null)
            {
                ViewBag.Error = "Profesional no encontrado";
                return View();
            }

            profesionalNombre = profesionalUser.Nombre + " " + profesionalUser.Apellidos;
        }

        var citaExistente = await _context.Citas
            .Where(c => c.FechaHora == fechaHora && c.Profesional == profesionalNombre)
            .FirstOrDefaultAsync();

        if (citaExistente != null)
        {
            ViewBag.Error = "Ese profesional ya tiene una cita en ese horario.";
            return View();
        }

        string nombreArchivo = null;

        if (archivo != null)
        {
            var carpeta = Path.Combine("wwwroot", "archivos");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var ruta = Path.Combine(carpeta, archivo.FileName);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            nombreArchivo = archivo.FileName;
        }

        var cita = new Cita
        {
            UsuarioId = usuarioId,
            FechaHora = fechaHora,
            Lugar = lugar,
            Profesional = profesionalNombre,
            EstatusProceso = estatusProceso,
            Comentarios = comentarios,
            Archivo = nombreArchivo
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        return RedirectToAction("Create");
    }

    [Authorize(Roles = "Alumno")]
    public async Task<IActionResult> MisCitas()
    {
        var user = await _userManager.GetUserAsync(User);

        var citas = await _context.Citas
            .Where(c => c.UsuarioId == user.Id)
            .OrderBy(c => c.FechaHora)
            .ToListAsync();

        return View(citas);
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    public IActionResult Calendario()
    {
        return View();
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    [HttpGet]
    public IActionResult Eventos()
    {
        var eventos = _context.Citas.Select(c => new
        {
            title = c.Profesional,
            start = c.FechaHora,
            lugar = c.Lugar,
            estado = c.Asistio == null ? "Programada" :
                     c.Asistio == true ? "Completada" : "EnEspera"
        }).ToList();

        return Json(eventos);
    }

    [Authorize(Roles = "Profesor,Psicologo,Asesor,Admin")]
    [HttpPost]
    public async Task<IActionResult> Asistencia(int id, bool asistio)
    {
        var cita = await _context.Citas.FindAsync(id);

        cita.Asistio = asistio;

        await _context.SaveChangesAsync();

        if (asistio)
        {
            return RedirectToAction("Atender", new { id = cita.Id });
        }

        return RedirectToAction("Profesor", "Dashboard");
    }

    public async Task<IActionResult> Atender(int id)
    {
        var cita = await _context.Citas
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);

        return View(cita);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarAtencion(int id, string comentarios, IFormFile archivo)
    {
        var cita = await _context.Citas.FindAsync(id);

        cita.Comentarios = comentarios;

        if (archivo != null)
        {
            var carpeta = Path.Combine("wwwroot", "archivos");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var ruta = Path.Combine(carpeta, archivo.FileName);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            cita.Archivo = archivo.FileName;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Profesor", "Dashboard");
    }
}