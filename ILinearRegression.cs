

namespace Accord.Statistics.Models.Regression.Linear
{
    using System;

   
    [Obsolete("Please use ITransform instead.")]
    public interface ILinearRegression
    {
       
        double[] Compute(double[] inputs);
    }
}
