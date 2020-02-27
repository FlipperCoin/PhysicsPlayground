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
using PhysicsPlayground.Display.DisplayAdapters;
using PhysicsPlayground.Simulation;
using PhysicsPlayground.Simulation.Simulators;
using Serilog;

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

        List<UIElement> _dynamicItems = new List<UIElement>();
        List<UIElement> _scaleLines = new List<UIElement>();

        int _tick = 10; // milliseconds
        private IRunnable _runningProgram;
        private ITimeProvider _timeProvider;
        private ShapesProvider _shapesProvider;
        private IScreenParametersProvider _screenParams;

        public double PixelsPerMeter => _screenParams.PixelsPerMeter;

        public MainWindow()
        {
            var logger = new LoggerConfiguration()
                .WriteTo.File(System.IO.Path.ChangeExtension(System.IO.Path.Join("Logs/", DateTime.Now.ToString("yyyyMMdd_HHmmss")), "txt"))
                .MinimumLevel.Debug()
                .CreateLogger();

            Log.Logger = logger;

            InitializeComponent();

            Loaded += (sender, args) => Initialize();
        }

        private async void Initialize()
        {
            double pixelsPerMeterBase;
            if (_xMeters != 0)
            {
                pixelsPerMeterBase = canvas.ActualWidth / _xMeters;
                _yMeters = canvas.ActualHeight / pixelsPerMeterBase;
            }
            else if (_yMeters != 0)
            {
                pixelsPerMeterBase = canvas.ActualHeight / _yMeters;
                _xMeters = canvas.ActualWidth / pixelsPerMeterBase;
            }
            else
                throw new Exception("Choose meters in x or y axis");

            _screenParams = new ScreenParametersProvider()
            {
                PixelsPerMeterBase = pixelsPerMeterBase,
                Zoom = zoomBar.Value,
                XCenter = canvas.ActualWidth / 2,
                YCenter = canvas.ActualHeight / 2
            };

            var runtime = new RunTime();
            _runningProgram = runtime;
            _timeProvider = runtime;
            var simulator = new ElasticCollisionSimulator(new Box() { X1 = -6, X2 = 6, Y1 = -5, Y2 = 5 },
                new List<(MassEllipse, MovementParameters2)>()
                {
                    (new MassEllipse(15, 0.5), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,10,-5,0),
                        Y=new InitialMovementParameters(0,5,0,0)
                    }),
                    (new MassEllipse(25, 0.7), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,-3,2,0),
                        Y=new InitialMovementParameters(0,-1,2,0)
                    }),
                    (new MassEllipse(60, 0.8), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,2,-2,0),
                        Y=new InitialMovementParameters(0,-4,1,0)
                    }),
                    (new MassEllipse(30, 0.6), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,2,-3,0),
                        Y=new InitialMovementParameters(0,2,-1,0)
                    }),
                    (new MassEllipse(10, 0.3), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,-12,4,0),
                        Y=new InitialMovementParameters(0,4,-4,0)
                    }),
                    (new MassEllipse(80, 0.6), new MovementParameters2()
                    {
                        X=new InitialMovementParameters(0,-10,5,0),
                        Y=new InitialMovementParameters(0,5,0,0)
                    })
                });

            startBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;
            loadingLabel.Content = "Generating Simulation...";
            var simulation = await simulator.GenerateSimulationAsync(0, 20);
            startBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;
            loadingLabel.Content = "Simulation Ready";

            _shapesProvider = ShapesProvider.CreateInstance(
                new SimulationRunner<IEnumerable<(MassEllipse, (double, double))>>(simulation, runtime),
                new MassEllipseDisplayAdapter(_screenParams)
                );
            _clock = new DispatcherTimer();
            _clock.Interval = TimeSpan.FromMilliseconds(_tick);
            _clock.Tick += (o, e) => OnClock();
            _clock.Start();
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
            startBtn.Content = "Resume";
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
            startBtn.Content = "Start";

            UpdateGraphics();
        }

        private void OnClock()
        {
            UpdateGraphics();
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
            x.Y1 = _screenParams.YCenter;
            x.Y2 = _screenParams.YCenter;

            _scaleLines.Add(x);
            canvas.Children.Add(x);

            Line y = new Line();
            y.Stroke = new SolidColorBrush(Colors.DarkGray);
            y.Opacity = 0.6;
            y.StrokeThickness = 1;

            y.X1 = _screenParams.XCenter;
            y.X2 = _screenParams.XCenter;
            y.Y1 = 0;
            y.Y2 = canvas.ActualHeight;

            _scaleLines.Add(y);
            canvas.Children.Add(y);

            for (double i = _screenParams.YCenter; i < canvas.ActualHeight; i += 2 * PixelsPerMeter)
            {
                var line = NewHorizontalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }
            for (double i = _screenParams.YCenter; i > 0; i -= 2 * PixelsPerMeter)
            {
                var line = NewHorizontalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }

            for (double i = _screenParams.XCenter; i < canvas.ActualWidth; i += 2 * PixelsPerMeter)
            {
                var line = NewVerticalLine(i);

                _scaleLines.Add(line);
                canvas.Children.Add(line);
            }
            for (double i = _screenParams.XCenter; i > 0; i -= 2 * PixelsPerMeter)
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

            foreach (var shape in _shapesProvider.Shapes)
            {
                _dynamicItems.Add(shape);
                canvas.Children.Add(shape);
            }
        }

        private void SpeedTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateSpeed();
                speedTextBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
            }
        }

        private void SpeedTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSpeed();
        }

        private void UpdateSpeed()
        {
            _timeProvider.Speed = double.Parse(speedTextBox.Text);
        }

        private void ZoomBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_screenParams != null) _screenParams.Zoom = e.NewValue;
        }
    }
}
