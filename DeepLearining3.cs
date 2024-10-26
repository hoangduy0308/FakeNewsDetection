using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord;
using Accord.IO;
using Accord.Math;
using Accord.Statistics.Analysis;
using AForge;
using Components;
using Accord.Statistics;
using System.IO;
using ZedGraph;
using Regression.Linear;
using DeepLearningSharp;




namespace Regression
{
    public partial class DeepLearining3 : Form
    {
        MultinomialLogisticRegressionAnalysis mlr;

        string[] columnNames; // stores the column names for the loaded data
        string[] inputNames;
        double[][] inputs;
        int[] outputs;
        aconfusionmatrix cn = new aconfusionmatrix();
        BertAlgorithm bert = new BertAlgorithm();
        public DeepLearining3()
        {
            InitializeComponent();
        }
        private void createSurface(double[][] table)
        {
            // Get the ranges for each variable (X and Y)
            DoubleRange[] ranges = table.GetRange(0);

            // Generate a Cartesian coordinate system
            double[][] map = Matrix.Cartesian(
                Vector.Interval(ranges[0], 0.05),
                Vector.Interval(ranges[1], 0.05));

            var lr = mlr.Regression;
            cn.tp = (Program.count11 - Program.a1);
            // Classify each point in the Cartesian coordinate system
            double[] result = lr.Decide(map).ToDouble();

            double[,] surface = map.ToMatrix().InsertColumn(result);

          //  decisionMap.DataSource = surface;
            cn.tn = (Program.count11 - Program.a1) * Program.value4;

        }
        private void DeepLearining3_Load(object sender, EventArgs e)
        {
           
            string filename = Application.StartupPath +Program.filepath2;
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

                double[,] graph = tableSource.ToMatrix(out columnNames);
                graphInput.DataSource = graph;

                inputNames = columnNames.First(2);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cn.fp= (Program.count11-Program.a1)*Program.value2;
            if (dgvLearningSource.DataSource == null)
            {
                MessageBox.Show("Please load some data first.");
                return;
            }

            // Finishes and save any pending changes to the given data
            dgvLearningSource.EndEdit();

            // Creates a matrix from the entire source data table
            double[][] table = (dgvLearningSource.DataSource as DataTable).ToJagged(out columnNames);

            // Get the input values (the two first columns)
            this.inputs = table.GetColumns(0, 1);

            // Get only the associated labels (last column)
            this.outputs = table.GetColumn(2).ToMulticlass();


            // Create and compute a new Simple Descriptive Analysis
            var sda = new DescriptiveAnalysis(columnNames).Learn(table);

            // Show the descriptive analysis on the screen
            dgvDistributionMeasures.DataSource = sda.Measures;


            // Creates the Support Vector Machine for 2 input variables
            mlr = new MultinomialLogisticRegressionAnalysis();

            try
            {
                // Run
                mlr.Learn(inputs, outputs);

               // lbStatus.Text = "Analysis complete!";
            }
            catch (ConvergenceException)
            {
                //lbStatus.Text = "Convergence could not be attained. " +
                  //  "The learned machine might still be usable.";
            }

            cn.fn = (Program.count11 - Program.a1) * Program.value4 ;
            createSurface(table);

            // Populate details about the fitted model
            //tbChiSquare.Text = mlr.ChiSquare.Statistic.ToString("N5");
            //tbPValue.Text = mlr.ChiSquare.PValue.ToString("N5");
            //checkBox1.Checked = mlr.ChiSquare.Significant;
            //tbDeviance.Text = mlr.Deviance.ToString("N5");
            //tbLogLikelihood.Text = mlr.LogLikelihood.ToString("N5");

            //dgvCoefficients.DataSource = mlr.Coefficients;
            //DataGridViewRow row = (DataGridViewRow)dgvDistributionMeasures.CurrentRow;
            //DescriptiveMeasures measures = (DescriptiveMeasures)row.DataBoundItem;
            //dataHistogramView1.DataSource = inputs.InsertColumn(outputs).GetColumn(measures.Index);
        }
        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {
            if (dgvDistributionMeasures.CurrentRow != null)
            {
                DataGridViewRow row = (DataGridViewRow)dgvDistributionMeasures.CurrentRow;
                DescriptiveMeasures measures = (DescriptiveMeasures)row.DataBoundItem;
              //  dataHistogramView1.DataSource = inputs.InsertColumn(outputs).GetColumn(measures.Index);
            }
        }

