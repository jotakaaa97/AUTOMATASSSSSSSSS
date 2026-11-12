using System;
using System.Collections.Generic;
using System.Text;

namespace ProyectoAutomatas.Models
{
    public class PartidaModel
    {
        public NivelModel Nivel { get; set; } = new NivelModel() { Numero = 1, SecuenciaMáxima=1, Tiempo=20};
        public RondaModel UltimaRondaJugada { get; set; } = new RondaModel { Numero_Ronda = 0, Secuencia = "" };
        public string Jugador { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsWinner { get; set; }

        public char GenerarColor()
        {
            Random random = new();
            int num = random.Next(0, 4);
            char color = num == 0 ? 'R' : num == 1 ? 'G' :
                         num == 2 ? 'B' : 'Y';
            return color;
        }

        public void IniciarNivel(int nivel)
        {
            IsGameOver = false;
            if (nivel == 1)
                Nivel = new NivelModel()
                {
                    Numero = 1,
                    SecuenciaMáxima = 3,
                    Tiempo = 180
                };
            else
                Nivel = new NivelModel()
                {
                    Numero = 2,
                    SecuenciaMáxima = 20,
                    Tiempo = 180
                };
            UltimaRondaJugada = new RondaModel()
            {
                Numero_Ronda = 0,
                Secuencia = ""
            };
            CambiarRonda();
        }

        public void CambiarRonda()
        {
            if (Nivel.SecuenciaMáxima < UltimaRondaJugada.Numero_Ronda)
            {
                IsWinner = true;
                TerminarJuego(true);
                return;
            }

            UltimaRondaJugada.Numero_Ronda++;
            UltimaRondaJugada.Secuencia += GenerarColor();
        }

        public void TerminarJuego(bool gano)
        {
            if (!gano)
            {
                IsGameOver = true;
                IsWinner = false;
            }
            else 
            {
                IsWinner = true;
                IsGameOver = false;
            }
        }

        public void Tick()
        {
            if (Nivel.Tiempo == 0)
                TerminarJuego(false);
            Nivel.Tiempo--;
        }
    }
}
