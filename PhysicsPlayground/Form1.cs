using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PhysicsPlayground
{
    public enum RunState
    {
        None = 0,
        Running = 1,
        Paused = 2,
        Stopped = 3
    }

    public partial class Form1 : Form
    {
        Graphics _graphics;
        Pen _scalePen;

        RunState _playState;

        Timer _clock;
        float _xMeters;
        float _yMeters = 100;
        float _pixelsPerMeter;
        int _metersInScale = 5;
        Point _location;
        MovementEquation _movementEquation;
        int _tick = 10;
        float _factor;

        TimeSpan _baseTime = TimeSpan.Zero;
        DateTime _startTime;
        public TimeSpan RunTime { get => _playState switch
            {
                RunState.Running => _baseTime + DateTime.Now.Subtract(_startTime),
                RunState.Paused => _baseTime,
                RunState.Stopped => TimeSpan.Zero,
                _ => throw new Exception("Internal Error")
            };
        }

        public Form1()
        {
            InitializeComponent();

            DoubleBuffered = true;
            _graphics = CreateGraphics();
            if (_xMeters != 0)
            {
                _pixelsPerMeter = _graphics.VisibleClipBounds.Width / _xMeters;
                _yMeters = _graphics.VisibleClipBounds.Height / _pixelsPerMeter;
            }
            else if (_yMeters != 0)
            {
                _pixelsPerMeter = _graphics.VisibleClipBounds.Height / _yMeters;
                _xMeters = _graphics.VisibleClipBounds.Width / _pixelsPerMeter;
            }
            else
                throw new Exception("Choose meters in x or y axis");
            
            _factor = (float)(_tick / TimeSpan.FromSeconds(1).TotalMilliseconds)*_pixelsPerMeter;

            _clock = new Timer();
            _clock.Interval = _tick;
            _clock.Tick += (o, e) => OnClock();

            _scalePen = new Pen(Color.Blue, 10);

            _movementEquation = new MovementEquation() { X0 = 0, Y0 = 2 * _yMeters / 3, Ax0 = 0, Ay0 = 9.8f, Vx0 = 10, Vy0 = -10 };

            _playState = RunState.Stopped;
            UpdateObjects();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Action action = _playState switch
            {
                RunState.Running => Pause,
                RunState.Paused => Resume,
                RunState.Stopped => Start,
                _ => throw new Exception("Internal error")
            };

            action();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void Pause()
        {
            startButton.Text = "Resume";
            _baseTime = RunTime;
            _playState = RunState.Paused;
            _clock.Stop();
        }

        private void Start()
        {
            Resume();
        }

        private void Resume()
        {
            startButton.Text = "Pause";
            _playState = RunState.Running;
            _startTime = DateTime.Now;
            _clock.Start();
        }

        private void Stop()
        {
            startButton.Text = "Start";
            _playState = RunState.Stopped;
            _clock.Stop();
            _baseTime = TimeSpan.Zero;
            UpdateObjects();
            UpdateGraphics();
        }

        private void OnClock()
        {
            UpdateObjects();

            UpdateGraphics();
        }

        private void UpdateObjects()
        {
            double t = RunTime.TotalSeconds;

            (double x, double y) = _movementEquation.GetLocationInTime(t);

            _location.X = (int)(x * _pixelsPerMeter);
            _location.Y = (int)(y * _pixelsPerMeter);
        }

        private void UpdateGraphics()
        {
            timerLabel.Text = RunTime.ToString(@"hh\:mm\:ss\:ff");
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawScale(e);

            DrawBall(e);
        }

        private void DrawScale(PaintEventArgs e)
        {
            e.Graphics.DrawLine(_scalePen,
                0, 0,
                0, _pixelsPerMeter * _metersInScale);
        }

        private void DrawBall(PaintEventArgs e)
        {
            Point p = new Point(_location.X, _location.Y);
            p.Offset(-10, -10);

            e.Graphics.FillEllipse(Brushes.Red,
                new Rectangle(
                    p,
                    new Size(20, 20)
                    )
                );
        }

    }
}
