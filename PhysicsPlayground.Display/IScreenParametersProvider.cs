namespace PhysicsPlayground.Display
{
    internal interface IScreenParametersProvider
    {
        double PixelsPerMeterBase { get; set; }
        public double Zoom { get; set; }
        public double PixelsPerMeter { get; }
        public double XCenter { get; set; }
        public double YCenter { get; set; }
    }
}