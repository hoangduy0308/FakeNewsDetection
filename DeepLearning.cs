using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Controls;
using Accord.IO;
using Accord.MachineLearning.Bayes;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Distributions.Univariate;
using Components;
using System.IO;
using ZedGraph;
using Regression.Linear;

namespace Regression
{
    public partial class DeepLearning : Form
    {
        private NaiveBayes<NormalDistribution> cnn;

        string[] columnNames;
        string[] classNames;
        public DeepLearning()
        {
            InitializeComponent();
        }

        private void DeepLearning_Load(object sender, EventArgs e)
        {
            string filename = Application.StartupPath + Program.filepath2;
            string extension = Path.GetExtension(filename);
            Accord.IO.ExcelReader db = new Accord.IO.ExcelReader(filename, true, false);
            string[] dd = db.GetWorksheetList();
            //TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

            //if (t.ShowDialog(this) == DialogResult.OK)
            //{
            DataTable tableSource = db.GetWorksheet(dd[0]);
            double[,] sourceMatrix = tableSource.ToMatrix(out columnNames);

            // Detect the kind of problem loaded.
            if (sourceMatrix.GetLength(1) == 2)
            {
                MessageBox.Show("Missing class column.");
            }
            else
            {
                this.dgvLearningSource.DataSource = tableSource;
                this.dgvTestingSource.DataSource = tableSource.Copy();


               // CreateScatterplot(graphInput, sourceMatrix);
            }
          //  lbStatus.Text = "Data loaded! Click the 'learn' button to continue!";
        }
        public void CreateScatterplot(ZedGraphControl zgc, double[,] graph)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = columnNames[0];
            myPane.YAxis.Title.Text = columnNames[1];


            // Classification problem
            PointPairList list1 = new PointPairList(); // Z = 0
            PointPairList list2 = new PointPairList(); // Z = 1
            for (int i = 0; i < graph.GetLength(0); i++)
            {
                if (graph[i, 2] == 0)
                    list1.Add(graph[i, 0], graph[i, 1]);
                if (graph[i, 2] == 1)
                    list2.Add(graph[i, 0], graph[i, 1]);
            }

            // Add the curve
            LineItem myCurve = myPane.AddCurve("G1", list1, Color.Blue, SymbolType.Diamond);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("G2", list2, Color.Green, SymbolType.Diamond);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Green);


