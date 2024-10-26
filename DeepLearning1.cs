using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.IO;
using Accord.Math;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Statistics.Analysis;
using ZedGraph;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Regression.Linear;

namespace Regression
{
    public partial class DeepLearning1 : Form
    {
        private ActivationNetwork rnn;

        string[] sourceColumns;
        double[,] sourceMatrix;

        private double learningRate = 0.1;
        private double sigmoidAlphaValue = 2.0;
        private int neuronsInFirstLayer = 10;
        private int iterations = 50;
        private bool useRegularization;
        private bool useNguyenWidrow;
        private bool useSameWeights;

        private Thread workerThread = null;
        private volatile bool needToStop = false;
        public DeepLearning1()
        {
            InitializeComponent();
        }

        private void DeepLearning1_Load(object sender, EventArgs e)
        {
            UpdateSettings();
            string filename = Application.StartupPath + Program.filepath2;
            string extension = Path.GetExtension(filename);
            Accord.IO.ExcelReader db = new Accord.IO.ExcelReader(filename, true, false);
            string[] dd = db.GetWorksheetList();
            //TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

            //if (t.ShowDialog(this) == DialogResult.OK)
            //{
            DataTable tableSource = db.GetWorksheet(dd[0]);
            double[,] sourceMatrix = tableSource.ToMatrix(out sourceColumns);

            // Detect the kind of problem loaded.
            if (sourceMatrix.GetLength(1) == 2)
            {
                MessageBox.Show("Missing class column.");
            }
            else
            {
                this.dgvLearningSource.DataSource = tableSource;
                this.dgvTestingSource.DataSource = tableSource.Copy();

                graphInput.DataSource = sourceMatrix;

                // enable "Start" button
                startButton.Enabled = true;
            }
        }
        private void UpdateSettings()
        {
            learningRateBox.Text = learningRate.ToString();
            alphaBox.Text = sigmoidAlphaValue.ToString();
            neuronsBox.Text = neuronsInFirstLayer.ToString();
            iterationsBox.Text = iterations.ToString();
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            if (dgvLearningSource.DataSource == null)
            {
                MessageBox.Show("Please load some data first.");
                return;
            }

            // Finishes and save any pending changes to the given data
            dgvLearningSource.EndEdit();

            // Creates a matrix from the source data table
            sourceMatrix = (dgvLearningSource.DataSource as DataTable).ToMatrix(out sourceColumns);


            // get learning rate
            try { learningRate = double.Parse(learningRateBox.Text); }
            catch { learningRate = 0.1; }

            // get sigmoid's alpha value
            try { sigmoidAlphaValue = Math.Max(0.001, Math.Min(50, double.Parse(alphaBox.Text))); }
            catch { sigmoidAlphaValue = 2; }

            // get neurons count in first layer
            try { neuronsInFirstLayer = Math.Max(5, Math.Min(1000, int.Parse(neuronsBox.Text))); }
            catch { neuronsInFirstLayer = 20; }

            // iterations
            try { iterations = Math.Max(0, int.Parse(iterationsBox.Text)); }
            catch { iterations = 100; }

            useRegularization = cbRegularization.Checked;
            useNguyenWidrow = cbNguyenWidrow.Checked;
            useSameWeights = cbSameWeights.Checked;

            // update settings controls
            UpdateSettings();

            // disable all settings controls except "Stop" button
            EnableControls(false);

            // run worker thread
            needToStop = false;
            workerThread = new Thread(new ThreadStart(SearchSolution));
            workerThread.Start();
        }
        private delegate void EnableCallback(bool enable);

        // Enable/disable controls (safe for threading)
        private void EnableControls(bool enable)
        {
            if (InvokeRequired)
            {
                EnableCallback d = new EnableCallback(EnableControls);
                Invoke(d, new object[] { enable });
            }
            else
            {
                learningRateBox.Enabled = enable;
                alphaBox.Enabled = enable;
                neuronsBox.Enabled = enable;
                iterationsBox.Enabled = enable;

                startButton.Enabled = enable;
                stopButton.Enabled = !enable;
            }
        }
        int iteration;
        double error;

