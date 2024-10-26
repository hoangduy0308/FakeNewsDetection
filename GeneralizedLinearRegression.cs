

namespace Accord.Statistics.Models.Regression
{
    using System;
    using System.Linq;
    using Accord.Statistics.Links;
    using Accord.Statistics.Testing;
    using Accord.MachineLearning;
    using Accord.Math;
    using Accord.Statistics.Models.Regression.Linear;
    using System.Runtime.Serialization;
    using Accord.Compat;

    
    [Serializable]
    public class GeneralizedLinearRegression : BinaryLikelihoodClassifierBase<double[]>, ICloneable
    {
        private MultipleLinearRegression linear;
        private ILinkFunction linkFunction;


        [Obsolete]
        private double[] coefficients;


        private double[] standardErrors;


        
        public GeneralizedLinearRegression(ILinkFunction function)
        {
            this.linear = new MultipleLinearRegression();
            this.linkFunction = function;
            this.NumberOfInputs = 1;
        }

        
        [Obsolete("Please use the default constructor and set the NumberOfInputs property instead.")]
        public GeneralizedLinearRegression(ILinkFunction function, int inputs)
            : this(function)
        {
            this.NumberOfInputs = inputs;
        }

       
        [Obsolete("Please use the default constructor and set the NumberofInputs and Intercept properties instead.")]
        public GeneralizedLinearRegression(ILinkFunction function, int inputs, double intercept)
            : this(function)
        {
            this.NumberOfInputs = inputs;
            this.Intercept = intercept;
        }

       
        public GeneralizedLinearRegression()
            : this(new LogitLinkFunction())
        {
        }

       
        [Obsolete("Please use the default constructor and set the Weights and StandardErrors properties instead.")]
        public GeneralizedLinearRegression(ILinkFunction function,
            double[] coefficients, double[] standardErrors)
            : this()
        {
            this.linkFunction = function;
            this.Weights = coefficients.Get(1, 0);
            this.StandardErrors = standardErrors;
        }

         
        public override int NumberOfInputs
        {
            get { return Linear.NumberOfInputs; }
            set
            {
                Linear.NumberOfInputs = value;
                this.standardErrors = Vector.Create(value + 1, this.standardErrors);
            }
        }

        
        [Obsolete("Please use Weights and Intercept instead.")]
        public double[] Coefficients
        {
            get { return Intercept.Concatenate(Weights); }
        }

        
        public int NumberOfParameters
        {
            get { return Linear.NumberOfParameters; }
        }

        
        public double[] Weights
        {
            get { return Linear.Weights; }
            set { linear.Weights = value; }
        }

        
        public double[] StandardErrors
        {
            get { return standardErrors; }
            set { standardErrors = value; }
        }

        
        [Obsolete("Please use NumberOfInputs instead.")]
        public int Inputs
        {
            get { return NumberOfInputs; }
        }

        
        public ILinkFunction Link
        {
            get { return linkFunction; }
            protected set { linkFunction = value; }
        }

       
        public MultipleLinearRegression Linear
        {
            get { return linear; }
            protected set { linear = value; }
        }

        
        public double Intercept
        {
            get { return linear.Intercept; }
            set { linear.Intercept = value; }
        }

       
        public double GetCoefficient(int index)
        {
            if (index == 0)
                return Intercept;
            return Weights[index - 1];
        }

       
        public void SetCoefficient(int index, double value)
        {
            if (index == 0)
            {
                Intercept = value;
            }
            else
            {
                Weights[index - 1] = value;
            }
        }


        
        [Obsolete("Please use the Probability method instead.")]
        public double Compute(double[] input)
        {
            return Probability(input);
        }

       
        [Obsolete("Please use the Probability method instead.")]
        public double[] Compute(double[][] input)
        {
            return Probability(input);
        }


        
        public WaldTest GetWaldTest(int index)
        {
            
            return new WaldTest(GetCoefficient(index), 0.0, standardErrors[index]);
        }


        
        public double GetLogLikelihood(double[][] input, double[] output)
        {
            double sum = 0;

            for (int i = 0; i < input.Length; i++)
            {
                double actualOutput = Probability(input[i]);
                double expectedOutput = output[i];

                if (actualOutput != 0)
                    sum += expectedOutput * Math.Log(actualOutput);

                if (actualOutput != 1)
                    sum += (1 - expectedOutput) * Math.Log(1 - actualOutput);

                Accord.Diagnostics.Debug.Assert(!Double.IsNaN(sum));
            }

            return sum;
        }

        
        public double GetLogLikelihood(double[][] input, double[] output, double[] weights)
        {
            double sum = 0;

            for (int i = 0; i < input.Length; i++)
            {
                double w = weights[i];
                double actualOutput = Probability(input[i]);
                double expectedOutput = output[i];

                if (actualOutput != 0)
                    sum += expectedOutput * Math.Log(actualOutput) * w;

                if (actualOutput != 1)
                    sum += (1 - expectedOutput) * Math.Log(1 - actualOutput) * w;

                Accord.Diagnostics.Debug.Assert(!Double.IsNaN(sum));
            }

            return sum;
        }

       
        public double GetDeviance(double[][] input, double[] output)
        {
            return -2.0 * GetLogLikelihood(input, output);
        }

        
        public double GetDeviance(double[][] input, double[] output, double[] weights)
        {
            return -2.0 * GetLogLikelihood(input, output, weights);
        }

       
        public double GetLogLikelihoodRatio(double[][] input, double[] output, GeneralizedLinearRegression regression)
        {
            return 2.0 * (this.GetLogLikelihood(input, output) - regression.GetLogLikelihood(input, output));
        }

       
        public double GetLogLikelihoodRatio(double[][] input, double[] output, double[] weights, GeneralizedLinearRegression regression)
        {
            return 2.0 * (this.GetLogLikelihood(input, output, weights) - regression.GetLogLikelihood(input, output, weights));
        }


       
        public ChiSquareTest ChiSquare(double[][] input, double[] output)
        {
            double y0 = 0;
            double y1 = 0;

            for (int i = 0; i < output.Length; i++)
            {
                y0 += 1.0 - output[i];
                y1 += output[i];
            }

            var regression = new GeneralizedLinearRegression(linkFunction)
            {
                NumberOfInputs = NumberOfInputs,
                Intercept = Math.Log(y1 / y0)
            };

            double ratio = GetLogLikelihoodRatio(input, output, regression);
            return new ChiSquareTest(ratio, NumberOfInputs);
        }

        
        public ChiSquareTest ChiSquare(double[][] input, double[] output, double[] weights)
        {
            double y0 = 0;
            double y1 = 0;

            for (int i = 0; i < output.Length; i++)
            {
                y0 += (1.0 - output[i]) * weights[i];
                y1 += output[i] * weights[i];
            }

            var regression = new GeneralizedLinearRegression(linkFunction)
            { 
                NumberOfInputs = NumberOfInputs,
                Intercept = Math.Log(y1 / y0)
            };

            double ratio = GetLogLikelihoodRatio(input, output, weights, regression);
            return new ChiSquareTest(ratio, NumberOfInputs);
        }

       
        public double GetDegreesOfFreedom(int numberOfSamples)
        {
            return Linear.GetDegreesOfFreedom(numberOfSamples);
        }

