using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CentroAcademicoSQLite.Models
{
    public class Cita
    {
        public int Id { get; set; }

        public DateTime FechaHora { get; set; }

        public string Lugar { get; set; } = string.Empty;

        public string Profesional { get; set; } = string.Empty;

        public string? Comentarios { get; set; }

        public string? EstatusProceso { get; set; }

        public string? Archivo { get; set; }

        public string UsuarioId { get; set; } = string.Empty;

        public bool Asistio { get; set; } = false;

        [ForeignKey(nameof(UsuarioId))]
        public ApplicationUser Usuario { get; set; } = null!;
    }
}
