using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CentroAcademicoSQLite.Data;
using CentroAcademicoSQLite.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=CentroAcademico.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Psicologo", "Profesor", "Asesor", "Alumno" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    string adminEmail = "admin@tesco.mx";
    string adminPassword = "Admin123*";

    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            Nombre = "Administrador",
            Apellidos = "General",
            Activo = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    var usuarios = context.Users.ToList();

    foreach (var u in usuarios)
    {
        if (u.Activo == false || u.Activo == null)
        {
            u.Activo = true;
        }

        var rolesUsuario = await userManager.GetRolesAsync(u);

        if (!rolesUsuario.Any())
        {
            await userManager.AddToRoleAsync(u, "Alumno");
        }
    }

    context.SaveChanges();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();