        // Worker thread
        void SearchSolution()
        {
            // number of learning samples
            int samples = sourceMatrix.GetLength(0);

            // prepare learning data
            double[][] inputs = sourceMatrix.GetColumns(0, 1).ToJagged();
            double[][] outputs = sourceMatrix.GetColumn(2).Transpose().ToJagged();

            // create multi-layer neural network
            this.rnn = new ActivationNetwork(new BipolarSigmoidFunction(sigmoidAlphaValue),
                2, neuronsInFirstLayer, 1);

            if (useNguyenWidrow)
            {
                if (useSameWeights)
                    Accord.Math.Random.Generator.Seed = 1;

                NguyenWidrow initializer = new NguyenWidrow(rnn);
                initializer.Randomize();
            }

            // create teacher
            LevenbergMarquardtLearning teacher = new LevenbergMarquardtLearning(rnn, useRegularization);

            // set learning rate and momentum
            teacher.LearningRate = learningRate;

            // iterations
            iteration = 1;

            var ranges = sourceMatrix.GetRange(0);
            double[][] map = Matrix.Mesh(ranges[0], ranges[1], 0.05, 0.05);
            var sw = Stopwatch.StartNew();

            // loop
            while (!needToStop)
            {
                // run epoch of learning procedure
                error = teacher.RunEpoch(inputs, outputs) / samples;

                var result = map.Apply(rnn.Compute).GetColumn(0).Apply(Math.Sign);

                var graph = map.ToMatrix().InsertColumn(result.ToDouble());

                this.Invoke((Action)(() =>
                {
                    zedGraphControl2.DataSource = graph;
                }));

                // increase current iteration
                iteration++;

                elapsed = sw.Elapsed;

                updateStatus();

                // check if we need to stop
                if ((iterations != 0) && (iteration > iterations))
                    break;
            }

            sw.Stop();

            // enable settings controls
            EnableControls(true);
        }

        TimeSpan elapsed = TimeSpan.Zero;

        private void updateStatus()
        {
            if (!currentIterationBox.InvokeRequired)
            {
                currentIterationBox.Text = iteration.ToString();
                currentErrorBox.Text = error.ToString("N10");
                currentElapsed.Text = elapsed.ToString();
            }
            else
            {
                currentIterationBox.BeginInvoke(new Action(updateStatus));
            }
        }

        private void btnTestingRun_Click(object sender, EventArgs e)
        {
           
        }

        public void CreateResultScatterplot(ZedGraphControl zgc, double[][] inputs, double[] expected, double[] output)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();

            // Set the titles
            myPane.Title.IsVisible = false;
            myPane.XAxis.Title.Text = sourceColumns[0];
            myPane.YAxis.Title.Text = sourceColumns[1];



