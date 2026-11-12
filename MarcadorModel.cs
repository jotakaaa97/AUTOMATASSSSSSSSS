using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace ProyectoAutomatas.Models
{
    [Table("marcador")]
    public class MarcadorModel
    {
        [PrimaryKey, NotNull, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int Tiempo { get; set; } = 0;
        [NotNull]
        public string Jugador { get; set; } = "Jugador 1";
        [NotNull]
        public int Ronda { get; set; } = 0;
        [NotNull]
        public int Nivel { get; set; } = 1;
    }
}
