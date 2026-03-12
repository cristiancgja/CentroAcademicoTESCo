using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CentroAcademicoSQLite.Models;
using CentroAcademicoSQLite.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CentroAcademicoSQLite.Controllers;

[Authorize(Roles = "Admin,Psicologo,Asesor,Profesor")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var alumnos = new List<ApplicationUser>();

        var users = _userManager.Users.ToList();

        foreach (var user in users)
        {
            if (await _userManager.IsInRoleAsync(user, "Alumno"))
            {
                alumnos.Add(user);
            }
        }

        return View(alumnos);
    }

    public IActionResult Create()
    {
        return View();
    }

   [HttpPost]
public async Task<IActionResult> Create(
string nombre,
string apellidos,
string matricula,
string email,
string telefono,
string estatus,
string area,
string profesionalAsignado,
string password)
{

var user = new ApplicationUser
{
    UserName = email,
    Email = email,
    PhoneNumber = telefono,
    Nombre = nombre,
    Apellidos = apellidos,
    Matricula = matricula,
    Estatus = estatus,
    Area = area,
    ProfesionalAsignado = profesionalAsignado,
    Activo = true,
    DebeCambiarPassword = true
};

var result = await _userManager.CreateAsync(user, password);

if (result.Succeeded)
{
    await _userManager.AddToRoleAsync(user, "Alumno");
    return RedirectToAction("Index");
}

ViewBag.Error = "Error al crear usuario";
return View();
}

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);

        if (user == null)
            return NotFound();

        user.Nombre = model.Nombre;
        user.Apellidos = model.Apellidos;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.Matricula = model.Matricula;
        user.Estatus = model.Estatus;

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        await _userManager.ResetPasswordAsync(user, token, "Alumno123*");

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Desactivar(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        user.Activo = false;

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Activar(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        user.Activo = true;

        await _userManager.UpdateAsync(user);

        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Asesor")]
    public async Task<IActionResult> CreateProfesional(
        string nombre,
        string apellidos,
        string email,
        string password,
        string rol)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nombre = nombre,
            Apellidos = apellidos,
            Activo = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, rol);
        }

        return RedirectToAction("Create", "Citas");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return NotFound();

        await _userManager.DeleteAsync(user);

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Expediente(string id)
    {
        var citas = await _context.Citas
            .Where(c => c.UsuarioId == id)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

        return View(citas);
    }
    public async Task<IActionResult> Canalizados(string buscar)
{
    var alumnos = _userManager.Users
        .Where(u => u.Estatus == "En Proceso" || u.Estatus == "Culminó Proceso");

    if(!string.IsNullOrEmpty(buscar))
    {
        alumnos = alumnos.Where(a => a.Nombre.Contains(buscar));
    }

    return View(alumnos.ToList());
}
public async Task<IActionResult> ExpedientePDF(string id)
{
    var alumno = await _userManager.FindByIdAsync(id);

    var citas = await _context.Citas
        .Where(c => c.UsuarioId == id)
        .OrderBy(c => c.FechaHora)
        .ToListAsync();

    var pdf = Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header().Text("Expediente del Alumno").FontSize(20).Bold();

            page.Content().Column(col =>
            {
                col.Item().Text($"Nombre: {alumno.Nombre} {alumno.Apellidos}");
                col.Item().Text($"Matrícula: {alumno.Matricula}");
                col.Item().Text("");

                foreach(var cita in citas)
                {
                    col.Item().Text($"Fecha: {cita.FechaHora}");
                    col.Item().Text($"Profesional: {cita.Profesional}");
                    col.Item().Text($"Lugar: {cita.Lugar}");
                    col.Item().Text($"Comentarios: {cita.Comentarios}");
                    col.Item().Text("-----------------------------");
                }
            });

        });
    });

    var stream = new MemoryStream();

    pdf.GeneratePdf(stream);

    stream.Position = 0;

    return File(stream, "application/pdf", "ExpedienteAlumno.pdf");
}
public async Task<IActionResult> Atendidos()
{

var citas = await _context.Citas
.Where(c => c.Asistio == true)
.Include(c=>c.Usuario)
.ToListAsync();

return View(citas);

}
}