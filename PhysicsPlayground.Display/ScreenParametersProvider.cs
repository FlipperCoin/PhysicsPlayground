namespace PhysicsPlayground.Display
{
    internal class ScreenParametersProvider : IScreenParametersProvider
    {
        public double PixelsPerMeterBase { get; set; }
        public double Zoom { get; set; }
        public double PixelsPerMeter => PixelsPerMeterBase * Zoom;
        public double XCenter { get; set; }
        public double YCenter { get; set; }
    }
}