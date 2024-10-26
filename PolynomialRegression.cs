

namespace Accord.Statistics.Models.Regression.Linear
{
    using Fitting;
    using MachineLearning;
    using Math.Optimization.Losses;
    using System;
    using System.Text;
    using Accord.Math;
    using Accord.Compat;

   
    [Serializable]
#pragma warning disable 612, 618
    public class PolynomialRegression : TransformBase<double, double>, ILinearRegression, IFormattable
#pragma warning restore 612, 618
    {
        private MultipleLinearRegression regression;


       
        public PolynomialRegression(int degree)
            : this()
        {
            regression = new MultipleLinearRegression()
            {
                NumberOfInputs = degree
            };
        }

        
        public PolynomialRegression(MultipleLinearRegression regression)
            : this()
        {
            this.regression = regression;
        }

       
        public PolynomialRegression()
        {
            NumberOfOutputs = 1;
            NumberOfInputs = 1;
        }

       
        public int Degree
        {
            get { return regression.Weights.Length; }
        }

       
        [Obsolete("Please use Weights instead.")]
        public double[] Coefficients
        {
#pragma warning disable 612, 618
            get { return regression.Weights.Concatenate(Intercept); }
#pragma warning restore 612, 618
        }

        
        public double[] Weights
        {
            get { return regression.Weights; }
            set
            {
                regression.Weights = value;
                NumberOfInputs = value.Length;
            }
        }

        
        public double Intercept
        {
            get { return regression.Intercept; }
            set { regression.Intercept = value; }
        }

        
        [Obsolete("Please use the OrdinaryLeastSquares class instead.")]
        public double Regress(double[] inputs, double[] outputs)
        {
            if (inputs.Length != outputs.Length)
                throw new ArgumentException("Number of input and output samples does not match", "outputs");

            var fit = new PolynomialLeastSquares()
            {
                Degree = Degree,
            }.Learn(inputs, outputs);
            regression.Weights.SetTo(fit.Weights);
            regression.Intercept = fit.Intercept;

            return new SquareLoss(outputs).Loss(Transform(inputs));
        }

        
        [Obsolete("Please use Transform instead.")]
        public double[] Compute(double[] input)
        {
            return Transform(input);
        }

        
        [Obsolete("Please use Transform instead.")]
        public double Compute(double input)
        {
            return Transform(input);
        }


       
        public double CoefficientOfDetermination(double[] inputs, double[] outputs, bool adjust, double[] weights = null)
        {
            var polynomial = new double[inputs.Length][];
            for (int i = 0; i < inputs.Length; i++)
            {
                polynomial[i] = new double[this.Degree];
                for (int j = 0; j < polynomial[i].Length; j++)
                    polynomial[i][j] = Math.Pow(inputs[i], this.Degree - j);
            }

            return regression.CoefficientOfDetermination(polynomial, outputs, adjust, weights);
        }


       
        public override string ToString()
        {
            return ToString(null as string);
        }

        
        public string ToString(string format)
        {
            return ToString(format, System.Globalization.CultureInfo.CurrentCulture);
        }

        
        public string ToString(IFormatProvider formatProvider)
        {
            return ToString(null, formatProvider);
        }

       
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("y(x) = ");
            for (int i = 0; i < regression.Weights.Length; i++)
            {
                int degree = regression.Weights.Length - i;
                double coeff = regression.Weights[i];

                string coefStr = format == null ?
                    coeff.ToString(formatProvider) :
                    coeff.ToString(format, formatProvider);

                sb.AppendFormat(formatProvider, "{0}x^{1}", coefStr, degree);

                if (i < regression.Weights.Length - 1)
                    sb.Append(" + ");
            }

            string interceptStr = format == null ?
                    Intercept.ToString(formatProvider) :
                    Intercept.ToString(format, formatProvider);

            sb.AppendFormat(formatProvider, " + {0}", interceptStr);

            return sb.ToString();
        }

       
        public static PolynomialRegression FromData(int degree, double[] x, double[] y)
        {
            return new PolynomialLeastSquares()
            {
                Degree = degree
            }.Learn(x, y);
        }

#pragma warning disable 612, 618
        [Obsolete("Please use Transform instead.")]
        double[] ILinearRegression.Compute(double[] inputs)
        {
            if (inputs.Length > 1)
                throw new ArgumentException("Polynomial regression supports only one-length input vectors", "inputs");
            return new double[] { this.Compute(inputs[0]) };
        }
#pragma warning restore 612, 618

       
        public override double Transform(double input)
        {
            var polynomial = new double[this.Degree];
            for (int j = 0; j < polynomial.Length; j++)
                polynomial[j] = Math.Pow(input, this.Degree - j);
            return regression.Transform(polynomial);
        }


        
        public override double[] Transform(double[] input, double[] result)
        {
            var polynomial = new double[this.Degree];
            for (int i = 0; i < input.Length; i++)
            {
                for (int j = 0; j < polynomial.Length; j++)
                    polynomial[j] = Math.Pow(input[i], this.Degree - j);

                result[i] = regression.Transform(polynomial);
            }

            return result;
        }
    }
}
