using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Financial;

namespace PhysicsPlayground.Math
{
    public class Polynomial
    {
        public double[] Coefficients { get; private set; }

        public Polynomial(params double[] coefficients)
        {
            Coefficients = coefficients.Reverse().SkipWhile(co => co == 0).Reverse().ToArray();
        }

        public Polynomial Offset(double offset)
        {
            Polynomial sumPol = new Polynomial();
            for (var i = 0; i < Coefficients.Length; i++)
            {
                sumPol += Coefficients[i] * ((new Polynomial(offset, 1)) ^ i);
            }

            return sumPol;
        }

        public Polynomial AntiDerivative(double arbitraryConstant = 0)
        {
            var newCoefficients = new double[Coefficients.Length + 1];
            newCoefficients[0] = arbitraryConstant;
            for (var i = 1; i < newCoefficients.Length; i++)
            {
                newCoefficients[i] = Coefficients[i - 1] * (1.0 / i);
            }

            return new Polynomial(newCoefficients);
        }

        public static Polynomial operator *(double c, Polynomial p2)
        {
            return new Polynomial(p2.Coefficients.Select(a => a * c).ToArray());
        }

        public static Polynomial operator *(Polynomial p1, double c) => c * p1;

        public static Polynomial operator *(Polynomial p1, Polynomial p2)
        {
            Polynomial sumPol = new Polynomial();
            for (var i = 0; i < p1.Coefficients.Length; i++)
            {
                var multipliedPolCos = new double[i + p2.Coefficients.Length];
                for (var j = 0; j < p2.Coefficients.Length; j++)
                {
                    multipliedPolCos[i + j] = p1.Coefficients[i] * p2.Coefficients[j];
                }

                sumPol += new Polynomial(multipliedPolCos);
            }

            return sumPol;
        }

        public static Polynomial operator ^(Polynomial p1, double pow)
        {
            Polynomial mulPol = new Polynomial(1);
            for (int i = 0; i < pow; i++)
            {
                mulPol *= p1;
            }

            return mulPol;
        }

        public static Polynomial operator +(Polynomial p1, Polynomial p2)
        {
            var zipped = p1.Coefficients.Zip(p2.Coefficients, (first,second) => first+second).ToList();
            
            var (higherDim, lowerDim) = 
                p1.Coefficients.Length > p2.Coefficients.Length ? (p1, p2) : (p2, p1);
            zipped.AddRange(higherDim.Coefficients.Skip(lowerDim.Coefficients.Length));
            
            return new Polynomial(zipped.ToArray());
        }

        public static Polynomial operator +(Polynomial p1, double x)
        {
            if (x == 0) return p1.Clone();
            var newCo = p1.Coefficients.Length > 0 ? (double[])p1.Coefficients.Clone() : new double[1];
            newCo[0] += x;

            return new Polynomial(newCo);
        }

        public Polynomial Clone()
        {
            return new Polynomial(Coefficients);
        }

        public static Polynomial operator +(double x, Polynomial p1) => p1 + x;
        public static Polynomial operator -(Polynomial p1, double x) => p1 + (-x);

        public double Evaluate(double x)
        {
            double sum = 0;
            for (var i = 0; i < Coefficients.Length; i++)
            {
                sum += Coefficients[i] * System.Math.Pow(x, i);
            }

            return sum;
        }

        public Polynomial Derivative()
        {
            if (Coefficients.Length == 0) return new Polynomial();

            var newCo = new double[Coefficients.Length - 1];
            for (var i = 1; i < Coefficients.Length; i++)
            {
                newCo[i - 1] = Coefficients[i] * i;
            }

            return new Polynomial(newCo);
        }

        public double[] Roots(double line = 0) => FindRoots.Polynomial((this - line).Coefficients).Where(c => c.IsReal()).Select(c => c.Real)
            .ToArray();

        public override string ToString()
        {
            return string.Join(" + ", Coefficients.Select((co, idx) => $"{co}x^{idx}"));
        }
    }
}