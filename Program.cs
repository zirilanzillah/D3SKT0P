using System;
using System.Drawing;
using System.IO; // Hinzugefügt für Path-Operationen
using System.Security.Cryptography;
using System.Threading.Tasks; // Hinzugefügt für asynchrone Operationen
using System.Windows.Forms;
using System.Media; // Hinzugefügt für SoundPlayer
using D3SKT0P.Properties;

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

    // Form-Klassen definieren
    public partial class TransparentOverlay : Form
    {
        private Image _animatedGif = Resources.EselRunningToRight;
        private bool _isAnimating = false;
        public int speed = 15;
        private bool _isSoundPlaying = false;

        private readonly Random random = new();
        private readonly SoundPlayer player = new(Resources.ia);
        private readonly System.Windows.Forms.Timer _fallTimer = new();

        // Variablen für Drag & Drop und Gravitation
        private bool _isDragging = false;
        private bool _isFalling = false;
        private Point _dragStartPoint;
        private Direction _currentDirection = Direction.Right;
        // Variablen zur Erkennung der Drag-Richtung und zur Optimierung
        private int _lastMouseX;
        //private string _currentGifPath = "";
        private Image _currentGif;

        public TransparentOverlay()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.BackColor = Color.LimeGreen;
            this.TransparencyKey = Color.LimeGreen;

            try
            {
                //  .gifs zur Prüfung
                //if (!File.Exists(_EselRunningToRight) ||
                //    !File.Exists(_EselRunningToLeft) ||
                //    !File.Exists(_EselWalkingToLeft) ||
                //    !File.Exists(_EselWalkingToRight) ||
                //    !File.Exists(_EselBreathingToLeft) ||
                //    !File.Exists(_EselBreathingToRight) ||
                //    !File.Exists(_EselFallingToLeft) ||
                //    !File.Exists(_EselFallingToRight) ||
                //    !File.Exists(_EselChattingToLeft) ||
                //    !File.Exists(_EselChattingToRight))
                //    throw new FileNotFoundException("Eine oder mehrere Bilddateien wurden nicht gefunden.");

                // Bei jedem mal "fallen" wird der Esel 5 px schneller. Jedes mal wird ein Handler hinzugefügt und zusammen mit allen alten ausgeführt.
                // Darum wird der Handler nur einmal hinzugefügt und später nur noch gestartet und gestoppte.
                _fallTimer.Interval = 15;
                _fallTimer.Tick += FallTimer_Tick;

                // Start mit der laufenden Animation
                //SetAnimation(_EselRunningToRight);
                SetAnimation(Resources.EselRunningToRight);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden des Bildes: {ex.Message}", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
                return;
            }

            if (_animatedGif != null)
            {
                this.Size = _animatedGif.Size;
            }
            else
            {
                this.Size = new Size(0, 0); // Setze eine Standardgröße, falls das .gif nicht geladen werden kann
            }
            this.Paint += new PaintEventHandler(TransparentOverlay_Paint);
            this.FormClosed += new FormClosedEventHandler(TransparentOverlay_FormClosed);
            this.Load += TransparentOverlay_Load;

            this.MouseDown += new MouseEventHandler(Form_MouseDown);
            this.MouseMove += new MouseEventHandler(Form_MouseMove);
            this.MouseUp += new MouseEventHandler(Form_MouseUp);

            // Initialize non-nullable fields to default values  
            //_fallTimer = new System.Windows.Forms.Timer();
            _currentGif = Resources.EselRunningToRight;
        }

        private async void TransparentOverlay_Load(object? sender, EventArgs e)
        {
            await AnimateMovementAcrossScreen();
        }

        // Konstanten definieren für Richtung und Gewschwindigkeit
        public enum Direction { Left, Right }
        public const int WalkSpeed = 50;
        public const int RunSpeed = 15;


        // Diese Methode verhindert das unnötige Neuladen derselben .gif-Datei.
        private void SetAnimation(Image newGif)
        {
            // Nur ändern, wenn es ein neuer Pfad ist und die Datei existiert.
            //if (_currentGifPath == newGifPath || !File.Exists(newGifPath))
            if (_currentGif == newGif)
            {
                return;
            }

            try
            {
                StopAnimation();
                //_animatedGif?.Dispose();
                //_animatedGif = Image.FromFile(newGifPath);
                //_currentGifPath = newGifPath; // den aktuellen Pfad merken
                _animatedGif = newGif; 
                _currentGif = newGif; // den aktuellen .gif merken
                this.Size = _animatedGif.Size; // Größe an das neue .gif anpassen falls später ein springender Esel ein höher als 64 px .gif verwendet
                StartAnimation();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Setzen der Animation: {ex.Message}");
            }
        }


        // drag & drop Methode

        private void Form_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = true;
                _isFalling = false;
                _fallTimer?.Stop();
                _dragStartPoint = new Point(e.X, e.Y);

                if (!_isSoundPlaying)
                {
                    try
                    {
                        player.PlayLooping();
                        _isSoundPlaying = true;
                    }
                    catch (Exception ex)
                    {
                        // Fehler anzeigen, wenn die Sound-Datei nicht geladen werden kann
                        MessageBox.Show($"Fehler beim Abspielen des Sounds: {ex.Message}");
                    }
                }

                // speichert die initiale Mausposition und setze die "Chat"-Animation
                _lastMouseX = e.X;
                // setze die Animation basierend auf der aktuellen Richtung des Esels
                //SetAnimation(_currentDirection == Direction.Right ? _EselChattingToRight : _EselChattingToLeft);
                SetAnimation(_currentDirection == Direction.Right ? Resources.EselChattingToRight : Resources.EselChattingToLeft);
            }
        }


        private void Form_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // Wird der Esel nach rechts oder links gezogen per drag & drop?
                if (e.X < _lastMouseX)
                {
                    //SetAnimation(_EselChattingToRight);
                    SetAnimation(Resources.EselChattingToRight);
                    _currentDirection = Direction.Right; // Richtung für später merken
                }
                else if (e.X > _lastMouseX)
                {
                    //SetAnimation(_EselChattingToLeft);
                    SetAnimation(Resources.EselChattingToLeft);
                    _currentDirection = Direction.Left; // Richtung für später merken
                }
                _lastMouseX = e.X; // letzte Position aktualisieren

                // Position des Fensters aktualisieren (bestehende Logik)
                Point newPoint = this.PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(newPoint.X - _dragStartPoint.X, newPoint.Y - _dragStartPoint.Y);
            }
        }

        private void Form_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _isDragging = false;

                if (_isSoundPlaying)
                {
                    player.Stop();
                    _isSoundPlaying = false;
                }

                // Übergang von der drag & drop Animation zur Fall Animation
                    //SetAnimation(_currentDirection == Direction.Right ? _EselRunningToRight : _EselRunningToLeft);
                    SetAnimation(_currentDirection == Direction.Right ? Resources.EselRunningToRight : Resources.EselRunningToLeft);
                speed = RunSpeed;         

                StartFalling();
            }
        }

        // Esel fällt zurück auf die Taskleiste
        private void StartFalling()
        {
            _isFalling = true;
            //_fallTimer = new System.Windows.Forms.Timer();
            //_fallTimer.Interval = 15; // Alle 15ms die Position aktualisieren
            //_fallTimer.Tick += FallTimer_Tick;
            _fallTimer.Start();
            //if (_isFalling) SetAnimation(_currentDirection == Direction.Right ? _EselFallingToLeft : _EselFallingToRight);
            if (_isFalling) SetAnimation(_currentDirection == Direction.Right ? Resources.EselFallingToRight : Resources.EselFallingToLeft);
        }
        private void FallTimer_Tick(object? sender, EventArgs e)
        {
            // Unterkante des Bildschirms (Arbeitsbereich, also ohne Taskleiste)
            if (Screen.PrimaryScreen != null)
            { 
                int groundLevel = Screen.PrimaryScreen.WorkingArea.Bottom;

                // Prüfen, ob der Esel den "Boden" erreicht hat
                if (this.Bottom >= groundLevel)
                {
                    // Position exakt auf den Boden setzen
                    this.Top = groundLevel - this.Height;
                    _isFalling = false;
                    _fallTimer.Stop();
                    //_fallTimer.Dispose();
                    //SetAnimation(_currentDirection == Direction.Right ? _EselRunningToRight : _EselRunningToLeft);
                    SetAnimation(_currentDirection == Direction.Right ? Resources.EselRunningToRight : Resources.EselRunningToLeft);
                    speed = RunSpeed;
                }
                else
                {
                    // weiter fallen lassen
                    this.Top += 5; // Fallgeschwindigkeit
                }
            }
        }


        private async Task AnimateMovementAcrossScreen()
        {
            int screenWidth = Screen.PrimaryScreen?.Bounds.Width ?? 0; // Falls kein Bildschirm gefunden wird, setzen wir die Breite auf 0
            if (Screen.PrimaryScreen != null)
            {
                this.Location = new Point(0, Screen.PrimaryScreen.WorkingArea.Bottom - this.Height);
            }
            else
            {
                this.Location = new Point(0, 0); // Fallback-Position, falls kein Bildschirm gefunden wird
            }
            int solangestehenbleiben = random.Next(3000, 7000);

            while (true)
            {
                if (_isDragging || _isFalling)
                {
                    await Task.Delay(100);
                    continue;
                }

                // Der Esel bewegt sich nach rechts.
                while ((this.Left < screenWidth - 172) && _currentDirection == Direction.Right) // .gif Width = 64 px; this.Width = 136 px
                {
                    if (_isDragging || _isFalling) { await Task.Delay(100); continue; }

                    if (random.Next(500) < 1)
                    {
                        //SetAnimation(_EselBreathingToRight);
                        SetAnimation(Resources.EselBreathingToRight);
                        await Task.Delay(solangestehenbleiben);
                        // entscheiden, ob er danach geht oder rennt
                        //if (random.Next(2) == 0) { speed = WalkSpeed; SetAnimation(_EselWalkingToRight); }
                        if (random.Next(3) == 0) { speed = WalkSpeed; SetAnimation(Resources.EselWalkingToRight); }
                        //else { speed = RunSpeed; SetAnimation(_EselRunningToRight); }
                        else { speed = RunSpeed; SetAnimation(Resources.EselRunningToRight); }
                    }
                    this.Left += 1;
                    await Task.Delay(speed);
                }
                // Der Esel soll 100 px vor dem Bildschirmrand seine Geschwindigkeit auf WalkSpeed reduzieren.
                while ((this.Left >= screenWidth - 172) && (this.Left < screenWidth - 72) && _currentDirection == Direction.Right)
                {
                    if (_isDragging || _isFalling) { await Task.Delay(100); continue; }
                    if (speed != WalkSpeed) 
                        {
                        speed = WalkSpeed;
                        SetAnimation(Resources.EselWalkingToRight); 
                        }
                    this.Left += 1;
                    await Task.Delay(speed);
                }


                if (this.Left >= screenWidth - 72) await ChangeDirection(Direction.Left, speed);

                // Der Esel bewegt sich nach links.
                while (this.Left > 100 && _currentDirection == Direction.Left)
                {
                    if (_isDragging || _isFalling) { await Task.Delay(100); continue; }

                    if (random.Next(500) < 1)
                    {
                        //SetAnimation(_EselBreathingToLeft);
                        SetAnimation(Resources.EselBreathingToLeft);
                        await Task.Delay(solangestehenbleiben);
                        //if (random.Next(2) == 0) { speed = WalkSpeed; SetAnimation(_EselWalkingToLeft); }
                        if (random.Next(3) == 0) { speed = WalkSpeed; SetAnimation(Resources.EselWalkingToLeft); }
                        //else { speed = RunSpeed; SetAnimation(_EselRunningToLeft); }
                        else { speed = RunSpeed; SetAnimation(Resources.EselRunningToLeft); }
                    }
                    this.Left -= 1;
                    await Task.Delay(speed);
                }
                // Der Esel soll 100 px vor dem Bildschirmrand seine Geschwindigkeit auf WalkSpeed reduzieren.
                while ((this.Left <= 100) && (this.Left > 0) && _currentDirection == Direction.Left)
                {
                    if (_isDragging || _isFalling) { await Task.Delay(100); continue; }
                    if (speed != WalkSpeed)
                    {
                        speed = WalkSpeed;
                        SetAnimation(Resources.EselWalkingToLeft);
                    }
                    this.Left -= 1;
                    await Task.Delay(speed);
                }

                if (this.Left <= 0) await ChangeDirection(Direction.Right, speed);

                await Task.Delay(10);
            }
        }

        private async Task ChangeDirection(Direction newDirection, int Geschwindigkeit)
        {
            _currentDirection = newDirection;
            //string newPath = "";
            Image newImage;

            if (Geschwindigkeit == WalkSpeed)
            {
                //newPath = (newDirection == Direction.Right) ? _EselWalkingToRight : _EselWalkingToLeft;
                newImage = (newDirection == Direction.Right) ? Resources.EselWalkingToRight : Resources.EselWalkingToLeft;
            }
            else // RunSpeed
            {
                //newPath = (newDirection == Direction.Right) ? _EselRunningToRight : _EselRunningToLeft;
                newImage = (newDirection == Direction.Right) ? Resources.EselRunningToRight : Resources.EselRunningToLeft;
            }

            //SetAnimation(newPath);
            SetAnimation(newImage);
            await Task.CompletedTask; // Ersetzt das leere Warten, da SetAnimation synchron ist
        }

        // Hilfsmethoden
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

            private void OnFrameChanged(object? sender, EventArgs e)
            {
                this.BeginInvoke((Action)(() => this.Invalidate()));
            }

            private void TransparentOverlay_Paint(object? sender, PaintEventArgs e)
            {
                if (_animatedGif == null) return;
                if (_isAnimating)
                {
                    ImageAnimator.UpdateFrames(_animatedGif);
                }
                e.Graphics.FillRectangle(new SolidBrush(this.TransparencyKey), this.ClientRectangle);
                e.Graphics.DrawImage(_animatedGif, Point.Empty);
            }

            private void TransparentOverlay_FormClosed(object? sender, FormClosedEventArgs e)
            {
                StopAnimation();
                _animatedGif?.Dispose();
            }
        }


    }