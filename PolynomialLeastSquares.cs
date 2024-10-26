

namespace Accord.Statistics.Models.Regression.Linear
{
    using MachineLearning;
    using System;
    using Math;
    using Accord.Math.Decompositions;
    using Accord.Compat;
    using System.Threading;

   
    [Serializable]
    public class PolynomialLeastSquares :
        ISupervisedLearning<PolynomialRegression, double, double>
    {
        int degree = 1;

        [NonSerialized]
        CancellationToken token = new CancellationToken();

        
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

        
        public bool IsRobust { get; set; }

       
        public int Degree
        {
            get { return degree; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "Degree should be higher than zero.");
                degree = value;
            }
        }

       
        public PolynomialRegression Learn(double[] x, double[] y, double[] weights = null)
        {
            double[][] z = Jagged.Zeros(x.Length, Degree);
            for (int i = 0; i < x.Length; i++)
                for (int j = 0; j < z[i].Length; j++)
                    z[i][j] = Math.Pow(x[i], Degree - j);

            var lls = new OrdinaryLeastSquares()
            {
                UseIntercept = true,
                IsRobust = IsRobust
            };

            var linear = lls.Learn(z, y, weights);

            return new PolynomialRegression(linear);
        }
    }
}
