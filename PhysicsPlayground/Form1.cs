using PhysicsPlayground.Engine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using PhysicsLibrary.Engine.Momentum;
using PhysicsPlayground.Engine.ConstantForces;

namespace PhysicsPlayground
{
    public enum RunState
    {
        None = 0,
        Running = 1,
        Paused = 2,
        Stopped = 3
    }

    public partial class SimulationWindow : Form
    {
        Graphics _graphics;
        Pen _scalePen;

        RunState _playState;

        Timer _clock;
        double _xMeters;
        double _yMeters = 50;
        double _pixelsPerMeter;
        double _metersInScale = 5;

        IEngine _engine;
        IEnumerable<Point> _grid;

        int _tick = 10;

        TimeSpan _baseTime = TimeSpan.Zero;
        DateTime _startTime;
        public TimeSpan RunTime
        {
            get => _playState switch
            {
                RunState.Running => _baseTime + DateTime.Now.Subtract(_startTime),
                RunState.Paused => _baseTime,
                RunState.Stopped => TimeSpan.Zero,
                _ => throw new Exception("Internal Error")
            };
        }

        public SimulationWindow()
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

            _clock = new Timer();
            _clock.Interval = _tick;
            _clock.Tick += (o, e) => OnClock();

            _scalePen = new Pen(Color.Blue, 10);

            _grid = new List<Point>();

            //_engine = new ForcesEngineFactory(new GridParams() {X = _xMeters, Y = _yMeters})
            //    .GetEngine();

            _engine = new ConservationOfMomentumEngineFactory(new GridParams() {X = _xMeters, Y = _yMeters})
                .GetEngine();

            _playState = RunState.Stopped;
            UpdateObjectsOnGrid();
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
            UpdateObjectsOnGrid();
            UpdateGraphics();
        }

        private void OnClock()
        {
            UpdateObjectsOnGrid();

            UpdateGraphics();
        }

        private void UpdateObjectsOnGrid()
        {
            double t = RunTime.TotalSeconds;

            _grid = _engine.GetCoordinates(t).Select(coordinate => {
                var (x, y) = coordinate;
                return new Point((int)(x * _pixelsPerMeter), (int)(y * _pixelsPerMeter));
                }
            );
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

            DrawObjectsGrid(e);
        }

        private void DrawScale(PaintEventArgs e)
        {
            e.Graphics.DrawLine(_scalePen,
                0, 0,
                0, (float)(_pixelsPerMeter * _metersInScale));
        }

        private void DrawObjectsGrid(PaintEventArgs e)
        {
            foreach (var point in _grid)
            {
                Point p = new Point(point.X, point.Y);
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
}
