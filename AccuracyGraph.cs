using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Regression.Linear;

namespace Regression
{
    public partial class AccuracyGraph : Form
    {
        public AccuracyGraph()
        {
            InitializeComponent();
            fillChart();
            fillChart2();
        }
        private void fillChart()
        {
            chart1.ChartAreas[0].AxisY.Minimum = 0.8;

            chart1.ChartAreas[0].AxisY.Maximum = 1;
            //AddXY value in chart1 in series named as Salary  
            chart1.Series["Accuracy"].Points.AddXY("CNN", Program.grapha1);
            chart1.Series["Accuracy"].Points.AddXY("RNN", Program.grapha2);
            chart1.Series["Accuracy"].Points.AddXY("LSTM", Program.grapha3);
            chart1.Series["Accuracy"].Points.AddXY("BERT", Program.grapha4);

            //chart title  
            chart1.Titles.Add("Accuracy Chart");
        }
        private void fillChart2()
        {
            chart2.ChartAreas[0].AxisY.Minimum = 0.8;

            chart2.ChartAreas[0].AxisY.Maximum = 1;
            //AddXY value in chart1 in series named as Salary  
            chart2.Series["F1Score"].Points.AddXY("CNN", Program.graphf1);
            chart2.Series["F1Score"].Points.AddXY("RNN", Program.graphf2);
            chart2.Series["F1Score"].Points.AddXY("LSTM", Program.graphf3);
            chart2.Series["F1Score"].Points.AddXY("BERT", Program.graphf4);

            //chart title  
            chart2.Titles.Add("F1Score Chart");
        }
    }
}
