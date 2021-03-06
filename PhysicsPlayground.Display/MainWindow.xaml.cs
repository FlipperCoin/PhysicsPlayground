﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using PhysicsPlayground.Display.DisplayAdapters;
using PhysicsPlayground.Math;
using PhysicsPlayground.Models;
using PhysicsPlayground.Runtime;
using PhysicsPlayground.Simulation;
using PhysicsPlayground.Simulation.Implementations;
using PhysicsPlayground.Simulation.Simulator.Implementations;
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
        private MetadataProvider _metadataProvider;
        private IScreenParametersProvider _screenParams;

        private List<(string name, Func<Task<(ShapesProvider shapesProvider, MetadataProvider metadataProvider)>> simulator)>
            _simulators;
            
        
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

        private void Initialize()
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
                YCenter = canvas.ActualHeight / 2,
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight
            };

            var runtime = new RunTime();
            _runningProgram = runtime;
            _timeProvider = runtime;

            _simulators =
                new List<(string name, Func<Task<(ShapesProvider shapesProvider, MetadataProvider metadataProvider)>>)>
                {
                    ("Torque From Friction", async () =>
                    {
                        var simulator = new FrictionAndTorqueSimulator(
                            new SpecificMassEllipse(new Polynomial(1), 1), 
                            0,
                            20,
                            1);

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<AngularMomentumSimulationMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new AngularMomentumDisplayAdapter(_screenParams, true, true)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<AngularMomentumSimulationMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Torque From Friction 2", async () =>
                    {
                        var simulator = new FrictionAndTorqueSimulator(
                            new SpecificMassEllipse(new Polynomial(1), 1), 
                            13,
                            0,
                            1);

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<AngularMomentumSimulationMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new AngularMomentumDisplayAdapter(_screenParams, true, true)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<AngularMomentumSimulationMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Torque From Friction 3", async () =>
                    {
                        var simulator = new FrictionAndTorqueSimulator(
                            new SpecificMassEllipse(new Polynomial(1), 1), 
                            12,
                            8,
                            1);

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<AngularMomentumSimulationMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new AngularMomentumDisplayAdapter(_screenParams, true, true)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<AngularMomentumSimulationMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Torque", async () =>
                    {
                        var simulator = new TorqueSimulator(new SpecificMassEllipse(new Polynomial(1), 3), 0, -4);

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<AngularMomentumSimulationMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new AngularMomentumDisplayAdapter(_screenParams, false, false)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<AngularMomentumSimulationMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Elastic Collision", async () =>
                    {
                        var simulator = new ElasticCollisionSimulator(new Box() { X1 = -6, X2 = 6, Y1 = -2, Y2 = 2 },
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
                                    Y=new InitialMovementParameters(0,-1,1.2,0)
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
                                    X=new InitialMovementParameters(0,-12,1.5,0),
                                    Y=new InitialMovementParameters(0,4,-1.5,0)
                                }),
                                (new MassEllipse(80, 0.6), new MovementParameters2()
                                {
                                    X=new InitialMovementParameters(0,-10,5,0),
                                    Y=new InitialMovementParameters(0,5,0,0)
                                })
                            });

                        var simulation = await simulator.GenerateSimulationAsync(0, 60);

                        var simulationRunner =
                            new SimulationRunner<ElasticCollisionMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new ElasticCollisionDisplayAdapter(_screenParams)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<ElasticCollisionMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Elastic Collision 2", async () =>
                    {
                        var simulator = new ElasticCollisionSimulator(new Box() { X1 = -6, X2 = 6, Y1 = -2, Y2 = 2 },
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
                                    Y=new InitialMovementParameters(0,-1,1.2,0)
                                }),
                                (new MassEllipse(30, 0.6), new MovementParameters2()
                                {
                                    X=new InitialMovementParameters(0,2,-1,0),
                                    Y=new InitialMovementParameters(0,2,-1,0)
                                }),
                                (new MassEllipse(10, 0.3), new MovementParameters2()
                                {
                                    X=new InitialMovementParameters(0,-12,2,0),
                                    Y=new InitialMovementParameters(0,4,-1.5,0)
                                })
                            });

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<ElasticCollisionMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new ElasticCollisionDisplayAdapter(_screenParams)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<ElasticCollisionMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Elastic Collision Pi", async () =>
                    {
                        var simulator = new ElasticCollisionSimulator(new Box() { X1 = -6, X2 = 6, Y1 = -2, Y2 = 2 },
                            new List<(MassEllipse, MovementParameters2)>()
                            {
                                (new MassEllipse(1, 0.5), new MovementParameters2()
                                {
                                    X=new InitialMovementParameters(0,0,-2,0),
                                    Y=new InitialMovementParameters(0,0,0,0)
                                }),
                                (new MassEllipse(1000, 0.5), new MovementParameters2()
                                {
                                    X=new InitialMovementParameters(0,-2,2,0),
                                    Y=new InitialMovementParameters(0,0,0,0)
                                })
                            });

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<ElasticCollisionMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new ElasticCollisionDisplayAdapter(_screenParams)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<ElasticCollisionMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    }),
                    ("Rocket", async () =>
                    {
                        var simulator = new RocketSimulator(10, 90, 1, 100, 0);

                        var simulation = await simulator.GenerateSimulationAsync(0, 200);

                        var simulationRunner =
                            new SimulationRunner<RocketSimulationMoment>(simulation, runtime);

                        var shapesProvider = ShapesProvider.CreateInstance(
                            simulationRunner,
                            new RocketDisplayAdapter(_screenParams)
                        );
                        var metadataProvider = MetadataProvider.CreateInstance(
                            simulationRunner,
                            new JsonSerializerMetadataAdapter<RocketSimulationMoment>()
                        );

                        return (shapesProvider, metadataProvider);
                    })
                };

            simulatorList.ItemsSource = _simulators.Select(s => s.name);
            startBtn.IsEnabled = false;
            stopBtn.IsEnabled = false;

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

            if (_metadataProvider != null) metadataText.Text = _metadataProvider.Metadata;

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

            if (_shapesProvider == null) return;

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

        private async void SimulatorList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stop();

            loadingLabel.Content = "Generating Simulation...";

            var (name, simulator )= _simulators[simulatorList.SelectedIndex];
            (_shapesProvider, _metadataProvider) = await simulator();

            startBtn.IsEnabled = true;
            stopBtn.IsEnabled = true;
            loadingLabel.Content = "Simulation Ready";
        }

        private void Canvas_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_screenParams != null)
            {
                _screenParams.Height = canvas.ActualHeight;
                _screenParams.Width = canvas.ActualWidth;
            }
        }
    }
}