            // Classification problem
            PointPairList list1 = new PointPairList(); // Z = -1, OK
            PointPairList list2 = new PointPairList(); // Z = +1, OK
            PointPairList list3 = new PointPairList(); // Z = -1, Error
            PointPairList list4 = new PointPairList(); // Z = +1, Error
            for (int i = 0; i < output.Length; i++)
            {
                if (output[i] == -1)
                {
                    if (expected[i] == -1)
                        list1.Add(inputs[i][0], inputs[i][1]);
                    if (expected[i] == 1)
                        list3.Add(inputs[i][0], inputs[i][1]);
                }
                else
                {
                    if (expected[i] == -1)
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


            // Fill the background of the chart rect and pane
            myPane.Fill = new Fill(Color.WhiteSmoke);

            zgc.AxisChange();
            zgc.Invalidate();
        }


        private static double computeError(double[][] inputs, double[][] outputs, ActivationNetwork ann)
        {
            // Compute the machine outputs
            int miss = 0;
            for (int i = 0; i < inputs.Length; i++)
            {
                var y = System.Math.Sign(ann.Compute(inputs[i])[0]);
                var o = outputs[i][0];
                if (y != o) miss++;
            }

            return (double)miss / inputs.Length;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            needToStop = true;
        }

        private void cbNguyenWidrow_CheckedChanged(object sender, EventArgs e)
        {
            cbSameWeights.Enabled = cbNguyenWidrow.Checked;
        }
        public int NextInteger(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        public double[,] Add(double[,] x, double[,] y)
        {
            int a = x.GetLength(0);
            int b = x.GetLength(1);
            double[,] z = new double[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        z[i, j] = x[i, j] + y[i, j];
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return z;
        }


        public double[,] Subtract(double[,] x, double[,] y)
        {
            var a = x.GetLength(0);
            var b = x.GetLength(1);
            double[,] z = new double[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        z[i, j] = x[i, j] - y[i, j];
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return z;
        }


        public double[,] Substract(double x, double[,] y)
        {
            var a = y.GetLength(0);
            var b = y.GetLength(1);
            double[,] z = new double[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        z[i, j] = x - y[i, j];
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return z;
        }

        public double[,] Mutiply(double[,] x, double[,] y)
        {
            int a = y.GetLength(0);
            int b = y.GetLength(1);
            double[,] z = new double[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    z[i, j] = x[i, j] * y[i, j];
                }
            }
            return z;
        }

        public double[,] MatrixMutiply(double[,] x, double[,] y)
        {
            int a = y.GetLength(0);
            int b = y.GetLength(1);//need to understand
            double[,] z = new double[a, b];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        z[i, j] = y[i, j] * x[j, i];

                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return z;
        }
        public double[][] MatrixMutiply(double[][] x, double[][] y)
        {
            int a = y.Length;
            int b = y.Length;//need to understand
            double[][] z = new double[a][];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        z[i][j] = x[i][j] * y[j][i];

                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            return z;
        }






        public double[,] Softmax(double[,] x)
        {
            //formula
            //e^x/Sum(e^x)

            int a = x.GetLength(0);
            int b = x.GetLength(1);
            double[,] y = new double[a, b];
            double sum = 0;
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    sum = sum + Math.Exp(x[i, j]);
                }
            }
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    y[i, j] = Math.Exp(x[i, j]) / sum;
                }
            }
            return y;
        }

        public double[] Softmax(double[] x)
        {
            //formula
            //e^x/Sum(e^x)

            int a = x.Length;
            //int b = x[0].Length;
            double[] y = new double[a];
            double sum = 0;
            for (int i = 0; i < a; i++)
            {
                //for (int j = 0; j < b; j++)
                {
                    sum = sum + Math.Exp(x[i]);
                }
            }
            for (int i = 0; i < a; i++)
            {
                //for (int j = 0; j < b; j++)
                {
                    y[i] = Math.Exp(x[i]) / sum;
                }
            }
            return y;
        }

        public double[,] Tanh(double[,] x)
        {
            var tanh = Sigmoid(x);
            int t = tanh.GetLength(0);
            var t2 = tanh.GetLength(1);
            double[,] output = new double[t, t2];
            for (int i = 0; i < t; i++)
            {
                for (int j = 0; j < t2; j++)
                {
                    try
                    {
                        output[i, j] = (tanh[i, j] * 2) - 1;
                    }
                    catch (Exception ex)
                    {
                    }

                }

            }
            //tanh formulae
            //(2*Sigmoid(x)-1)
            //(e^x-e^-x)/(e^x+e^-x)
            return output;
        }

        public double[,] Sigmoid(double[,] x)
        {
            //formula
            //1/(1+e^-x)
            int a = x.GetLength(0);
            int b = x.GetLength(1);
            double[,] y = new double[a, b];

            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        y[i, j] = 1.0 / (1 + Math.Exp(-x[i, j]));
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            return y;
        }

        public double Error()
        {
            return 0.0;
        }