            // Fill the background of the chart rect and pane
            //myPane.Chart.Fill = new Fill(Color.White, Color.LightGoldenrodYellow, 45.0f);
            //myPane.Fill = new Fill(Color.White, Color.SlateGray, 45.0f);
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }
        private void btnSampleRunAnalysis_Click(object sender, EventArgs e)
        {
            if (dgvLearningSource.DataSource == null)
            {
                MessageBox.Show("Please load some data first.");
                return;
            }

            classNames = new string[] { "G1", "G2" };


            // Finishes and save any pending changes to the given data
            dgvLearningSource.EndEdit();

            // Creates a matrix from the source data table
            double[,] table = (dgvLearningSource.DataSource as DataTable).ToMatrix(out columnNames);

            // Get only the input vector values
            double[][] inputs = table.GetColumns(0, 1).ToJagged();

            // Get only the label outputs
            int[] outputs = table.GetColumn(2).ToInt32();
            string[] colNames = columnNames.Get(0, 2);

            // Create the Bayes classifier and perform classification
            var teacher = new NaiveBayesLearning<NormalDistribution>();

            // Estimate the model using the data
            cnn = teacher.Learn(inputs, outputs);

            // Show the estimated distributions and class probabilities
            dataGridView1.DataSource = new ArrayDataView(cnn.Distributions, colNames);


            // Generate samples for class 1
            var x1 = cnn.Distributions[0, 0].Generate(1000);
            var y1 = cnn.Distributions[0, 1].Generate(1000);

            // Generate samples for class 2
            var x2 = cnn.Distributions[1, 0].Generate(1000);
            var y2 = cnn.Distributions[1, 1].Generate(1000);

            // Combine in a single graph
            double[,] w1 = Matrix.Stack(x1, y1).Transpose();
            double[,] w2 = Matrix.Stack(x2, y2).Transpose();

            double[] z = Vector.Ones(2000);
            for (int i = 0; i < 1000; i++)
                z[i] = 0;

            var a = Matrix.Stack<double>(new double[][,] { w1, w2 });
            var graph = a.Concatenate(z);

            CreateScatterplot(zedGraphControl2, graph);


           // lbStatus.Text = "Classifier created! See the other tabs for details!";
        }
        public void CreateResultScatterplot(ZedGraphControl zgc, double[][] inputs, double[] expected, double[] output)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = columnNames[0];
            myPane.YAxis.Title.Text = columnNames[1];



            // Classification problem
            PointPairList list1 = new PointPairList(); // Z = 0, OK
            PointPairList list2 = new PointPairList(); // Z = 1, OK
            PointPairList list3 = new PointPairList(); // Z = 0, Error
            PointPairList list4 = new PointPairList(); // Z = 1, Error
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == 0)
                {
                    if (expected[i] == 0)
                        list1.Add(inputs[i][0], inputs[i][1]);
                    if (expected[i] == 1)
                        list3.Add(inputs[i][0], inputs[i][1]);
                }
                else
                {
                    if (expected[i] == 0)
                        list4.Add(inputs[i][0], inputs[i][1]);
                    if (expected[i] == 1)
                        list2.Add(inputs[i][0], inputs[i][1]);
                }
            }

            // Add the curve
            LineItem
            myCurve = myPane.AddCurve("G1 Hits", list1, Color.Blue, SymbolType.Diamond);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("G2 Hits", list2, Color.Green, SymbolType.Diamond);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Green);

            myCurve = myPane.AddCurve("G1 Miss", list3, Color.Blue, SymbolType.Plus);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = true;
            myCurve.Symbol.Fill = new Fill(Color.Blue);

            myCurve = myPane.AddCurve("G2 Miss", list4, Color.Green, SymbolType.Plus);
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = true;
            myCurve.Symbol.Fill = new Fill(Color.Green);


            // Fill the chart panel background color
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
                dataGridView1.Rows[i].HeaderCell.Value = classNames[i];

            dataGridView1.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
        }
        public double[,,] ConvolutionLayer(double[,,] input, double[,,,] filter)
        {
            double[,,] output = new double[input.GetLength(0), input.GetLength(1), input.GetLength(2)];
            try
            {
                //formula for output is Output = Input - Filter + 1  eg: Output = 255 - 5 + 1
                for (int i = 0; i < filter.GetLength(0); i++)
                {
                    for (int j = 0; j < input.GetLength(0); j++)
                    {
                        for (int k = 0; k < input.GetLength(1); k++)
                        {
                            for (int l = 0; l < input.GetLength(2); l++)
                            {
                                output[j, k, l] = input[j, k, l] * filter[i, j, k, l];
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {


            }
            return output;
        }
        public double[,,] ActivationLayer(double[,,] input)
        {
            double[,,] output = new double[input.GetLength(0), input.GetLength(1), input.GetLength(2)];
            try
            {
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    for (int k = 0; k < input.GetLength(1); k++)
                    {
                        for (int l = 0; l < input.GetLength(2); l++)
                        {
                            output[j, k, l] = input[j, k, l] < 0 ? 0 : input[j, k, l];
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }
        public double[,,] MaxPoolingLayer(double[,,] input, int filtersize)
        {
            double[,,] output = null;
            try
            {
                //Formula for MaxPooling
                // newwidth = Width/filtersize and newheight = Height/filtersize
                // width+newwidth + width and height+newheight +heigth

                var newHeight = ((input.GetLength(1) - filtersize) / 2) + 1;
                var newWidth = ((input.GetLength(2) - filtersize) / 2) + 1;
                //output= new double[input.GetLength(0), input.GetLength(1), input.GetLength(2)]; for (int j = 0; j < input.GetLength(0); j++)
                output = new double[input.GetLength(0), newHeight, newWidth];
                for (int j = 0; j < input.GetLength(0); j++)
                {
                    var cuurent_y = 0; var out_y = 0;
                    for (int k = cuurent_y + filtersize; k < input.GetLength(1); k++)
                    {
                        var cuurent_x = 0; var out_x = 0;
                        var rowValue = input[j, k, 0] * newWidth + input[j, k, 0];
                        for (int l = cuurent_x + filtersize; l < input.GetLength(2); l++)
                        {
                            var columnValue = input[j, k, l] * newHeight + input[j, k, l];
                            double maxValue = MaxValue(input, j, k, l, filtersize);
                            output[j, out_y, out_x] = input[j, k, l] > maxValue ? input[j, k, l] : maxValue; // using which is maximum value
                            cuurent_x = cuurent_x + 2;
                            out_x = out_x + 1;
                        }
                        cuurent_y = cuurent_y + 2;
                        out_y = out_y + 1;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }



        public double MaxValue(double[,,] input, int i, int j, int k, int filtersize)
        {
            double maxValue = 0;
            try
            {
                for (int a = 0; a < j + filtersize; a++)
                {
                    for (int b = 0; b < k + filtersize; b++)
                    {
                        maxValue = maxValue < input[i, a, b] ? input[i, a, b] : maxValue;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return maxValue;
        }
        public double[] FlatternLayer(double[,,] input)
        {

            int rgbChannel = input.GetLength(0);
            int rowPixel = input.GetLength(1);
            int columnPixel = input.GetLength(2);
            int length = rgbChannel * rowPixel * columnPixel;
            double[] output = new double[length];
            try
            {
                int count = 0;
                for (int i = 0; i < rgbChannel; i++)
                {
                    for (int j = 0; j < rowPixel; j++)
                    {
                        for (int k = 0; k < columnPixel; k++)
                        {
                            output[count] = input[i, j, k];
                            count = count + 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return output;
        }

        public double FullyConnectedLayer(double[] input, double[] weights)
        {
            double sum = 0;
            try
            {
                for (int i = 0; i < input.Length; i++)
                {
                    sum = sum + (input[i] * weights[i]);
                }
            }
            catch (Exception ex)
            {


            }
            return sum;
        }



        public double[,,,] Filter(int filter, int nooffilters, int pixelsize)
        {
            double[,,,] doubleFilter = new double[filter, nooffilters, pixelsize, pixelsize];
            Random random = new Random();
            for (int i = 0; i < filter; i++)
            {
                for (int j = 0; j < nooffilters; j++)
                {
                    for (int k = 0; k < pixelsize; k++)
                    {
                        for (int l = 0; l < pixelsize; l++)
                        {
                            doubleFilter[i, j, k, l] = random.NextDouble();
                        }
                    }
                }
            }

            return doubleFilter;
        }


        public double[] RandomWeights(int count)
        {
            double[] weights = new double[count];
            Random random = new Random();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    weights[i] = random.NextDouble();
                }
            }
            catch (Exception ex)
            {

            }
            return weights;
        }

        private void btnTestingRun_Click(object sender, EventArgs e)
        {
            if (cnn == null || dgvTestingSource.DataSource == null)
            {
                MessageBox.Show("Please create a classifier first.");
                return;
            }


            // Creates a matrix from the source data table
            double[,] table = (dgvLearningSource.DataSource as DataTable).ToMatrix();


            // Get only the input vector values
            double[][] inputs = table.Get(null, 0, 2).ToJagged();

            // Get only the label outputs
            int[] expected = new int[table.GetLength(0)];
            for (int i = 0; i < expected.Length; i++)
                expected[i] = (int)table[i, 2];

            // Compute the machine outputs
            int[] output = cnn.Decide(inputs);


            // Use confusion matrix to compute some statistics.
            ConfusionMatrix confusionMatrix = new ConfusionMatrix(output, expected, 1, 0);
            richTextBox1.AppendText("Accuracy:" + confusionMatrix.Accuracy);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            Program.grapha1= confusionMatrix.Accuracy.ToString();
            richTextBox1.AppendText("True Positive:" + confusionMatrix.TruePositives);
             richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("True Negative:" + confusionMatrix.TrueNegatives);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("False Positive:" + confusionMatrix.FalsePositives);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("False Negative:" + confusionMatrix.FalseNegatives);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("Precision:" + confusionMatrix.Precision);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("Recall:" + confusionMatrix.Recall);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("FScore:" + confusionMatrix.FScore);
            Program.graphf1 = confusionMatrix.FScore.ToString();
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("ChiSquare:" + confusionMatrix.ChiSquare);

            
            //dgvPerformance.DataSource = new List<ConfusionMatrix> { confusionMatrix };
            //listBox1.Items.Add(confusionMatrix.Accuracy);

            //foreach (DataGridViewColumn col in dgvPerformance.Columns)

            //    col.Visible = true;
            //Column1.Visible = Column2.Visible = false;
            //   listBox1.Items.Add(dgvPerformance.Rows[1].Cells["Accuracy"].Value.ToString());

            // Create performance scatter plot
            // CreateResultScatterplot(zedGraphControl1, inputs, expected.ToDouble(), output.ToDouble());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
