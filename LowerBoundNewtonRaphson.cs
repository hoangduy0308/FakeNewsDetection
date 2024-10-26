

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.MachineLearning;
    using Accord.Compat;
    using System.Threading;

    
    
#pragma warning disable 612, 618
    public class LowerBoundNewtonRaphson :
        ISupervisedLearning<LogisticRegression1, double[], int>,
        ISupervisedLearning<LogisticRegression1, double[], int[]>,
        ISupervisedLearning<LogisticRegression1, double[], double[]>,
        IMultipleRegressionFitting, IConvergenceLearning
#pragma warning restore 612, 618
    {
        [NonSerialized]
        CancellationToken token = new CancellationToken();

        private LogisticRegression1 regression;

        private double[] previous;
        private double[] solution;
        private double[] deltas;

        private double[] errors;
        private double[] output;

        private double[,] weights;

        private double[,] lowerBound;
        private double[] gradient;
        private double[,] xxt;

        private int K;
        private int M;
        private int parameterCount;

        private bool computeStandardErrors = true;
        private bool updateLowerBound = true;
        private ISolverMatrixDecomposition<double> decomposition = null;

        private IConvergence<double> convergence;

      
        public double[] Previous
        {
            get { return previous; }
        }

        
        public double[] Solution
        {
            get { return solution; }
        }

        
        public bool UpdateLowerBound
        {
            get { return updateLowerBound; }
            set { updateLowerBound = value; }
        }

       
        public double[,] HessianLowerBound
        {
            get { return lowerBound; }
        }

        
        public double[] Gradient
        {
            get { return gradient; }
        }

       
        public int Parameters { get { return parameterCount; } }


       
        public bool ComputeStandardErrors
        {
            get { return computeStandardErrors; }
            set { computeStandardErrors = value; }
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

       
        public double[] ParameterChange {  get { return deltas; } }

        
        public double MaximumChange {  get { return convergence.NewValue; } }

       
        public LowerBoundNewtonRaphson()
        {
            convergence = new AbsoluteConvergence();
        }

       
        public LowerBoundNewtonRaphson(LogisticRegression1 regression)
            : this()
        {
            init(regression);
        }

        private void init(LogisticRegression1 regression)
        {
            this.regression = regression;

            K = regression.NumberOfOutputs - 1;
            M = regression.NumberOfInputs + 1;
            parameterCount = K * M;

            solution = regression.Coefficients.Reshape();

            xxt = new double[M, M];
            errors = new double[K];
            output = new double[K];

            lowerBound = new double[parameterCount, parameterCount];
            gradient = new double[parameterCount];

            
            weights = (-0.5).Multiply(Matrix.Identity(K).Subtract(Matrix.Create(K, K, 1.0 / M)));
        }


       
        [Obsolete("Please use the Learn() method instead.")]
        public double Run(double[][] inputs, int[] classes)
        {
            int old = Iterations;
            Iterations = 1;
            Learn(inputs, classes);
            Iterations = old;
            return deltas.Max();
        }

        [Obsolete("Please use the Learn() method instead.")]
        public double Run(double[][] inputs, double[][] outputs)
        {
            int old = Iterations;
            Iterations = 1;
            Learn(inputs, outputs);
            Iterations = old;
            return deltas.Max();
        }



        private void compute(double[] x, double[] responses)
        {
            double[][] coefficients = this.regression.Coefficients;

            double sum = 1;

            // For each category (except the first)
            for (int j = 0; j < coefficients.Length; j++)
            {
                // Get category coefficients
                double[] c = coefficients[j];

                double logit = c[0]; // intercept

                for (int i = 0; i < x.Length; i++)
                    logit += c[i + 1] * x[i];

                sum += responses[j] = Math.Exp(logit);
            }

            // Normalize the probabilities
            for (int j = 0; j < responses.Length; j++)
                responses[j] /= sum;
        }



       
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

       
        public LogisticRegression1 Learn(double[][] x, int[] y, double[] weights = null)
        {
            return Learn(x, Jagged.OneHot(y), weights);
        }

       
        public LogisticRegression1 Learn(double[][] x, int[][] y, double[] weights = null)
        {
            return Learn(x, y.ToDouble(), weights);
        }

        
        public LogisticRegression1 Learn(double[][] x, double[][] y, double[] weights = null)
        {
            if (weights != null)
              

            if (regression == null)
                init(new LogisticRegression1(x.Columns(), y.Columns()));

            // Initial definitions and memory allocations
            int N = x.Length;

            double[][] design = x.InsertColumn(value: 1, index: 0);
            double[][] coefficients = this.regression.Coefficients;

          

            do
            {
                if (Token.IsCancellationRequested)
                    break;

                // Reset Hessian matrix and gradient
                Array.Clear(gradient, 0, gradient.Length);

                if (UpdateLowerBound)
                    Array.Clear(lowerBound, 0, lowerBound.Length);

                // For each input sample in the dataset
                for (int i = 0; i < x.Length; i++)
                {
                    // Grab variables related to the sample
                    double[] rx = design[i];
                    double[] ry = y[i];

                    // Compute and estimate outputs
                    this.compute(x[i], output);

                    // Compute errors for the sample
                    for (int j = 0; j < errors.Length; j++)
                        errors[j] = ry[j + 1] - output[j];


                   
                    double[] g = Matrix.Kronecker(errors, rx);
                    for (int j = 0; j < g.Length; j++)
                        gradient[j] += g[j];

                    if (UpdateLowerBound)
                    {
                        // Compute xxt matrix
                        for (int k = 0; k < rx.Length; k++)
                            for (int j = 0; j < rx.Length; j++)
                                xxt[k, j] = rx[k] * rx[j];

                        // (Re-) Compute weighted "Hessian" matrix 
                        double[,] h = Matrix.Kronecker(this.weights, xxt);
                        for (int j = 0; j < parameterCount; j++)
                            for (int k = 0; k < parameterCount; k++)
                                lowerBound[j, k] += h[j, k];
                    }
                }


                if (UpdateLowerBound)
                {
                    UpdateLowerBound = false;

                   


                    decomposition = new SingularValueDecomposition(lowerBound);
                    deltas = decomposition.Solve(gradient);
                }
                else
                {
                    deltas = decomposition.Solve(gradient);
                }


                previous = coefficients.Reshape();

                // Update coefficients using the calculated deltas
                for (int i = 0, k = 0; i < coefficients.Length; i++)
                    for (int j = 0; j < coefficients[i].Length; j++)
                        coefficients[i][j] -= deltas[k++];

                solution = coefficients.Reshape();

                // Return the relative maximum parameter change
                for (int i = 0; i < deltas.Length; i++)
                    deltas[i] = Math.Abs(deltas[i]) / Math.Abs(previous[i]);

                convergence.NewValue = deltas.Max();

                if (Token.IsCancellationRequested)
                    break;

            } while (!convergence.HasConverged);

            if (computeStandardErrors)
            {
                // Grab the regression information matrix
                double[,] inverse = decomposition.Inverse();

                // Calculate coefficients' standard errors
                double[][] standardErrors = regression.StandardErrors;
                for (int i = 0, k = 0; i < standardErrors.Length; i++)
                    for (int j = 0; j < standardErrors[i].Length; j++, k++)
                        standardErrors[i][j] = Math.Sqrt(Math.Abs(inverse[k, k]));
            }

            return regression;
        }
    }
}
