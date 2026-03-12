using Microsoft.AspNetCore.Identity;

namespace CentroAcademicoSQLite.Models;

public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Matricula { get; set; } = string.Empty;
    public string Estatus { get; set; } = "Inicio de Proceso";
    public string Area { get; set; } = string.Empty;
    public string ProfesionalAsignado { get; set; } = string.Empty;
    public bool? Activo { get; set; }
    public string? Carrera { get; set; }
    public string? TipoProfesor { get; set; }
    public string? Turno { get; set; }
    public bool DebeCambiarPassword { get; set; } = false;
}