

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.Statistics.Links;
    using Accord.MachineLearning;
    using Accord.Compat;
    using System.Threading;

    

    public class IterativeReweightedLeastSquares : IterativeReweightedLeastSquares<GeneralizedLinearRegression>,
        IRegressionFitting

    {
        /// <summary>
        ///   Constructs a new Iterative Reweighted Least Squares.
        /// </summary>
        /// 
        public IterativeReweightedLeastSquares()
        {
        }

        public IterativeReweightedLeastSquares(LogisticRegression regression)
        {
           
        }

       
        public IterativeReweightedLeastSquares(GeneralizedLinearRegression regression)
        {
            Initialize(regression);
        }



       
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[] outputs)
        {
            return Run(inputs, outputs.Apply(x => x > 0 ? 1.0 : 0.0));
        }

       
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[] outputs, double[] weights)
        {
            return Run(inputs, outputs.Apply(x => x > 0 ? 1.0 : 0.0), weights);
        }

       
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[][] outputs)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");
            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0] > 0 ? 1.0 : 0.0;
            return Run(inputs, output);
        }

        
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[][] outputs, double[] sampleWeight)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");
            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0] > 0 ? 1.0 : 0.0;
            return Run(inputs, output, sampleWeight);
        }

        
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[][] outputs)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");

            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0];

            return Run(inputs, output);
        }

       
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[] outputs)
        {
            return Run(inputs, outputs, null);
        }

        
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[] outputs, double[] sampleWeights)
        {
            int old = Iterations;
            Iterations = 1;
            Learn(inputs, outputs, sampleWeights);
            Iterations = old;
            return Updates.Abs().Max();
        }

       
        [Obsolete("Please use the LogLikelihoodLoss class instead.")]
        public double ComputeError(double[][] inputs, double[] outputs)
        {
            double sum = 0;

            for (int i = 0; i < inputs.Length; i++)
            {
                double actual = Model.Probability(inputs[i]);
                double expected = outputs[i];
                double delta = actual - expected;
                sum += delta * delta;
            }

            return sum;
        }

    }

    
    public class IterativeReweightedLeastSquares<TModel> :
        ISupervisedLearning<TModel, double[], double>,
        ISupervisedLearning<TModel, double[], int>,
        ISupervisedLearning<TModel, double[], bool>,
        IConvergenceLearning
        where TModel : GeneralizedLinearRegression, new()
    {
        [NonSerialized]
        CancellationToken token = new CancellationToken();

        private TModel regression;

        private int parameterCount;

        private double[][] hessian;
        private double[] gradient;
        private double[] previous;
        private double[] deltas;

        private double lambda = 1e-10;


        private bool computeStandardErrors = true;
        private ISolverArrayDecomposition<double> decomposition;
        private RelativeConvergence convergence;

        /// <summary>
        ///   Initializes this instance.
        /// </summary>
        /// 
        protected void Initialize(TModel regression)
        {
            if (regression == null)
                throw new ArgumentNullException("regression");

            this.regression = regression;
            this.parameterCount = regression.NumberOfParameters;
            this.hessian = Jagged.Zeros(parameterCount, parameterCount);
            this.gradient = new double[parameterCount];
            this.previous = new double[parameterCount];
        }

        public TModel Model
        {
            get { return regression; }
            set { regression = value; }
        }

       
        public double[] Previous { get { return previous; } }

        /// <summary>
        ///   Gets the last parameter updates in the last iteration.
        /// </summary>
        /// 
        public double[] Updates { get { return deltas; } }

        /// <summary>
        ///   Gets the current values for the coefficients.
        /// </summary>
        /// 
        public double[] Solution
        {
            get { return regression.Intercept.Concatenate(regression.Weights); }
        }

        /// <summary>
        ///   Gets the Hessian matrix computed in 
        ///   the last Newton-Raphson iteration.
        /// </summary>
        /// 
        public double[][] Hessian { get { return hessian; } }

        /// <summary>
        ///   Gets the Gradient vector computed in
        ///   the last Newton-Raphson iteration.
        /// </summary>
        /// 
        public double[] Gradient { get { return gradient; } }

        /// <summary>
        ///   Gets the total number of parameters in the model.
        /// </summary>
        /// 
        public int Parameters { get { return parameterCount; } }

        /// <summary>
        /// Gets or sets a cancellation token that can be used to
        /// stop the learning algorithm while it is running.
        /// </summary>
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

        /// <summary>
        ///   Please use MaxIterations instead.
        /// </summary>
        [Obsolete("Please use MaxIterations instead.")]
        public int Iterations
        {
            get { return convergence.MaxIterations; }
            set { convergence.MaxIterations = value; }
        }

        /// <summary>
        ///   Gets or sets the tolerance value used to determine
        ///   whether the algorithm has converged.
        /// </summary>
        public double Tolerance
        {
            get { return convergence.Tolerance; }
            set { convergence.Tolerance = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of iterations
        /// performed by the learning algorithm.
        /// </summary>
        /// <value>The maximum iterations.</value>
        public int MaxIterations
        {
            get { return convergence.MaxIterations; }
            set { convergence.MaxIterations = value; }
        }

        /// <summary>
        /// Gets the current iteration number.
        /// </summary>
        /// <value>The current iteration.</value>
        public int CurrentIteration
        {
            get { return convergence.CurrentIteration; }
        }

        /// <summary>
        /// Gets or sets whether the algorithm has converged.
        /// </summary>
        /// <value><c>true</c> if this instance has converged; otherwise, <c>false</c>.</value>
        public bool HasConverged
        {
            get { return convergence.HasConverged; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether standard
        ///   errors should be computed in the next iteration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to compute standard errors; otherwise, <c>false</c>.
        /// </value>
        /// 
        public bool ComputeStandardErrors
        {
            get { return computeStandardErrors; }
            set { computeStandardErrors = value; }
        }

        /// <summary>
        ///   Gets or sets the regularization value to be
        ///   added in the objective function. Default is
        ///   1e-10.
        /// </summary>
        /// 
        public double Regularization
        {
            get { return lambda; }
            set { lambda = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="IterativeReweightedLeastSquares{TModel}"/> class.
        /// </summary>
        public IterativeReweightedLeastSquares()
        {
            this.convergence = new RelativeConvergence()
            {
                MaxIterations = 0,
                Tolerance = 1e-5
            };
        }



       
        public TModel Learn(double[][] x, int[] y, double[] weights = null)
        {
            return Learn(x, y.ToDouble(), weights);
        }

        
        public TModel Learn(double[][] x, bool[] y, double[] weights = null)
        {
            return Learn(x, Classes.ToZeroOne(y), weights);
        }

  
        public TModel Learn(double[][] x, double[] y, double[] weights = null)
        {
            

            if (x.Length != y.Length)
            {
                throw new DimensionMismatchException("outputs",
                    "The number of input vectors and their associated output values must have the same size.");
            }

            if (regression == null)
            {
                Initialize(new TModel()
                {
                    NumberOfInputs = x.Columns()
                });
            }

            // Initial definitions and memory allocations
            int N = x.Length;

            double[] errors = new double[N];
            double[] w = new double[N];
            convergence.Clear();

            double[][] design = x.InsertColumn(value: 1, index: 0);

            do
            {
                if (Token.IsCancellationRequested)
                    break;

                // Compute errors and weighting matrix
                for (int i = 0; i < x.Length; i++)
                {
                    double z = regression.Linear.Transform(x[i]);
                    double actual = regression.Link.Inverse(z);

                    // Calculate error vector
                    errors[i] = actual - y[i];

                    // Calculate weighting matrix
                    w[i] = regression.Link.Derivative2(actual);
                }

                if (weights != null)
                {
                    for (int i = 0; i < weights.Length; i++)
                    {
                        errors[i] *= weights[i];
                        w[i] *= weights[i];
                    }
                }

                // Reset Hessian matrix and gradient
                gradient.Clear();
                hessian.Clear();

                // (Re-) Compute error gradient
                for (int j = 0; j < design.Length; j++)
                    for (int i = 0; i < gradient.Length; i++)
                        gradient[i] += design[j][i] * errors[j];

                // (Re-) Compute weighted "Hessian" matrix 
                for (int k = 0; k < w.Length; k++)
                {
                    double[] row = design[k];
                    for (int j = 0; j < row.Length; j++)
                        for (int i = 0; i < row.Length; i++)
                            hessian[j][i] += row[i] * row[j] * w[k];
                }

                // Apply L2 regularization
                if (lambda > 0)
                {
                    
                    for (int i = 0; i < gradient.Length; i++)
                    {
                        gradient[i] += lambda * regression.GetCoefficient(i);
                        hessian[i][i] += lambda;
                    }
                }

                decomposition = new JaggedSingularValueDecomposition(hessian);
                deltas = decomposition.Solve(gradient);

                previous = (double[])this.Solution.Clone();

                
                for (int i = 0; i < regression.Weights.Length; i++)
                    regression.Weights[i] -= deltas[i + 1];
                regression.Intercept -= deltas[0];

                // Return the relative maximum parameter change
                convergence.NewValue = deltas.Abs().Max();

                if (Token.IsCancellationRequested)
                    break;

            } while (!convergence.HasConverged);

            if (computeStandardErrors)
            {
                // Grab the regression information matrix
                double[][] inverse = decomposition.Inverse();

                // Calculate coefficients' standard errors
                double[] standardErrors = regression.StandardErrors;
                for (int i = 0; i < standardErrors.Length; i++)
                    standardErrors[i] = Math.Sqrt(inverse[i][i]);
            }

            return regression;
        }

       
        public double[][] GetInformationMatrix()
        {
            return decomposition.Inverse();
        }
    }
}
