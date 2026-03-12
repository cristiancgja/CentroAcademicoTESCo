using System.ComponentModel.DataAnnotations;

namespace CentroAcademicoSQLite.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
[EmailAddress]
[RegularExpression(@"^[^@\s]+@tesco\.edu\.mx$", 
ErrorMessage = "Debe usar un correo institucional @tesco.edu.mx")]
public string Email { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Carrera { get; set; }

        public string? Matricula { get; set; }

        public string? ClaveProfesor { get; set; }
    }
}