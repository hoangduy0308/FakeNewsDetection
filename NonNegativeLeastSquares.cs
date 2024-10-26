

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using System.Collections.Generic;
    using Accord.Math;
    using Accord.Statistics.Models.Regression.Linear;
    using Accord.MachineLearning;
    using Accord.Math.Optimization.Losses;
    using Accord.Compat;
    using System.Threading;

    
#pragma warning disable 612, 618
    public class NonNegativeLeastSquares : IRegressionFitting,
        ISupervisedLearning<MultipleLinearRegression, double[], double>
#pragma warning restore 612, 618
    {
        [NonSerialized]
        CancellationToken token = new CancellationToken();

        MultipleLinearRegression regression;

        List<int> p = new List<int>();
        List<int> r = new List<int>();
        double[] s;
        double tolerance = 0.001;
        double[][] scatter;
        double[] vector;
        double[] W;
        double[][] X;
        int cols;
        int maxIter;

        
        public double[] Coefficients { get { return regression.Weights; } }

        
        public int MaxIterations
        {
            get { return maxIter; }
            set { maxIter = value; }
        }

        
        public double Tolerance
        {
            get { return tolerance; }
            set { tolerance = value; }
        }

       
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

       
        public NonNegativeLeastSquares()
        {
        }

        
        public NonNegativeLeastSquares(MultipleLinearRegression regression)
        {
            init(regression);
        }

        private void init(MultipleLinearRegression regression)
        {
            this.regression = regression;
            this.cols = regression.Weights.Length;
            this.s = new double[cols];
            this.W = new double[cols];
        }


       
        [Obsolete("Please use the Learn() method instead.")]
        public double Run(double[][] inputs, double[] outputs)
        {
            this.regression = Learn(inputs, outputs);
            return new SquareLoss(inputs).Loss(regression.Transform(inputs));
        }

       
        public MultipleLinearRegression Learn(double[][] x, double[] y, double[] weights = null)
        {
            if (weights != null)
               // throw new ArgumentException(Accord.Properties.Resources.NotSupportedWeights, "weights");

            if (this.regression == null)
                init(new MultipleLinearRegression { NumberOfInputs = x.Columns() });

            this.X = x;
            this.scatter = X.TransposeAndDot(X);
            this.vector = X.TransposeAndDot(y);

            // Initialization
            p.Clear();
            r.Clear();
            for (var i = 0; i < cols; i++)
                r.Add(i);

            var w = Coefficients;

            ComputeWeights(w);
            var iter = 0;
            int maxWeightIndex;
            W.Max(out maxWeightIndex);

            while (r.Count > 0 && W[maxWeightIndex] > tolerance && iter < maxIter)
            {
                if (Token.IsCancellationRequested)
                    break;

                // Include the index j in P and remove it from R
                if (!p.Contains(maxWeightIndex))
                    p.Add(maxWeightIndex);

                if (r.Contains(maxWeightIndex))
                    r.Remove(maxWeightIndex);

                GetSP();
                int iter2 = 0;

                while (GetElements(s, p).Min() <= 0 && iter2 < maxIter)
                {
                    InnerLoop(w);
                    iter2++;
                }

                // 5
                Array.Copy(s, w, s.Length);

                // 6
                ComputeWeights(w);

                W.Max(out maxWeightIndex);
                iter++;
            }

            //Coefficients = x;
            return regression;
        }

        private void InnerLoop(double[] x)
        {
            var alpha = double.PositiveInfinity;
            foreach (int i in p)
            {
                if (s[i] <= 0)
                    alpha = System.Math.Min(alpha, x[i] / (x[i] - s[i]));
            }

            if (System.Math.Abs(alpha) < 0.001 || double.IsNaN(alpha))
                return;

            x = (s.Subtract(x)).Multiply(alpha).Add(x);

            // 4.4 Update R and P
            for (var i = 0; i < p.Count;)
            {
                int pItem = p[i];
                if (System.Math.Abs(x[pItem]) < double.Epsilon)
                {
                    r.Add(pItem);
                    p.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            // 4.5 
            GetSP();

            // 4.6
            foreach (var i in r)
                s[i] = 0;
        }

        private void ComputeWeights(double[] x)
        {
            W = vector.Subtract(scatter.Dot(x));
        }

        private void GetSP()
        {
            int[] array = p.ToArray();
            double[][] left = scatter
                .GetColumns(array)
                .GetRows(array)
                .PseudoInverse();

            double[] columnVector = GetElements(vector, p);
            double[] result = left.Dot(columnVector);
            for (int i = 0; i < p.Count; i++)
                s[p[i]] = result[i];
        }

        private static double[] GetElements(double[] vector, List<int> elementsIndex)
        {
            var z = new double[elementsIndex.Count];
            for (var i = 0; i < elementsIndex.Count; i++)
                z[i] = vector[elementsIndex[i]];
            return z;
        }

    }
}
