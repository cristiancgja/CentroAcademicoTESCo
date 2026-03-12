using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CentroAcademicoSQLite.Models;
using CentroAcademicoSQLite.ViewModels;

namespace CentroAcademicoSQLite.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> Login()
    {
        await _signInManager.SignOutAsync();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            ViewBag.Error = "Usuario no encontrado";
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(user, password, false, false);

        if (result.Succeeded)
        {
            if (user.DebeCambiarPassword)
                return RedirectToAction("CambiarPassword");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Admin", "Dashboard");

            if (await _userManager.IsInRoleAsync(user, "Alumno"))
                return RedirectToAction("Alumno", "Dashboard");

            if (await _userManager.IsInRoleAsync(user, "Profesor"))
                return RedirectToAction("Profesor", "Dashboard");

            if (await _userManager.IsInRoleAsync(user, "Psicologo"))
                return RedirectToAction("Psicologo", "Dashboard");

            if (await _userManager.IsInRoleAsync(user, "Asesor"))
                return RedirectToAction("Asesor", "Dashboard");
        }

        ViewBag.Error = "Credenciales incorrectas";
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            Nombre = model.Nombre,
            Apellidos = model.Apellidos,
            Activo = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            ViewBag.Error = "Error creando usuario";
            return View(model);
        }

        if (model.ClaveProfesor == "TESCO2026")
        {
            await _userManager.AddToRoleAsync(user, "Profesor");
            user.DebeCambiarPassword = true;
        }
        else if (model.ClaveProfesor == "PSICOLOGO2026")
        {
            await _userManager.AddToRoleAsync(user, "Psicologo");
            user.DebeCambiarPassword = true;
        }
        else if (model.ClaveProfesor == "ASESOR2026")
        {
            await _userManager.AddToRoleAsync(user, "Asesor");
            user.DebeCambiarPassword = true;
        }
        else if (model.ClaveProfesor == "ADMIN2027")
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }
        else
        {
            user.Carrera = model.Carrera;
            user.Matricula = model.Matricula;
            await _userManager.AddToRoleAsync(user, "Alumno");
        }

        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Usuario creado correctamente";

        return RedirectToAction("Login");
    }

    public IActionResult CambiarPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CambiarPassword(string nuevaPassword)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return RedirectToAction("Login");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, nuevaPassword);

        if (result.Succeeded)
        {
            user.DebeCambiarPassword = false;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Login");
        }

        ViewBag.Error = "Error al cambiar contraseña";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}