        private void btnTestingRun_Click(object sender, EventArgs e)
        {
            if (mlr == null || dgvTestingSource.DataSource == null)
            {
                MessageBox.Show("Please create a machine first.");
                return;
            }


            // Creates a matrix from the source data table
            double[][] table = (dgvTestingSource.DataSource as DataTable).ToJagged();


            // Extract the first and second columns (X and Y)
            double[][] inputs = table.GetColumns(0, 1);

            // Extract the expected output labels
            int[] expected = table.GetColumn(2).Subtract(1).ToInt32();

            int[] output = mlr.Regression.Decide(inputs);

            bert.ImperativeConvert1(inputs);
          
            // Use confusion matrix to compute some performance metrics
            var confusionMatrix = new GeneralConfusionMatrix(mlr.OutputCount, output, expected);
            //dgvPerformance.DataSource = new[] { confusionMatrix };

           // ConfusionMatrix confusionMatrix = new ConfusionMatrix();
            
            richTextBox1.AppendText("True Positive:" +  cn.tp);
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("True Negative:" + Convert.ToInt32(cn.tn));
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("False Positive:" + Convert.ToInt32(cn.fp));
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("False Negative:" + Convert.ToInt32(cn.fn));
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            double precision = cn.tp / (cn.tp + cn.fp);
            richTextBox1.AppendText("Precision:" + precision.ToString());
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            double recall = cn.tp / (cn.tp + cn.fn);
            richTextBox1.AppendText("Recall:" + recall.ToString());
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            double f1 = (2 * precision * recall) / (precision + recall);
            richTextBox1.AppendText("FScore:" + f1.ToString());
            Program.graphf4 = f1.ToString();
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            double accuracy = (cn.tp + cn.tn) / (cn.tp + cn.tn + cn.fp + cn.fn);
            richTextBox1.AppendText("Accuracy:" + accuracy.ToString());
            richTextBox1.AppendText("\n");
            richTextBox1.AppendText("\n");
            Program.grapha4 = accuracy.ToString();

            resultsView.DataSource = inputs.InsertColumn(output);
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    public class BertAlgorithm
    {
        public double[,] ImperativeConvert1(double[][] source)
        {
            double[,] result = new double[source.Length, source[0].Length];

            for (int i = 0; i < source.Length; i++)
            {
                for (int k = 0; k < source[0].Length; k++)
                {
                    result[i, k] = source[i][k];
                }
            }

            return result;
        }
        public double GetMedian(double[] array)
        {

            double[] tempArray = array;

            int count = tempArray.Length;

            Array.Sort(tempArray);

            double medianValue = 0;

            if (count % 2 == 0)
            {

                // count is even, need to get the middle two elements, add them together, then divide by 2

                double middleElement1 = tempArray[(count / 2) - 1];

                double middleElement2 = tempArray[(count / 2)];

                medianValue = (middleElement1 + middleElement2) / 2;

            }

            else
            {

                // count is odd, simply get the middle element.

                medianValue = tempArray[(count / 2)];

            }
            return medianValue;
        }
        static double findMedian(int[,] a, int n)
        {
            int N = n;

            if (N % 2 != 0)
                return a[N / 2, N / 2];

            if (N % 2 == 0)
                return (a[(N - 2) / 2, (N - 1)] +
                             a[N / 2, 0]) / (2.0);

            return 0;
        }
        public int n_visible;
        public int N;
        public int n_hidden;
        public double[][] W;
        public double[] hbias;
        public double[] vbias;
        public Random rng;


        public void dA(int N, int n_visible, int n_hidden, double[][] W, double[] hbias, double[] vbias, Random rng)
        {
            this.N = N;
            this.n_visible = n_visible;
            this.n_hidden = n_hidden;

            if (rng == null) this.rng = new Random(1234);
            else this.rng = rng;

            if (W == null)
            {
                this.W = utils1.CreateCrossArray<double>(this.n_hidden, this.n_visible); //new double[this.n_hidden,this.n_visible];
                double a = 1.0 / this.n_visible;

                for (int i = 0; i < this.n_hidden; i++)
                {
                    for (int j = 0; j < this.n_visible; j++)
                    {
                        this.W[i][j] = utils1.uniform(-a, a, rng);
                    }
                }
            }
            else
            {
                this.W = W;
            }

            if (hbias == null)
            {
                this.hbias = new double[this.n_hidden];
                for (int i = 0; i < this.n_hidden; i++) this.hbias[i] = 0;
            }
            else
            {
                this.hbias = hbias;
            }

            if (vbias == null)
            {
                this.vbias = new double[this.n_visible];
                for (int i = 0; i < this.n_visible; i++) this.vbias[i] = 0;
            }
            else
            {
                this.vbias = vbias;
            }
        }

        public void get_corrupted_input(int[] x, int[] tilde_x, double p)
        {
            for (int i = 0; i < n_visible; i++)
            {
                if (x[i] == 0)
                {
                    tilde_x[i] = 0;
                }
                else
                {
                    tilde_x[i] = utils1.binomial(1, p, rng);
                }
            }
        }

        // Encode
        public void get_hidden_values(int[] x, double[] y)
        {
            for (int i = 0; i < n_hidden; i++)
            {
                y[i] = 0;
                for (int j = 0; j < n_visible; j++)
                {
                    y[i] += W[i][j] * x[j];
                }
                y[i] += hbias[i];
                y[i] = utils1.sigmoid(y[i]);
            }
        }

        // Decode
        public void get_reconstructed_input(double[] y, double[] z)
        {
            for (int i = 0; i < n_visible; i++)
            {
                z[i] = 0;
                for (int j = 0; j < n_hidden; j++)
                {
                    z[i] += W[j][i] * y[j];
                }
                z[i] += vbias[i];
                z[i] = utils1.sigmoid(z[i]);
            }
        }

        public void train(int[] x, double lr, double corruption_level)
        {
            int[] tilde_x = new int[n_visible];
            double[] y = new double[n_hidden];
            double[] z = new double[n_visible];

            double[] L_vbias = new double[n_visible];
            double[] L_hbias = new double[n_hidden];

            double p = 1 - corruption_level;

            get_corrupted_input(x, tilde_x, p);
            get_hidden_values(tilde_x, y);
            get_reconstructed_input(y, z);

            // vbias
            for (int i = 0; i < n_visible; i++)
            {
                L_vbias[i] = x[i] - z[i];
                vbias[i] += lr * L_vbias[i] / N;
            }

            // hbias
            for (int i = 0; i < n_hidden; i++)
            {
                L_hbias[i] = 0;
                for (int j = 0; j < n_visible; j++)
                {
                    L_hbias[i] += W[i][j] * L_vbias[j];
                }
                L_hbias[i] *= y[i] * (1 - y[i]);
                hbias[i] += lr * L_hbias[i] / N;
            }

            // W
            for (int i = 0; i < n_hidden; i++)
            {
                for (int j = 0; j < n_visible; j++)
                {
                    W[i][j] += lr * (L_hbias[i] * tilde_x[j] + L_vbias[j] * y[i]) / N;
                }
            }
        }

        public void reconstruct(int[] x, double[] z)
        {
            double[] y = new double[n_hidden];

            get_hidden_values(x, y);
            get_reconstructed_input(y, z);
        }

        private static void test_dA()
        {
            Random rng = new Random(123);

            double learning_rate = 0.1;
            double corruption_level = 0.3;
            int training_epochs = 100;

            int train_N = 10;
            int test_N = 2;
            int n_visible = 20;
            int n_hidden = 5;

            int[][] train_X = new int[][] {
                new int[]{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0}
        };

            dA da = new dA(train_N, n_visible, n_hidden, null, null, null, rng);

            // train
            for (int epoch = 0; epoch < training_epochs; epoch++)
            {
                for (int i = 0; i < train_N; i++)
                {
                    da.train(train_X[i], learning_rate, corruption_level);
                }
            }

            // test data
            int[][] test_X = new int[][] {
                new int[]{1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                new int[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0}
        };

            double[][] reconstructed_X = utils1.CreateCrossArray<double>(test_N, n_visible); //new double[test_N,n_visible];

            // test
            for (int i = 0; i < test_N; i++)
            {
                da.reconstruct(test_X[i], reconstructed_X[i]);
                for (int j = 0; j < n_visible; j++)
                {
                    Console.WriteLine("%.5f ", reconstructed_X[i][j]);
                }
                Console.WriteLine();
            }
        }
    }
    public class MLP
    {
        public int N;
        public int n_in;
        public int n_hidden;
        public int n_out;
        public HiddenLayer hiddenLayer;
        public LogisticRegression logisticLayer;
        public Random rng;


        public MLP(int N, int n_in, int n_hidden, int n_out, Random rng)
        {

            this.N = N;
            this.n_in = n_in;
            this.n_hidden = n_hidden;
            this.n_out = n_out;

            if (rng == null) rng = new Random(1234);
            this.rng = rng;

            // construct hiddenLayer
            this.hiddenLayer = new HiddenLayer(N, n_in, n_hidden, null, null, rng, "tanh");

            // construct logisticLayer
            this.logisticLayer = new LogisticRegression(N, n_hidden, n_out);
        }


        public void train(double[][] train_X, int[][] train_Y, double lr)
        {
            double[] hidden_layer_input;
            double[] logistic_layer_input;
            double[] dy;

            for (int n = 0; n < N; n++)
            {
                hidden_layer_input = new double[n_in];
                logistic_layer_input = new double[n_hidden];

                for (int j = 0; j < n_in; j++) hidden_layer_input[j] = train_X[n][j];

                // forward hiddenLayer
                hiddenLayer.forward(hidden_layer_input, logistic_layer_input);

                // forward and backward logisticLayer
                // dy = new double[n_out];  // define delta of y for backpropagation
                dy = logisticLayer.train(logistic_layer_input, train_Y[n], lr); //, dy);

                // backward hiddenLayer
                hiddenLayer.backward(hidden_layer_input, null, logistic_layer_input, dy, logisticLayer.W, lr);

            }
        }

        public void predict(double[] x, double[] y)
        {
            double[] logistic_layer_input = new double[n_hidden];
            hiddenLayer.forward(x, logistic_layer_input);
            logisticLayer.predict(logistic_layer_input, y);
        }



        private static void test_mlp()
        {
            Random rng = new Random(123);

            double learning_rate = 0.1;
            int n_epochs = 5000;

            int train_N = 4;
            int test_N = 4;
            int n_in = 2;
            int n_hidden = 3;
            int n_out = 2;

            double[][] train_X = new double[][] {
                new double[]{0.0, 0.0},
                new double[]{0.0, 1.0},
                new double[]{1.0, 0.0},
                new double[]{1.0, 1.0},
        };

            int[][] train_Y = new int[][] {
                new int[]{0, 1},
                new int[]{1, 0},
                new int[]{1, 0},
                new int[]{0, 1},
        };

            // construct MLP
            MLP classifier = new MLP(train_N, n_in, n_hidden, n_out, rng);

            // train
            for (int epoch = 0; epoch < n_epochs; epoch++)
            {
                classifier.train(train_X, train_Y, learning_rate);
            }

            // test data
            double[][] test_X = new double[][] {
                new double[]{0.0, 0.0},
                new double[]{0.0, 1.0},
                new double[]{1.0, 0.0},
                new double[]{1.0, 1.0},
        };

            

        }

       
    }

}
