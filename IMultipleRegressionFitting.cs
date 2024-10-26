

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;

   
    [Obsolete("Please use ISupervisedLearning instead.")]
    interface IMultipleRegressionFitting
    {

        
        double Run(double[][] inputs, double[][] outputs);

    }
}
