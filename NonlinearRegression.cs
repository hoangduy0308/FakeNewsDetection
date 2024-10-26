
namespace Accord.Statistics.Models.Regression
{
    using Accord.MachineLearning;
    using Accord.Math.Optimization;
    using Accord.Statistics.Models.Regression.Fitting;
    using System;
    using Accord.Compat;

     
    public delegate double RegressionFunction(double[] coefficients, double[] input);

   
    public delegate void RegressionGradientFunction(double[] coefficients, double[] input, double[] result);

   
    [Serializable]
    public class NonlinearRegression : TransformBase<double[], double>, ICloneable
    {
        double[] coefficients;
        double[] standardErrors;

        

        RegressionFunction function;
        RegressionGradientFunction gradient;


       
        public double[] Coefficients
        {
            get { return coefficients; }
        }

      
        public double[] StandardErrors
        {
            get { return standardErrors; }
        }



        
        public RegressionFunction Function
        {
            get { return function; }
        }

        
        public RegressionGradientFunction Gradient
        {
            get { return gradient; }
            set { gradient = value; }
        }




        
        public NonlinearRegression(int parameters, RegressionFunction function)
        {
            this.coefficients = new double[parameters];
            this.standardErrors = new double[parameters];
            this.function = function;
        }

        
        public NonlinearRegression(int parameters, RegressionFunction function, RegressionGradientFunction gradient)
            : this(parameters, function)
        {
            this.gradient = gradient;
        }



       
        [Obsolete("Please use Transform instead.")]
        public double Compute(double[] inputs)
        {
            return function(coefficients, inputs);
        }



       
        public object Clone()
        {
            var clone = new NonlinearRegression(Coefficients.Length, function, gradient);
            clone.coefficients = (double[])this.coefficients.Clone();
            clone.standardErrors = (double[])this.standardErrors.Clone();
            return clone;
        }

        
        public override double Transform(double[] input)
        {
            return function(coefficients, input);
        }
    }
}