        public double[] GetStandardErrors(double[][] informationMatrix)
        {
            double[] se = new double[informationMatrix.Length];
            for (int i = 0; i < se.Length; i++)
                se[i] = Math.Sqrt(informationMatrix[i][i]);
            return se;
        }

        
        public double GetStandardError(double[] input, double[][] informationMatrix)
        {
            double rim = predictionVariance(input, informationMatrix);
            return Math.Sqrt(rim);
        }

       
        public double GetPredictionStandardError(double[] input, double[][] informationMatrix)
        {
            double rim = predictionVariance(input, informationMatrix);
            return Math.Sqrt(1 + rim);
        }

       
        public DoubleRange GetConfidenceInterval(double[] input, int numberOfSamples, double[][] informationMatrix, double percent = 0.95)
        {
            double se = GetStandardError(input, informationMatrix);
            return computeInterval(input, numberOfSamples, percent, se);
        }

        
        public DoubleRange GetPredictionInterval(double[] input, int numberOfSamples, double[][] informationMatrix, double percent = 0.95)
        {
            double se = GetPredictionStandardError(input, informationMatrix);
            return computeInterval(input, numberOfSamples, percent, se);
        }

        private static double predictionVariance(double[] input, double[][] im)
        {
            if (input.Length < im.Length)
                input = (1.0).Concatenate(input);
            return input.Dot(im).Dot(input);
        }

        private DoubleRange computeInterval(double[] input, int numberOfSamples, double percent, double se)
        {
            double y = linear.Transform(input);
            double df = GetDegreesOfFreedom(numberOfSamples);
            var t = new TTest(estimatedValue: y, standardError: se, degreesOfFreedom: df);
            DoubleRange lci = t.GetConfidenceInterval(percent);
            DoubleRange nci = new DoubleRange(linkFunction.Inverse(lci.Min), linkFunction.Inverse(lci.Max));
            return nci;
        }

        
        public object Clone()
        {
            return new GeneralizedLinearRegression()
             {
                 Link = (ILinkFunction)linkFunction.Clone(),
                 Linear = (MultipleLinearRegression)this.Linear.Clone(),
                 StandardErrors = (double[])this.StandardErrors.Clone()
             };
        }


        
        [Obsolete("Simply cast the logistic regression to a GeneralizedLinearRegression instead, using Clone() if necessary.")]
        public static GeneralizedLinearRegression FromLogisticRegression(LogisticRegression regression, bool makeCopy)
        {
#pragma warning disable 612, 618
            if (makeCopy)
            {
                double[] coefficients = (double[])regression.Coefficients.Clone();
                double[] standardErrors = (double[])regression.StandardErrors.Clone();
                return new GeneralizedLinearRegression(new LogitLinkFunction(),
                    coefficients, standardErrors);
            }
            else
            {
                return new GeneralizedLinearRegression(new LogitLinkFunction(),
                    regression.Coefficients, regression.StandardErrors);
            }
#pragma warning restore 612, 618
        }


        
        public override double[] Score(double[][] input, double[] result)
        {
            linear.Transform(input, result: result);
            //for (int i = 0; i < input.Length; i++)
            //    result[i] = linkFunction.Inverse(result[i]);
            //return result.Subtract(0.5, result: result);
            return result;
        }

        
        public override double[] LogLikelihood(double[][] input, double[] result)
        {
            linear.Transform(input, result: result);
            for (int i = 0; i < input.Length; i++)
                result[i] = linkFunction.Inverse(result[i]);
            return Elementwise.Log(result, result: result);
        }

      
        public override double[] Probability(double[][] input, double[] result)
        {
            linear.Transform(input, result: result);
            for (int i = 0; i < input.Length; i++)
                result[i] = linkFunction.Inverse(result[i]);
            return result;
        }


        [OnDeserialized]
        private void SetValuesOnDeserialized(StreamingContext context)
        {
            if (linear == null)
            {
                linear = new MultipleLinearRegression()
                {
#pragma warning disable 612, 618
                    Weights = coefficients.Get(1, 0),
                    Intercept = coefficients[0]
#pragma warning restore 612, 618
                };
            }
        }
    }
}
