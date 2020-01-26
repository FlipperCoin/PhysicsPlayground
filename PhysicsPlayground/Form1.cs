using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;

namespace PhysicsPlayground
{
    public class MovementEquationConstants
    {
        public float Ax0 { get; set; }
        public float Ay0 { get; set; }
        public float Vx0 { get; set; }
        public float Vy0 { get; set; }
        public double X0 { get; set; }
        public double Y0 { get; set; }
    }

    public class MassObject
    {
        public Guid Guid { get; internal set; }
        public float Mass { get; set; }
        public IList<Force> Forces { get; set; }
        public MovementEquationConstants InitValues { get; set; }

        public MassObject(int mass, IList<Force> forces, MovementEquationConstants initValues)
        {
            Guid = Guid.NewGuid();

            Mass = mass;
            Forces = forces;
            InitValues = initValues;
        }

        public (double x, double y) GetLocationInTime(double t)
        {
            var aVector = Forces
                .Select(force => Vector2.Divide(force.Vector, Mass))
                .Aggregate((force1Vector, force2Vector) => Vector2.Add(force1Vector, force2Vector));
            (float ax, float ay) = (aVector.X, aVector.Y);

            double x = InitValues.X0 + 0.5 * (InitValues.Ax0 + aVector.X) * Math.Pow(t, 2) + InitValues.Vx0 * t;
            double y = InitValues.Y0 + 0.5 * (InitValues.Ay0 + aVector.Y) * Math.Pow(t, 2) + InitValues.Vy0 * t;

            return (x, y);
        }
    }

    public class Force
    {
        public Vector2 Vector { get; set; }
    }

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

        IList<MassObject> _massObjects;
        IDictionary<Guid,Point> _grid;

        int _tick = 10;
        float _factor;

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

            _factor = (float)(_tick / TimeSpan.FromSeconds(1).TotalMilliseconds) * _pixelsPerMeter;

            _clock = new Timer();
            _clock.Interval = _tick;
            _clock.Tick += (o, e) => OnClock();

            _scalePen = new Pen(Color.Blue, 10);

            _massObjects = new List<MassObject>
            {
                new MassObject(
                    10, 
                    new List<Force> { new Force {Vector = new Vector2(0, 98)} }, 
                    new MovementEquationConstants { X0 = 0, Y0 = 2 * _yMeters / 3, Ax0 = 0, Ay0 = 0, Vx0 = 10, Vy0 = -10})
            };

            _grid = new Dictionary<Guid, Point>();

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

            foreach (var massObj in _massObjects)
            {
                (double x, double y) = massObj.GetLocationInTime(t);
                _grid[massObj.Guid] = new Point((int)(x * _pixelsPerMeter), (int)(y * _pixelsPerMeter));
            }
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
                0, _pixelsPerMeter * _metersInScale);
        }

        private void DrawObjectsGrid(PaintEventArgs e)
        {
            foreach (var point in _grid.Values)
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
