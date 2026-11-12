using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using ProyectoAutomatas.Models;
using ProyectoAutomatas.Repositories;

namespace ProyectoAutomatas.ViewModels
{
    public class JuegoViewModel : INotifyPropertyChanged
    {
        readonly PartidaModel _partida = new();
        MarcadorRepository _repo = new();
        public string SecuenciaInput { get; set; } = "";
        public int Nivel => _partida.Nivel.Numero;
        public string SecuenciaActual => _partida.UltimaRondaJugada.Secuencia;
        public int Ronda => _partida.UltimaRondaJugada.Numero_Ronda;
        public int TiempoRestante => _partida.Nivel.Tiempo;
        public string Jugador => _partida.Jugador;
        public bool IsGameOver => _partida.IsGameOver;
        public bool IsWinner => _partida.IsWinner;
        public int TiempoTranscurrido { get; set; } = 0;
        public string NombreJugador { get; set; } = "";
        public bool MostrandoSecuencia { get; set; } = false;
        public string? ColorActivo { get; set; }
        public bool EstaJugando { get; set; } = false;
        public DispatcherTimer Temporizador { get; set; }
        public string Vista { get; set; } = "Inicio";
        public string TituloResultado { get; set; } = "";
        public string MensajeResultado { get; set; } = "";
        public string Error { get; set; }
        public List<MarcadorModel>? Partidas { get; set; } = null;

        //Comandos
        public ICommand IniciarJuegoCommand { get; set; }
        public ICommand CambiarDeNivelCommand { get; set; }
        public ICommand AgregarSecuenciaCommand { get; set; }
        public ICommand VerMarcadorCommand { get; set; }
        public ICommand VerInicioCommand { get; set; }
        public ICommand VerResultadosCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public async void IniciarJuego()
        {
            if (string.IsNullOrWhiteSpace(NombreJugador)) 
            {
                Error = "Ingrese un nombre de jugador.";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Error)));
                return;
            }
            Vista = "Juego";
            SecuenciaInput = "";
            EstaJugando = true;
            _partida.IniciarNivel(1);
            _partida.Jugador = NombreJugador;
            Temporizador.Start();
            await Task.Delay(500);
            MostrarSecuencia();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public async void CambiarDeNivel() 
        {
            _partida.IniciarNivel(_partida.Nivel.Numero + 1);
            Vista = "Juego";
            SecuenciaInput = "";
            EstaJugando = true;
            Temporizador.Start();
            await Task.Delay(500);
            MostrarSecuencia();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public async void AgregarSecuencia(string color)
        {
            SecuenciaInput += color;
            EncenderColorAsync(color);
            ValidarSecuencia();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(SecuenciaInput));
        }
        public void MostrarMarcador() 
        {
            Partidas = new List<MarcadorModel>(_repo.GetMarcadorByNivel(Nivel));
            Vista = "Marcador";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Partidas)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Vista)));
        }
        public void VerResultados() 
        {
            Vista = "PantallaFinal";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Vista)));
        }
        public void VerInicio() 
        {
            Vista = "Inicio";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Vista)));
        }
        public void AgregarPartida() 
        {
            var partida = new MarcadorModel() 
            {
                Nivel = Nivel,
                Jugador = Jugador,
                Ronda = Ronda,
                Tiempo = TiempoTranscurrido
            };
            _repo.Agregar(partida);
        }
        private async void CambiarRonda()
        {
            if (SecuenciaActual.Length < _partida.Nivel.SecuenciaMáxima)
            {
                SecuenciaInput = "";
                _partida.CambiarRonda();
                await Task.Delay(700);
                MostrarSecuencia();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecuenciaInput)));
                return;
            }

            TerminarJuego(true);
        }


        private async void ValidarSecuencia()
        {
            if (SecuenciaActual[SecuenciaInput.Length - 1] != SecuenciaInput[SecuenciaInput.Length - 1]) 
            {
                TerminarJuego(false);
                return;
            }
            if (SecuenciaActual.Length == SecuenciaInput.Length) CambiarRonda();
        }

        private void TerminarJuego(bool gano)
        {
            _partida.TerminarJuego(gano);
            ColorActivo = null;
            EstaJugando = false;
            Temporizador.Stop();
            if (IsWinner && Nivel == 1)
            {
                TituloResultado = "!Has ganado!";
                MensajeResultado = $"¡Has superado el nivel {Nivel}!";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TituloResultado)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeResultado)));
            }
            else if (IsWinner && Nivel == 2)
            {
                TituloResultado = "!Has ganado!";
                MensajeResultado = $"¡Has completado todos los niveles!.";
                _partida.TerminarJuego(false);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TituloResultado)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeResultado)));
            }
            else
            {
                TituloResultado = "!Has perdido!";
                MensajeResultado = $"Siempre puedes volver a intentarlo.";
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TituloResultado)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensajeResultado)));
            }
            Vista = "PantallaFinal";
            AgregarPartida();
            TiempoTranscurrido = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public void SubirPartida() 
        {
            var marcador = new MarcadorModel() 
            {
                Jugador = _partida.Jugador,
                Nivel = _partida.Nivel.Numero,
                Ronda = _partida.UltimaRondaJugada.Numero_Ronda,
                Tiempo = TiempoTranscurrido
            };
            _repo.Agregar(marcador);
        }

        public async void MostrarSecuencia()
        {
            MostrandoSecuencia = true;
            int tiempo = 0;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostrandoSecuencia)));
            foreach (char color in SecuenciaActual) 
            {
                EncenderColorAsync(color.ToString());
                await Task.Delay(700);
                tiempo += 700;
            }
            MostrandoSecuencia = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostrandoSecuencia)));
        }

        private async void EncenderColorAsync(string color)
        {
            ColorActivo = color;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorActivo)));
            await Task.Delay(400);
            ColorActivo = null;
            await Task.Delay(150);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorActivo)));
        }

        public JuegoViewModel()
        {
            Temporizador = new DispatcherTimer();
            Temporizador.Tick += Temporizador_Tick;
            Temporizador.Interval = TimeSpan.FromSeconds(1);
            IniciarJuegoCommand = new RelayCommand(IniciarJuego);
            AgregarSecuenciaCommand = new RelayCommand<string>(AgregarSecuencia);
            CambiarDeNivelCommand = new RelayCommand(CambiarDeNivel);
            VerMarcadorCommand = new RelayCommand(MostrarMarcador);
            VerResultadosCommand = new RelayCommand(VerResultados);
            CambiarDeNivelCommand = new RelayCommand(CambiarDeNivel);
            VerInicioCommand = new RelayCommand(VerInicio);
        }

        private void Temporizador_Tick(object? sender, EventArgs e)
        {
            if (TiempoRestante == 0) 
            {
                Temporizador.Stop();
                TerminarJuego(false);
            }
            _partida.Tick();
            TiempoTranscurrido++;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
