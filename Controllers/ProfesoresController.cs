using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CentroAcademicoSQLite.Models;

namespace CentroAcademicoSQLite.Controllers;

[Authorize(Roles = "Admin,Asesor")]
public class ProfesoresController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfesoresController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string nombre, string apellidos, string email, string carrera, string tipoProfesor)
    {
        var profesor = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nombre = nombre,
            Apellidos = apellidos,
            Carrera = carrera,
            TipoProfesor = tipoProfesor,
            Activo = true
        };

        var result = await _userManager.CreateAsync(profesor, "Profesor123*");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(profesor, "Profesor");
            return RedirectToAction("Index");
        }

        ViewBag.Error = "Error al crear profesor";
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var profesores = new List<ApplicationUser>();

        foreach (var user in _userManager.Users.ToList())
        {
            if (await _userManager.IsInRoleAsync(user, "Profesor"))
            {
                profesores.Add(user);
            }
        }

        return View(profesores);
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
        user.Carrera = model.Carrera;
        user.TipoProfesor = model.TipoProfesor;

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

        await _userManager.ResetPasswordAsync(user, token, "Nueva123*");

        return RedirectToAction("Index");
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
}