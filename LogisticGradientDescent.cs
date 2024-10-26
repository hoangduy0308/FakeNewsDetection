

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.MachineLearning;
    using System.Threading;
    using Accord.Compat;

    

    public class LogisticGradientDescent :
        ISupervisedLearning<LogisticRegression, double[], int>,
        ISupervisedLearning<LogisticRegression, double[], bool>,
        ISupervisedLearning<LogisticRegression, double[], double>,
        IRegressionFitting, IConvergenceLearning
#pragma warning restore 612, 618
    {
        [NonSerialized]
        CancellationToken token = new CancellationToken();

        private LogisticRegression regression;

        private int parameterCount;
        private bool stochastic = false;
        private RelativeParameterConvergence convergence;

        private double rate = 0.1;

        private double[] gradient;
        private double[] previous;
        private double[] deltas;

       
        public double[] Previous { get { return previous; } }

        
        public double[] Solution
        {
            get { return regression.Intercept.Concatenate(regression.Weights); }
        }

      
        public double[] Gradient { get { return gradient; } }

       
        public int Parameters { get { return parameterCount; } }

       
        public bool Stochastic
        {
            get { return stochastic; }
            set { stochastic = value; }
        }

        
        public double LearningRate
        {
            get { return rate; }
            set { rate = value; }
        }

        
        [Obsolete("Please use MaxIterations instead.")]
        public int Iterations
        {
            get { return convergence.MaxIterations; }
            set { convergence.MaxIterations = value; }
        }

        
        public int MaxIterations
        {
            get { return convergence.MaxIterations; }
            set { convergence.MaxIterations = value; }
        }

      
        public double Tolerance
        {
            get { return convergence.Tolerance; }
            set { convergence.Tolerance = value; }
        }

       
        public int CurrentIteration
        {
            get { return convergence.CurrentIteration; }
        }

       
        public bool HasConverged
        {
            get { return convergence.HasConverged; }
        }

       
        public LogisticGradientDescent()
        {
            convergence = new RelativeParameterConvergence()
            {
                MaxIterations = 0,
                Tolerance = 1e-8
            };
        }

       
        public LogisticGradientDescent(LogisticRegression regression)
            : this()
        {
            init(regression);
        }

        private void init(LogisticRegression regression)
        {
            this.regression = regression;

            this.parameterCount = regression.NumberOfParameters;

            this.gradient = new double[parameterCount];
            this.deltas = new double[parameterCount];
        }

        
        [Obsolete("Please use the Learn(x, y) method instead.")]
        public double Run(double[][] inputs, double[][] outputs)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");

            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0];

            return Run(inputs, output);
        }

       
        [Obsolete("Please use the Learn(x, y) method instead.")]
        public double Run(double[] input, double output)
        {
            int old = convergence.Iterations;
            convergence.Iterations = 1;
            Learn(new[] { input }, new[] { output });
            convergence.Iterations = old;
            return Matrix.Max(deltas);
        }

       
        [Obsolete("Please use the Learn(x, y) method instead.")]
        public double Run(double[][] inputs, double[] outputs)
        {
            int old = convergence.Iterations;
            convergence.Iterations = 1;
            Learn(inputs, outputs);
            convergence.Iterations = old;
            return Matrix.Max(deltas);
        }

        
        [Obsolete("Please use the LogLikelihoodLoss class instead.")]
        public double ComputeError(double[][] inputs, double[] outputs)
        {
            double sum = 0;

            for (int i = 0; i < inputs.Length; i++)
            {
#pragma warning disable 612, 618
                double actual = regression.Compute(inputs[i]);
#pragma warning restore 612, 618
                double expected = outputs[i];
                double delta = actual - expected;
                sum += delta * delta;
            }

            return sum;
        }


       
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

      
        public LogisticRegression Learn(double[][] x, int[] y, double[] weights = null)
        {
            return Learn(x, y.ToDouble(), weights);
        }

        
        public LogisticRegression Learn(double[][] x, bool[] y, double[] weights = null)
        {
            return Learn(x, y.ToDouble(), weights);
        }

        
        public LogisticRegression Learn(double[][] x, double[] y, double[] weights = null)
        {
            if (weights != null)
              // throw new ArgumentException(Accord.Properties.Resources.NotSupportedWeights, "weights");
           
            if (regression == null)
            {
                init(new LogisticRegression()
                {
                    NumberOfInputs = x.Columns()
                });
            }

            // Initial definitions and memory allocations
            double[] previous = (double[])Solution.Clone();
            convergence.Clear();

            do
            {
                if (Token.IsCancellationRequested)
                    break;

                if (stochastic)
                {
                    for (int j = 0; j < x.Length; j++)
                    {
                        // 1. Compute local gradient estimate
                        double z = regression.Linear.Transform(x[j]);
                        double actual = regression.Link.Inverse(z);
                        double error = y[j] - actual;

                        gradient[0] = error;
                        for (int i = 0; i < x[j].Length; i++)
                            gradient[i + 1] = x[j][i] * error;

                        // 2. Update using the local estimate
                        for (int i = 0; i < regression.Weights.Length; i++)
                            regression.Weights[i] += rate * gradient[i + 1];
                        regression.Intercept += rate * gradient[0];
                    }
                }
                else
                {
                    // Compute the complete error gradient
                    Array.Clear(gradient, 0, gradient.Length);

                    for (int i = 0; i < x.Length; i++)
                    {
                        // 1. Compute local gradient estimate
                        double z = regression.Linear.Transform(x[i]);
                        double actual = regression.Link.Inverse(z);
                        double error = y[i] - actual;

                        gradient[0] += error;
                        for (int j = 0; j < x[i].Length; j++)
                            gradient[j + 1] += x[i][j] * error;
                    }

                    // Update coefficients using the gradient
                    for (int i = 0; i < regression.Weights.Length; i++)
                        regression.Weights[i] += rate * gradient[i + 1] / x.Length;
                    regression.Intercept += rate * gradient[0] / x.Length;
                }

                // Return the maximum parameter change
                this.previous = previous;
                for (int i = 0; i < previous.Length; i++)
                    deltas[i] = Math.Abs(regression.GetCoefficient(i) - previous[i]) / Math.Abs(previous[i]);

                convergence.NewValues = this.Solution;

                if (Token.IsCancellationRequested)
                    return regression;

            } while (!convergence.HasConverged);

            return regression;
        }
    }
}
