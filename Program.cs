using System;
using System.Drawing;
using System.IO; // Hinzugefügt für Path-Operationen
using System.Security.Cryptography;
using System.Threading.Tasks; // Hinzugefügt für asynchrone Operationen
using System.Windows.Forms;

namespace D3SKT0P
{
    // Die Program-Klasse mit dem Haupteinstiegspunkt Main()
    static class Program
    {
        [STAThread] // Wichtig für Windows Forms
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TransparentOverlay());
        }
    }
    // Enum für die Richtung des Esels
    public enum Direction
    {
        Left,
        Right
    }

    // Enum für die Zustände des Esels: Noch nicht eingebaut: Soll später das Einfügen von Springen, Essen, Warten erleichtern.
    public enum EselZustand
    {
        LäuftNachLinks,
        LäuftNachRechts,
        GehtNachLinks,
        GehtNachRechts,
        AtmetNachLinks,
        AtmetNachRechts
    }

    // Form-Klassen definieren
    public partial class TransparentOverlay : Form
    {
        private Image _animatedGif;
        private bool _isAnimating = false;
        public int speed = 15; // Anfangsgeschwindigkeit für Rennen 15 (50 für Gehen)

        // Variablen für die Pfade zu den GIF-Dateien
        private readonly string _EselRunningToLeft = "EselRunningToLeft.gif";
        private readonly string _EselRunningToRight = "EselRunningToRight.gif";
        private readonly string _EselWalkingToLeft = "EselWalkingToLeft.gif";
        private readonly string _EselWalkingToRight = "EselWalkingToRight.gif";
        private readonly string _EselBreathingToLeft = "EselBreathingToLeft.gif";
        private readonly string _EselBreathingToRight = "EselBreathingToRight.gif";

        // Zufallszahlen für die Entscheidung ob und wie lange gewartet wird bis zum Weiterlaufen und wie schnell (gehen / rennen).
        Random random = new Random();


        public TransparentOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None; // Rahmen des Fensters soll nicht sichtbar sein / .FixedSingle mit Rahmen
            this.TopMost = true;                         // Das Fenster im Vordergrund halten
            this.BackColor = Color.LimeGreen;            // Die Farbe, die transparent gemacht wird
            this.TransparencyKey = Color.LimeGreen;      // Diese Farbe wird auf der Form transparent

            // Checken ob alle .gif-Dateien auch da sind
            try
            {
                if (!File.Exists(_EselRunningToRight) || 
                    !File.Exists(_EselRunningToLeft) || 
                    !File.Exists(_EselWalkingToLeft) || 
                    !File.Exists(_EselWalkingToRight) || 
                    !File.Exists(_EselBreathingToLeft) || 
                    !File.Exists(_EselBreathingToRight))
                    throw new FileNotFoundException("Die Bilddatei wurde nicht gefunden.");

                _animatedGif = Image.FromFile(_EselRunningToRight);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Bildes: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Beenden, wenn das Startbild nicht geladen werden kann
                Environment.Exit(1);
                return;
            }

            //MessageBox.Show(this.Width.ToString() + " " + _animatedGif.Width.ToString());  // Form ist 136 px breit, .gif ist 64 px breit Warum?
            this.Size = _animatedGif.Size;         
            this.Paint += new PaintEventHandler(TransparentOverlay_Paint);
            this.FormClosed += new FormClosedEventHandler(TransparentOverlay_FormClosed);
            this.Load += TransparentOverlay_Load;

            StartAnimation();
        }

        private async void TransparentOverlay_Load(object sender, EventArgs e)
        {
            AnimateMovementAcrossScreen();
        }

        // Noch nicht eingebaut: Soll später duplizierten Code für Rechtsbewegungen und Linksbewegungen in einer Methode zusammenfassen.
        private enum MovementDirection
        {
            Left = -1,
            Right = 1
        }

        // Konstanten für die Eselbewegungen definieren
        public const int WalkSpeed = 50;
        public const int RunSpeed = 15;

        // Angepasste Methode, die jetzt die Bilddateien wechselt
        private async Task AnimateMovementAcrossScreen()
        {

            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            // Startposition ganz links unten über der Taskleiste, damit er ins Bild hineinläuft.
            // Der Esel soll auf der Taskleiste stehen, nicht an der unteren Bildschirmkante.
            this.Location = new Point(0, Screen.PrimaryScreen.WorkingArea.Bottom - this.Height); 

            // Wieviele Sekunden soll der Esel stehen
            int solangestehenbleiben = random.Next(3000, 7000); // zwischen 3 s und 7 s stehen bleiben
            int stehenbleibenjaodernein = random.Next(1000);    // 0,1% pro Frame Wahrscheinlichkeit fürs stehen bleiben

            while (true)
            {
                // 1. Nach rechts bewegen
                while (this.Left < screenWidth - 72) // .gif Width = 64 px; this.Width = 136 px
                {
                    if (stehenbleibenjaodernein < 1)
                    {
                        StopAnimation();
                        _animatedGif?.Dispose();
                        _animatedGif = Image.FromFile(_EselBreathingToRight);
                        StartAnimation();

                        await Task.Delay(solangestehenbleiben);

                        StopAnimation();
                        _animatedGif?.Dispose();
                        int runwalk = random.Next(2);  // 0 == gehen; 1 == rennen
                        if (runwalk == 0)
                        {
                            speed = WalkSpeed;
                            _animatedGif = Image.FromFile(_EselWalkingToRight);
                        }
                        if (runwalk == 1)
                        {
                            speed = RunSpeed;
                            _animatedGif = Image.FromFile(_EselRunningToRight);
                        }
                        StartAnimation();
                    }
                    this.Left += 1;
                    await Task.Delay(speed);
                    stehenbleibenjaodernein = random.Next(10000);
                    solangestehenbleiben = random.Next(3000, 7000);
                }

                // 2. Am rechten Rand: Auf das "nach links laufende" Bild wechseln
                ChangeDirection(Direction.Left, speed);

                // 3. Nach links bewegen
                while (this.Left > 0)
                {
                    if (stehenbleibenjaodernein < 1)
                    {
                        StopAnimation();
                        _animatedGif?.Dispose();
                        _animatedGif = Image.FromFile(_EselBreathingToLeft);
                        StartAnimation();

                        await Task.Delay(solangestehenbleiben);

                        StopAnimation();
                        _animatedGif?.Dispose();
                        int runwalk = random.Next(2);
                        if (runwalk == 0)
                        {
                            speed = WalkSpeed;
                            _animatedGif = Image.FromFile(_EselWalkingToLeft);
                        }
                        if (runwalk == 1)
                        {
                            speed = RunSpeed;
                            _animatedGif = Image.FromFile(_EselRunningToLeft);
                        }
                        StartAnimation();
                    }
                    this.Left -= 1;
                    await Task.Delay(speed);
                    stehenbleibenjaodernein = random.Next(1000);
                    solangestehenbleiben = random.Next(3000, 7000);
                }

                // 4. Am linken Rand: Auf das "nach rechts laufende" Bild wechseln
                ChangeDirection(Direction.Right, speed);

            }
        }

        // Eine Hilfsmethode, um die Richtung sauber zu wechseln
        private async Task ChangeDirection(Direction newDirection, int Geschwindigkeit)
        {
            string newPath = _EselWalkingToRight; // Wenn noch kein .gif ausgewählt ist, erst mal nach rechts gehen.

            if (Geschwindigkeit == WalkSpeed)
            {
                if (newDirection == Direction.Right) newPath = _EselWalkingToRight;
                if (newDirection != Direction.Right) newPath = _EselWalkingToLeft;
            }
            else if (Geschwindigkeit == RunSpeed)
            {
                if (newDirection == Direction.Right) newPath = _EselRunningToRight;
                if (newDirection != Direction.Right) newPath = _EselRunningToLeft;
            }

                try
                {
                    if (!File.Exists(newPath))
                    {
                        // Wenn das nächste Bild fehlt, pausieren wir kurz und versuchen es weiter, anstatt abzustürzen.
                        await Task.Delay(2000);
                        return;
                    }

                    StopAnimation();         // Alte Animation stoppen
                    _animatedGif?.Dispose(); // Altes Bild aus dem Speicher entfernen

                    _animatedGif = Image.FromFile(newPath); // Neues Bild laden
                    StartAnimation();                       // Neue Animation starten
                }
                catch (Exception ex)
                {
                    // Loggt den Fehler, aber lässt die Anwendung weiterlaufen
                    Console.WriteLine($"Fehler beim Bildwechsel: {ex.Message}");
                }
        }

        // Hilfsmethoden zum Starten/Stoppen der Animation
        private void StartAnimation()
        {
            if (_animatedGif != null && ImageAnimator.CanAnimate(_animatedGif))
            {
                ImageAnimator.Animate(_animatedGif, this.OnFrameChanged);
                _isAnimating = true;
            }
        }

        private void StopAnimation()
        {
            if (_animatedGif != null && _isAnimating)
            {
                ImageAnimator.StopAnimate(_animatedGif, this.OnFrameChanged);
                _isAnimating = false;
            }
        }

        private void OnFrameChanged(object sender, EventArgs e)
        {
            // Invoke sorgt dafür, dass die UI-Aktualisierung im richtigen Thread stattfindet
            this.BeginInvoke((Action)(() => this.Invalidate()));
        }

        private void TransparentOverlay_Paint(object sender, PaintEventArgs e)
        {
            if (_animatedGif == null) return;

            // Wichtig: UpdateFrames muss im Paint-Event aufgerufen werden
            if (_isAnimating)
            {
                ImageAnimator.UpdateFrames(_animatedGif);
            }

            e.Graphics.FillRectangle(new SolidBrush(this.TransparencyKey), this.ClientRectangle);
            e.Graphics.DrawImage(_animatedGif, Point.Empty);
        }

        private void TransparentOverlay_FormClosed(object sender, FormClosedEventArgs e)
        {
            StopAnimation();
            _animatedGif?.Dispose();
        }
    }


}
