namespace PhysicsPlayground.Models
{
    public class Box
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double MinX => System.Math.Min(X1, X2);
        public double MaxX => System.Math.Max(X1, X2);
        public double MinY => System.Math.Min(Y1, Y2);
        public double MaxY => System.Math.Max(Y1, Y2);
    }
}