using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PhysicsPlayground.Simulation;

namespace PhysicsPlayground.Display
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer _clock;
        double _xMeters;
        double _yMeters = 20;
        double _pixelsPerMeter;
        double _metersInScale = 5;

        IObjectsStateProvider _objectsStateProvider;
        IEnumerable<Point> _grid;

        int _tick = 10; // milliseconds
        private IRunnable _runningProgram;
        private ITimeProvider _timeProvider;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) => Initialize();
        }

        private void Initialize()
        {
            if (_xMeters != 0)
            {
                _pixelsPerMeter = canvas.ActualWidth / _xMeters;
                _yMeters = canvas.ActualHeight / _pixelsPerMeter;
            }
            else if (_yMeters != 0)
            {
                _pixelsPerMeter = canvas.ActualHeight / _yMeters;
                _xMeters = canvas.ActualWidth / _pixelsPerMeter;
            }
            else
                throw new Exception("Choose meters in x or y axis");

            _clock = new DispatcherTimer();
            _clock.Interval = TimeSpan.FromMilliseconds(_tick);
            _clock.Tick += (o, e) => OnClock();
            _clock.Start();

            _grid = new List<Point>();

            //_engine = new ForcesEngineFactory(new GridParams() {X = _xMeters, Y = _yMeters})
            //    .GetEngine();

            var runtime = new RunTime();
            _runningProgram = runtime;
            _timeProvider = runtime;
            var simulator = new ConservationOfMomentumSimulator(new GridParams() { X = _xMeters, Y = _yMeters },
                new List<(MassObject, MovementParameters2)>()
                {
                    (new MassObject(100), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,15,5,0),
                        Y=new InitialMovementParameters(0,15,5,0)
                    })
                });
            _objectsStateProvider = new SimulationRunner(simulator.GenerateSimulation(0, 60), runtime);

            UpdateObjectsOnGrid();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Action action = _runningProgram.RunState switch
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
            _runningProgram.Pause();
            startBtn.Content= "Resume";
        }

        private void Start()
        {
            _runningProgram.Start();
            startBtn.Content = "Pause";
        }

        private void Resume()
        {
            _runningProgram.Resume();
            startBtn.Content = "Pause";
        }

        private void Stop()
        {
            _runningProgram.Stop();
            startBtn.Content= "Start";

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
            _grid = _objectsStateProvider.GetCoordinates().Select(coordinate => {
                    var (x, y) = coordinate;
                    return new Point((int)(x * _pixelsPerMeter), (int)(y * _pixelsPerMeter));
                }
            );
        }

        private void UpdateGraphics()
        {
            timerLabel.Content = _timeProvider.Time.ToString(@"hh\:mm\:ss\:ff");
            
            DrawScale();
            DrawObjectsGrid();
        }

        private void DrawScale()
        {
            
        }

        private void DrawObjectsGrid()
        {
            canvas.Children.Clear();

            foreach (var point in _grid.Select(p => new Point(p.X - 10, p.Y - 10)))
            {
                Ellipse ball = new Ellipse();
                ball.Height = 20;
                ball.Width = 20;
                ball.Fill = new SolidColorBrush(Colors.Black);

                Canvas.SetLeft(ball, point.X);
                Canvas.SetTop(ball, point.Y);
                canvas.Children.Add(ball);
            }
        }
    }
}