        public Dictionary<string, int> ConvertTexttoInteger(char[] charList)
        {
            Dictionary<string, int> charToInteger = new Dictionary<string, int>();
            for (int i = 0; i < charList.Length; i++)
            {

                if (!charToInteger.ContainsKey(charList[i].ToString()))
                    charToInteger.Add(charList[i].ToString(), i);
            }
            return charToInteger;
        }

        public Dictionary<int, string> ConvertIntegertoText(char[] charList)
        {
            Dictionary<int, string> intToChar = new Dictionary<int, string>();
            for (int i = 0; i < charList.Length; i++)
            {
                if (!intToChar.ContainsKey(i))
                    intToChar.Add(i, charList[i].ToString());
            }
            return intToChar;
        }


        public double NextDouble()
        {
            Random random = new Random();
            return random.NextDouble();
        }


        public double[,] RandomValues(int x, int y)
        {
            double[,] randomValues = new double[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    //randomValues[i][j] = 0.0 + (NextDouble() * (1.0 - 0.0));
                    randomValues[i, j] = (NextDouble() * 0.01);
                }
            }
            return randomValues;
        }

        public double[][] RandomVValues(int x, int y)
        {
            double[][] randomValues = new double[x][];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    //randomValues[i][j] = 0.0 + (NextDouble() * (1.0 - 0.0));
                    randomValues[i][j] = (NextDouble() * 0.01);
                }
            }
            return randomValues;
        }


        public double[,] RandomZeros(int x, int y = 1)
        {
            double[,] zeros = new double[x, y];
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    zeros[i, j] = 0;
                }
            }
            return zeros;
        }
        public double[,] TransposeValues(double[,] x)
        {
            int a = x.GetLength(0);
            int b = x.GetLength(1);
            double[,] y = new double[b, a];
            for (int i = 0; i < a; i++)
            {
                for (int j = 0; j < b; j++)
                {
                    try
                    {
                        y[j, i] = x[i, j];
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }

                }
            }
            return y;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (rnn == null || dgvTestingSource.DataSource == null)
            {
                MessageBox.Show("Please create a machine first.");
                return;
            }


            // Creates a matrix from the source data table
            double[,] sourceMatrix = (dgvTestingSource.DataSource as DataTable).ToMatrix();


            // Extract inputs
            double[][] inputs = new double[sourceMatrix.GetLength(0)][];
            for (int i = 0; i < inputs.Length; i++)
                inputs[i] = new double[] { sourceMatrix[i, 0], sourceMatrix[i, 1] };

            // Get only the label outputs
            int[] expected = new int[sourceMatrix.GetLength(0)];
            for (int i = 0; i < expected.Length; i++)
                expected[i] = (int)sourceMatrix[i, 2];

            // Compute the machine outputs
            int[] output = new int[expected.Length];
            for (int i = 0; i < expected.Length; i++)
                output[i] = System.Math.Sign(rnn.Compute(inputs[i])[0]);

            double[] expectedd = new double[expected.Length];
            double[] outputd = new double[expected.Length];
            for (int i = 0; i < expected.Length; i++)
            {
                expectedd[i] = expected[i];
                outputd[i] = output[i];
            }

            // Use confusion matrix to compute some statistics.
            ConfusionMatrix confusionMatrix = new ConfusionMatrix(output, expected, 1, -1);
            //  dgvPerformance.DataSource = new List<ConfusionMatrix> { confusionMatrix };


            richTextBox1.AppendText("Accuracy:" + confusionMatrix.Accuracy);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            Program.grapha2 = confusionMatrix.Accuracy.ToString();
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
            Program.graphf2 = confusionMatrix.FScore.ToString();
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("ChiSquare:" + confusionMatrix.ChiSquare);

            //foreach (DataGridViewColumn col in dgvPerformance.Columns) col.Visible = true;
            //Column1.Visible = Column2.Visible = false;

            // Create performance scatterplot
            CreateResultScatterplot(zedGraphControl1, inputs, expectedd, outputd);
        }
    }
}

