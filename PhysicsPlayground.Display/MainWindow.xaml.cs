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
using PhysicsPlayground.Simulation.Simulators;

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
        double _pixelsPerMeterBase;
        private double _y0OnCanvas;
        private double _x0OnCanvas;

        IObjectsStateProvider _objectsStateProvider;

        List<UIElement> _dynamicItems = new List<UIElement>();
        List<UIElement> _scaleLines = new List<UIElement>();
        IEnumerable<Point> _grid;

        int _tick = 10; // milliseconds
        private IRunnable _runningProgram;
        private ITimeProvider _timeProvider;

        public double PixelsPerMeter => _pixelsPerMeterBase * zoomBar.Value;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) => Initialize();
        }

        private async void Initialize()
        {
            if (_xMeters != 0)
            {
                _pixelsPerMeterBase = canvas.ActualWidth / _xMeters;
                _yMeters = canvas.ActualHeight / _pixelsPerMeterBase;
            }
            else if (_yMeters != 0)
            {
                _pixelsPerMeterBase = canvas.ActualHeight / _yMeters;
                _xMeters = canvas.ActualWidth / _pixelsPerMeterBase;
            }
            else
                throw new Exception("Choose meters in x or y axis");

            _x0OnCanvas = canvas.ActualWidth / 2;
            _y0OnCanvas = canvas.ActualHeight / 2;

            _grid = new List<Point>();

            var runtime = new RunTime();
            _runningProgram = runtime;
            _timeProvider = runtime;
            var simulator = new ElasticCollisionSimulator( 
                new List<(MassObject, MovementParameters2)>()
                {
                    (new MassObject(100), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,5,-5,0),
                        Y=new InitialMovementParameters(0,0,0,0)
                    }),
                    (new MassObject(100), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,-5,5,0),
                        Y=new InitialMovementParameters(0,0,0,0)
                    })
                });

            startBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
            loadingLabel.Content = "Generating Simulation...";
            ISimulation simulation = await simulator.GenerateSimulationAsync(0, 60);
            startBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;
            loadingLabel.Content = "Simulation Ready";
            _objectsStateProvider = new SimulationRunner(simulation, runtime);
            _clock = new DispatcherTimer();
            _clock.Interval = TimeSpan.FromMilliseconds(_tick);
            _clock.Tick += (o, e) => OnClock();
            _clock.Start();

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
                    return new Point((int)(_x0OnCanvas + x * PixelsPerMeter), (int)(canvas.ActualHeight - (_y0OnCanvas + y * PixelsPerMeter)));
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
            _scaleLines.ForEach(element => canvas.Children.Remove(element));
            _scaleLines.Clear();

            Line x = new Line();
            x.Stroke = new SolidColorBrush(Colors.DarkGray);
            x.Opacity = 0.6;
            x.StrokeThickness = 1;
            
            x.X1 = 0;
            x.X2 = canvas.ActualWidth;
            x.Y1 = _y0OnCanvas;
            x.Y2 = _y0OnCanvas;

            _scaleLines.Add(x);
            canvas.Children.Add(x);

            Line y = new Line();
            y.Stroke = new SolidColorBrush(Colors.DarkGray);
            y.Opacity = 0.6;
            y.StrokeThickness = 1;
            
            y.X1 = _x0OnCanvas;
            y.X2 = _x0OnCanvas;
            y.Y1 = 0;
            y.Y2 = canvas.ActualHeight;

            _scaleLines.Add(y);
            canvas.Children.Add(y);

            for (double i = _y0OnCanvas; i < canvas.ActualHeight; i += 2 * PixelsPerMeter)
            {
                var line = NewHorizontalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }
            for (double i = _y0OnCanvas; i > 0; i -= 2 * PixelsPerMeter)
            {
                var line = NewHorizontalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }

            for (double i = _x0OnCanvas; i < canvas.ActualWidth; i += 2 * PixelsPerMeter)
            {
                var line = NewVerticalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }
            for (double i = _x0OnCanvas; i > 0; i -= 2 * PixelsPerMeter)
            {
                var line = NewVerticalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }
        }

        private Line NewVerticalLine(double x)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.DarkGray);
            line.Opacity = 0.3;
            line.StrokeThickness = 1;

            line.X1 = x;
            line.X2 = x;
            line.Y1 = 0;
            line.Y2 = canvas.ActualHeight;
            return line;
        }

        private Line NewHorizontalLine(double y)
        {
            Line line = new Line();
            line.Stroke = new SolidColorBrush(Colors.DarkGray);
            line.Opacity = 0.3;
            line.StrokeThickness = 1;

            line.X1 = 0;
            line.X2 = canvas.ActualWidth;
            line.Y1 = y;
            line.Y2 = y;
            return line;
        }

        private void DrawObjectsGrid()
        {
            _dynamicItems.ForEach(element => canvas.Children.Remove(element));
            _dynamicItems.Clear();

            var radius = 0.5D;
            foreach (var point in _grid.Select(p => new Point(p.X - PixelsPerMeter * radius, p.Y - PixelsPerMeter * radius)))
            {
                Ellipse ball = new Ellipse();
                ball.Height = 2 * (radius * PixelsPerMeter);
                ball.Width = 2 * (radius * PixelsPerMeter);
                ball.Fill = new SolidColorBrush(Colors.Black);

                Canvas.SetLeft(ball, point.X);
                Canvas.SetTop(ball, point.Y);
                _dynamicItems.Add(ball);
                canvas.Children.Add(ball);
            }
        }
    }
}
