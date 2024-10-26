

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using Accord.Math.Optimization;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Compat;
    using System.Threading;

   
#pragma warning disable 612, 618
    public class NonlinearLeastSquares :
        ISupervisedLearning<NonlinearRegression, double[], double>,
        IRegressionFitting
#pragma warning restore 612, 618
    {
        [NonSerialized]
        CancellationToken token = new CancellationToken();

        private ILeastSquaresMethod solver;
        private NonlinearRegression regression;
        private bool computeStandardErrors = true;
        private int numberOfParameters;

        RegressionFunction function;
        RegressionGradientFunction gradient;


       
        public bool ComputeStandardErrors
        {
            get { return computeStandardErrors; }
            set { computeStandardErrors = value; }
        }

        
        public ILeastSquaresMethod Algorithm
        {
            get { return solver; }
            set { solver = value; }
        }

        
        public int NumberOfParameters
        {
            get { return numberOfParameters; }
            set { numberOfParameters = value; }
        }

       
        public RegressionFunction Function
        {
            get { return function; }
            set { function = value; }
        }

        
        public RegressionGradientFunction Gradient
        {
            get { return gradient; }
            set { gradient = value; }
        }

       
        public double[] StartValues
        {
            get; set;
        }


       
        public NonlinearLeastSquares()
        {

        }

        
        public NonlinearLeastSquares(NonlinearRegression regression)
            : this(regression, new LevenbergMarquardt(regression.Coefficients.Length))
        {
        }

      
        public NonlinearLeastSquares(NonlinearRegression regression, ILeastSquaresMethod algorithm)
        {
            if (regression == null)
                throw new ArgumentNullException("regression");

            if (algorithm == null)
                throw new ArgumentNullException("algorithm");

            if (regression.Gradient == null)
                throw new ArgumentException("The regression must have a gradient function defined.", "regression");

            this.regression = regression;
            this.NumberOfParameters = regression.Coefficients.Length;

            this.solver = algorithm;
            this.solver.Solution = regression.Coefficients;
            this.solver.Function = new LeastSquaresFunction(regression.Function);
            this.solver.Gradient = new LeastSquaresGradientFunction(regression.Gradient);
        }



       
        [Obsolete("Please use the Learn() method instead.")]
        public double Run(double[][] inputs, double[] outputs)
        {
            var c = this.solver as IConvergenceLearning;

            if (c != null)
            {
                c.MaxIterations = 1;
                c.Tolerance = 0;
            } 

            Learn(inputs, outputs);
            return solver.Value;
        }


        
        public CancellationToken Token
        {
            get { return token; }
            set { token = value; }
        }

        
        public NonlinearRegression Learn(double[][] x, double[] y, double[] weights = null)
        {
            if (weights != null)
               // throw new ArgumentException(Accord.Properties.Resources.NotSupportedWeights, "weights");

            if (NumberOfParameters == 0)
            {
                if (regression == null)
                {
                    if (StartValues == null)
                        throw new InvalidOperationException("Please set the number of parameters, starting values, or the initial regression model.");
                    NumberOfParameters = StartValues.Length;
                }
            }

            if (regression == null)
            {
                this.regression = new NonlinearRegression(numberOfParameters, function, gradient);
                if (StartValues != null)
                    this.regression.Coefficients.SetTo(StartValues);
            }

            if (this.solver == null)
                this.solver = new LevenbergMarquardt(numberOfParameters);

            this.solver.NumberOfVariables = numberOfParameters;
            this.solver.Solution = regression.Coefficients;
            this.solver.Function = new LeastSquaresFunction(regression.Function);
            this.solver.Gradient = new LeastSquaresGradientFunction(regression.Gradient);
            this.solver.Token = Token;

            double error = solver.Minimize(x, y);

            if (Double.IsNaN(error) || Double.IsInfinity(error))
                throw new Exception();

            if (computeStandardErrors)
            {
                double[] errors = solver.StandardErrors;
                for (int i = 0; i < errors.Length; i++)
                    regression.StandardErrors[i] = solver.StandardErrors[i];
            }


            return regression;
        }
    }
}
