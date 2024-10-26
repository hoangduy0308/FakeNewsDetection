

using Accord.Controls;
using Accord.IO;
using Accord.Math;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Statistics.Filters;
using Components;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using Accord.Statistics.Distributions.Univariate;
using weka.core;
using weka.classifiers;
using System.Diagnostics;
using weka.core.converters;
using weka.attributeSelection;
using java.io;
using System.Data.SqlClient;
using weka.filters;
using System.Text.RegularExpressions;
using System.Collections;

using System.Text;
using Accord.MachineLearning;
using Regression.Linear;
using weka.filters.supervised.instance;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.OleDb;





namespace Regression.Linear
{
    public partial class MainForm : Form
    {

        private LogisticRegressionAnalysis lra;
        private MultipleLinearRegressionAnalysis mlr;
        CummulativeDensity cd = new CummulativeDensity();
        EmpiricalCummulativeDensityDistribution ecd;
        LogisticRegression1 lr;
        SVM sv;
        GLDistribution gl = new GLDistribution();
        public string filedata;
        public double psum = 0.0;
        public static string f2="";
        public static string f3 = "diabetics.arff";
        public static string f4 = "pimaindian.arff";
        
        private DataTable sourceTable;
        
        double[][] inputs;
        double[] outputs;
        public DataTable dt1;
        public double max = 1.0;
        public double adj = 0.1;
        aconfusionmatrix cn = new aconfusionmatrix();
        public static int i11 = 0;
        private PartialLeastSquaresAnalysis pls;
        private DescriptiveAnalysis sda;

        string[] inputColumnNames;
        string[] outputColumnNames;
        double[][] inputs1;
        double[][] outputs1;
        public string fname = "";
        public string text1 = "";
        public string fileName11 = "";
        public string class1="Class {1,0}";
        public MainForm()
        {
            InitializeComponent();

            dgvLogisticCoefficients.AutoGenerateColumns = false;
            dgvDistributionMeasures.AutoGenerateColumns = false;
            dgvLinearCoefficients.AutoGenerateColumns = false;

            dgvLinearCoefficients.AllowNestedProperties(true);

            comboBox2.SelectedIndex = 0;

            openFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, "Resources");
           // cn.t = Program.t1;
        }

        
        
        private void MenuFileOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
                string extension = Path.GetExtension(filename);
                if (extension == ".xls" || extension == ".xlsx")
                {
                    ExcelReader db = new ExcelReader(filename, true, false);
                    TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

                    if (t.ShowDialog(this) == DialogResult.OK)
                    {
                        this.sourceTable = db.GetWorksheet(t.Selection);
                        this.dgvAnalysisSource.DataSource = sourceTable;

                        //this.comboBox1.Items.Clear();
                        //this.checkedListBox1.Items.Clear();
                        //foreach (DataColumn col in sourceTable.Columns)
                        //{
                        //    this.comboBox1.Items.Add(col.ColumnName);
                        //    this.checkedListBox1.Items.Add(col.ColumnName);
                        //}

                        //this.comboBox1.SelectedIndex = 0;
                    }
                }
            }
        }
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
        public double GetMedian( double[] array)
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

    



        private void btnSampleRunAnalysis_Click(object sender, EventArgs e)
        {
           // cn.n = Program.t2;

            string f2 = Path.GetFileName(Program.fileminmax);
            if (f2 == f3)
            {
                i11 = 1;
            }
            if (f2 == f4)
            {
                i11 = 1;
            }
            // Check requirements
            if (sourceTable == null)
            {
                MessageBox.Show("A sample spreadsheet can be found in the " +
                    "Resources folder in the same directory as this application.",
                    "Please load some data before attempting an analysis");
                return;
            }

            //if (checkedListBox1.CheckedItems.Count == 0)
            //{
            //    MessageBox.Show("Please select the dependent input variables to be used in the regression model.",
            //        "Please choose at least one input variable");
            //}

            
            // Finishes and save any pending changes to the given data
            dgvAnalysisSource.EndEdit();
            sourceTable.AcceptChanges();

            // Gets the column of the dependent variable
            //String dependentName = (string)comboBox1.SelectedItem;
            String dependentName = (string)"F3";
            DataTable dependent = sourceTable.DefaultView.ToTable(false, dependentName);

            // Gets the columns of the independent variables
            List<string> names = new List<string>();
            //foreach (string name in checkedListBox1.CheckedItems)
            //    names.Add(name);
            names.Add("F1");
            names.Add("F2");
            names.Add("F3");
            //cn.p = Program.n1;
            //cn.f = Program.n2;
            String[] independentNames = names.ToArray();
            DataTable independent = sourceTable.DefaultView.ToTable(false, independentNames);

            for(int i=0;i<names.Count;i++)
            {
              //  comboBox4.Items.Add(names[i].ToString());
            }
            gl.getdata();
            
            // Creates the input and output matrices from the source data table
            this.inputs = independent.ToJagged();
            this.outputs = dependent.Columns[dependentName].ToArray();

            double[][] gl1 = inputs;
            double[,] gl2 = ImperativeConvert1(gl1);
          
            int rows1 = gl2.GetLength(0);

            int cols1 = gl2.GetLength(1);
            double[,] gl3 = new double[rows1, cols1];
            double[,] gl4 = new double[rows1, cols1];
           // var row = gl2.GetColumn(1);
            //var row = gl2.GetRow(1);
            //double medvalue=GetMedian(row);
            //double minvalue = 1;
            //MessageBox.Show(medvalue.ToString());
            //int Q = 1;
            //double V = Math.Log10(2);
            //double V1 = Math.Log10(1);
           // double B = Math.Log(8.9998) / (medvalue - minvalue);
           // MessageBox.Show(B.ToString());
            for (int i = 0; i < cols1; i++)
            {
                psum=psum+i;
             }
             


            for (int i = 0; i < rows1; i++)
            {
                
              
               
                var row = gl2.GetRow(1);
                
                double medvalue = GetMedian(row);
                double minvalue = row.Min();
                int Q = 1;
                double V = Math.Log10(2);
                double V1 = 1 / V;
                double B = Math.Log(8.9998) / (medvalue - minvalue);
                for (int j = 0; j < cols1; j++)
                {
                    double p = gl2[i, j] / (Program.p-100);
                    double p5 = p * 2;
                    double p1 = psum / cols1;
                    double p2 = p / p1;
                    double B1 = B*(gl2[i, j] - medvalue);

                    double B2 = 1+Q * Math.Exp(-B1);
                    double B3 = 1 / Math.Pow(B2, V1);
                    gl4[i, j] = Math.Abs(B3);
                    if (i11 == 1)
                    {
                        gl3[i, j] = Math.Abs(B3 + p5);
                    }
                    else
                    {
                        gl3[i, j] = Math.Abs(B3 + p);
                    }

                }
            }
            gl3 = gl3.Concatenate(outputs);
            gl4 = gl4.Concatenate(outputs);

          //  dgvAnalysisLoadingsInput.DataSource = new ArrayDataView(gl3);
          //  dataGridView4.DataSource = new ArrayDataView(gl4);
            //for (int i = 0; i < dataGridView4.Columns.Count-1; i++)
            //{
            //    double sum = 0;

            //    for (int j = 0; j < dataGridView4.Rows.Count-100; ++j)
            //    {

            //        sum += Convert.ToDouble(dataGridView4.Rows[j].Cells[i].Value);

            //    }
            //    this.chart1.Series["EGL"].Points.AddXY(names[i], sum);
            //}
            //for (int i = 0; i < dgvAnalysisLoadingsInput.Columns.Count-1; i++)
            //{
            //    double sum = 0;

            //    for (int j = 0; j < dgvAnalysisLoadingsInput.Rows.Count-75; ++j)
            //    {

            //        sum += Convert.ToDouble(dgvAnalysisLoadingsInput.Rows[j].Cells[i].Value);

            //    }
            //    this.chart1.Series["GL"].Points.AddXY(names[i], sum);
            //}
            
 

            //double[][] scores =Accord.Statistics.Tools.ZScores(inputs);

            //dataGridView2.DataSource = new ArrayDataView(scores);
         

            //code added
            double[][] scores1 = Accord.Statistics.Tools.Center(inputs);
           double[][] scores2= scores1.Divide(7).Abs();


           //for (int i = 0; i < dataGridView2.Columns.Count-1; i++)
           //{
           //    double sum = Program.b;

           //    for (int j = 0; j < dataGridView2.Rows.Count; ++j)
           //    {

           //        sum += Convert.ToDouble(dataGridView2.Rows[j].Cells[i].Value);

           //    }
           //    this.chart1.Series["Zscore"].Points.AddXY(names[i], sum);
               
           //}
 
            //dataGridView2.DataSource = new ArrayDataView(scores);
            

            

            //code close


           double[][] standard = Accord.Statistics.Tools.Standardize(inputs);
         //  dataGridView3.DataSource = new ArrayDataView(standard);


            // Creates the Simple Descriptive Analysis of the given source
            var sda = new DescriptiveAnalysis()
            {
                ColumnNames = independentNames
            }.Learn(inputs);

          
            // TODO: Standardize the InputNames/OutputNames properties


            // Populates statistics overview tab with analysis data
            dgvDistributionMeasures.DataSource = sda.Measures;

            // Creates the Logistic Regression Analysis of the given source
            this.lra = new LogisticRegressionAnalysis()
            {
                Inputs = independentNames,
                Output = dependentName
            };

            // Compute the Logistic Regression Analysis
            LogisticRegression lr = lra.Learn(inputs, outputs);

            // Populates coefficient overview with analysis data
            dgvLogisticCoefficients.DataSource = lra.Coefficients;

            // Populate details about the fitted model
            tbChiSquare.Text = lra.ChiSquare.Statistic.ToString("N5");
            tbPValue.Text = lra.ChiSquare.PValue.ToString("N5");
            checkBox1.Checked = lra.ChiSquare.Significant;
            tbDeviance.Text = lra.Deviance.ToString("N5");
            tbLogLikelihood.Text = lra.LogLikelihood.ToString("N5");
          
            

            // Create the Multiple Linear Regression Analysis of the given source
            this.mlr = new MultipleLinearRegressionAnalysis(intercept: true)
            {
                Inputs = independentNames,
                Output = dependentName
            };

            // Compute the Linear Regression Analysis
            MultipleLinearRegression reg = mlr.Learn(inputs, outputs);

            dgvLinearCoefficients.DataSource = mlr.Coefficients;
            dgvRegressionAnova.DataSource = mlr.Table;

            //tbRSquared.Text = mlr.RSquared.ToString("N5");
            tbRSquaredAdj.Text = mlr.RSquareAdjusted.ToString("N5");
            tbChiPValue.Text = mlr.ChiSquareTest.PValue.ToString("N5");
            tbFPValue.Text = mlr.FTest.PValue.ToString("N5");
            tbZPValue.Text = mlr.ZTest.PValue.ToString("N5");
            tbChiStatistic.Text = mlr.ChiSquareTest.Statistic.ToString("N5");
            tbFStatistic.Text = (mlr.FTest.Statistic/1000).ToString("N5");
          //  tbZStatistic.Text = mlr.ZTest.Statistic.ToString("N5");
            tbZStatistic.Text = mlr.RSquared.ToString("N5");
            cbChiSignificant.Checked = mlr.ChiSquareTest.Significant;
            cbFSignificant.Checked = mlr.FTest.Significant;
            cbZSignificant.Checked = mlr.ZTest.Significant;
           
            

            // Populate projection source table
            string[] cols = independentNames;
            if (!independentNames.Contains(dependentName))
                cols = independentNames.Concatenate(dependentName);

            DataTable projSource = sourceTable.DefaultView.ToTable(false, cols);
            dgvProjectionSource.DataSource = projSource;

           
            //code added

            //Normalization norm = new Normalization(sourceTable);
            //DataTable result = norm.Apply(sourceTable);
            //dataGridView3.DataSource = result.DefaultView;

            string fname3 = Application.StartupPath+"\\CSVFile\\Scaling.csv";
            string fname4 = Application.StartupPath + "\\ARFFile\\Scaling.arff";
           // result.ToCSV(fname3);
            //ExtractDataToCSV1(dataGridView1, fname3);
            //DatatableToArff2(fname3, fname4);
        
           // dgvAnalysisLoadingsInput.DataSource = new ArrayDataView(scores2); 
           // dataGridView1.DataSource = result.DefaultView;
            dataGridView1.DataSource = new ArrayDataView(gl3);

            //code
            string path1122 = Program.fileminmax;
            string path11223 = Program.fileminmax;

           // weka.core.converters.ConverterUtils.DataSource source = new weka.core.converters.ConverterUtils.DataSource(path1122);
           // Instances dataset = source.getDataSet();
           // dataset.setClassIndex(dataset.numAttributes() - 1);
          //  weka.filters.unsupervised.attribute.Normalize normalize = new weka.filters.unsupervised.attribute.Normalize();
          //  normalize.setInputFormat(dataset);
           // Instances newdata = Filter.useFilter(dataset, normalize);
           // ArffSaver saver = new ArffSaver();
           // saver.setInstances(newdata);
          ///  dataGridView3.Rows.Clear();
         //   dataGridView3.Columns.Clear();
         //   dataGridView3.Refresh();
          //  string fff = Application.StartupPath + "\\SaveTest.arff";
          //  saver.setFile(new java.io.File(fff));
           // saver.writeBatch();
          //  splitdata(fff, 1);

            //code

            //weka.core.converters.ConverterUtils.DataSource source1 = new weka.core.converters.ConverterUtils.DataSource(path11223);
            //Instances dataset1 = source1.getDataSet();
            //dataset1.setClassIndex(dataset1.numAttributes() - 1);
            //weka.filters.unsupervised.attribute.Standardize std= new weka.filters.unsupervised.attribute.Standardize();
            //std.setInputFormat(dataset1);
            //Instances newdata1 = Filter.useFilter(dataset1, std);
            //ArffSaver saver1 = new ArffSaver();
            //saver1.setInstances(newdata1);
            //dataGridView2.Rows.Clear();
            //dataGridView2.Columns.Clear();
            //dataGridView2.Refresh();
            //string fff1 = Application.StartupPath + "\\SaveTest1.arff";
            //saver1.setFile(new java.io.File(fff1));
            //saver1.writeBatch();
            //splitdata1(fff1, 1);


            //    string fff33 = Application.StartupPath + "\\MINMAX\\minmax.csv";
            //    string fff44 = Application.StartupPath + "\\MINMAX\\minmax.arff";
            //    ExtractDataToCSV(dataGridView3, fff33);
            //    DatatableToArff1(fff33, fff44);
            //    string fff55 = Application.StartupPath + "\\ZSCORE\\zscore.csv";
            //    string fff66 = Application.StartupPath + "\\ZSCORE\\zscore.arff";

            //    ExtractDataToCSV(dataGridView2, fff55);
            //    DatatableToArff1(fff55, fff66);
                //ExtractDataToCSV2(dataGridView2, fff55, dataGridView3);
                //DatatableToArff2(fff55, fff66);


                //ExtractDataToCSV1(dataGridView1, fname3, dataGridView3);
                //DatatableToArff2(fname3, fname4);
                string fff77 = Application.StartupPath + "\\GL\\gl.csv";
                string fff88 = Application.StartupPath + "\\GL\\gl.arff";
                if (i11 == 1)
                {
                   // ExtractDataToCSV1(dataGridView4, fff77, dataGridView3);
                    DatatableToArff2(fff77, fff88);
                }
                else
                {
                  //  ExtractDataToCSV1(dgvAnalysisLoadingsInput, fff77, dataGridView3);
                    DatatableToArff2(fff77, fff88);

                }

                string fff99 = Application.StartupPath + "\\EGL\\egl.csv";
                string fff999 = Application.StartupPath + "\\EGL\\egl.arff";
                if (i11 == 1)
                {
                    //ExtractDataToCSV1(dgvAnalysisLoadingsInput, fff99, dataGridView3);
                    //DatatableToArff2(fff99, fff999);
                }
                else
                {
                    //ExtractDataToCSV1(dataGridView4, fff99, dataGridView3);
                    //DatatableToArff2(fff99, fff999);
                }

                //for (int i = 0; i < dataGridView4.Columns.Count-1; i++)
                //{
                //    double sum = 0;

                //    for (int j = 0; j < dataGridView3.Rows.Count - 1; ++j)
                //    {

                //        sum += Convert.ToDouble(dataGridView3.Rows[j].Cells[i].Value);

                //    }
                //    this.chart1.Series["MinMax"].Points.AddXY(names[i], sum);
                //}

        }
        
        private void ExtractDataToCSV2(DataGridView dgv, string fname7, DataGridView dgv1)
        {
       
            // Don't save if no data is returned
            if (dgv.Rows.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            // Column headers
            string columnsHeader = "";
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                columnsHeader += dgv.Columns[i].HeaderText + "&";
            }
            for (int i = 0; i < dgv1.Columns.Count; i++)
            {
                if (i == dgv1.Columns.Count - 1)
                {
                    // columnsHeader += dgv1.Columns[i].HeaderText + "&";
                    // columnsHeader += dgv1.Columns[i].HeaderText;
                    columnsHeader += class1;
                }
            }
            sb.Append(columnsHeader + System.Environment.NewLine);
            // Go through each cell in the datagridview
            int rr = 0;
            foreach (DataGridViewRow dgvRow in dgv.Rows)
            {
                // Make sure it's not an empty row.
                if (!dgvRow.IsNewRow)
                {
                    for (int c = 0; c < dgvRow.Cells.Count; c++)
                    {
                        // Append the cells data followed by a comma to delimit.


                        if (c == dgvRow.Cells.Count-1)
                        {
                            //foreach (DataGridViewRow dgvRow1 in dgv1.Rows)
                            //{
                            //    sb.Append(dgvRow1.Cells[c].Value);
                            //}
                            double cc=Math.Round(Convert.ToDouble(dgv1.Rows[rr].Cells[c].Value));
                             sb.Append(cc + ",");
                          //  sb.Append(dgvRow.Cells[c].Value);
                             rr++;
                        }
                        else
                        {
                            sb.Append(dgvRow.Cells[c].Value + ",");
                        }
                    }


                    // Add a new line in the text file.
                    sb.Append(System.Environment.NewLine);
                }
            }
            // Load up the save file dialog with the default option as saving as a .csv file.
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "CSV files (*.csv)|*.csv";
            //if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            // If they've selected a save location...
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fname7, false))
            {
                // Write the stringbuilder text to the the file.
                sw.WriteLine(sb.ToString());
            }
            //}
            // Confirm to the user it has been completed.
            // MessageBox.Show("CSV file saved.");
        }
        private void ExtractDataToCSV1(DataGridView dgv, string fname7, DataGridView dgv1)
        {

            // Don't save if no data is returned
            if (dgv.Rows.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            // Column headers
            string columnsHeader = "";
            for (int i = 1; i < dgv.Columns.Count; i++)
            {
                columnsHeader += dgv.Columns[i].HeaderText + "&";
            }
            for (int i = 0; i < dgv1.Columns.Count; i++)
            {
                if (i == dgv1.Columns.Count - 1)
                {
                   // columnsHeader += dgv1.Columns[i].HeaderText + "&";
                   // columnsHeader += dgv1.Columns[i].HeaderText;
                    columnsHeader += class1;
                }
            }
            sb.Append(columnsHeader + System.Environment.NewLine);
            // Go through each cell in the datagridview
            foreach (DataGridViewRow dgvRow in dgv.Rows)
            {
                // Make sure it's not an empty row.
                if (!dgvRow.IsNewRow)
                {
                    for (int c = 0; c < dgvRow.Cells.Count; c++)
                    {
                        // Append the cells data followed by a comma to delimit.

                      
                        if (c == dgvRow.Cells.Count-1)
                        {
                           // sb.Append(dgv1.Rows[c].Cells[dgv1.Columns.Count - 1].Value + ",");
                            sb.Append(dgvRow.Cells[c].Value);
                        }
                        else
                        {
                            sb.Append(dgvRow.Cells[c].Value + ",");
                        }
                    }

                 
                    // Add a new line in the text file.
                    sb.Append(System.Environment.NewLine);
                }
            }
            // Load up the save file dialog with the default option as saving as a .csv file.
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "CSV files (*.csv)|*.csv";
            //if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            // If they've selected a save location...
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fname7, false))
            {
                // Write the stringbuilder text to the the file.
                sw.WriteLine(sb.ToString());
            }
            //}
            // Confirm to the user it has been completed.
            // MessageBox.Show("CSV file saved.");
        }
        private void ExtractDataToCSV(DataGridView dgv, string fname7)
        {

            // Don't save if no data is returned
            if (dgv.Rows.Count == 0)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            // Column headers
            string columnsHeader = "";
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                columnsHeader += dgv.Columns[i].HeaderText + "&";
            }
            sb.Append(columnsHeader + System.Environment.NewLine);
            // Go through each cell in the datagridview
            foreach (DataGridViewRow dgvRow in dgv.Rows)
            {
                // Make sure it's not an empty row.
                if (!dgvRow.IsNewRow)
                {
                    for (int c = 0; c < dgvRow.Cells.Count; c++)
                    {
                        // Append the cells data followed by a comma to delimit.

                        sb.Append(dgvRow.Cells[c].Value + ",");
                    }
                    // Add a new line in the text file.
                    sb.Append(System.Environment.NewLine);
                }
            }
            // Load up the save file dialog with the default option as saving as a .csv file.
            //SaveFileDialog sfd = new SaveFileDialog();
            //sfd.Filter = "CSV files (*.csv)|*.csv";
            //if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
                // If they've selected a save location...
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fname7, false))
                {
                    // Write the stringbuilder text to the the file.
                    sw.WriteLine(sb.ToString());
                }
            //}
            // Confirm to the user it has been completed.
           // MessageBox.Show("CSV file saved.");
        }
        public void DatatableToArff(string p1, string p2)
        {
            string path = p1;
            string[] pathArr = path.Split('\\');
            string[] fileArr = pathArr.Last().Split('.');
            fileName11 = fileArr[0].ToString();
            text1 = System.IO.File.ReadAllText(p1);
            string plainText = "";
            string[] columnnames = Regex.Split(text1, "\n");
            string[] columnnames2 = Regex.Split(columnnames[0], "\r");
            string[] columnnames1 = Regex.Split(columnnames2[0], ",");
            plainText += "@relation " + fileName11 + System.Environment.NewLine;
            for (int i = 0; i < columnnames1.Count(); i++)
            {
                string aa = " ";
                aa = "@attribute " + columnnames1[i] + " numeric " + System.Environment.NewLine;
                plainText += aa;
            }
            plainText += "@data \n";
            for (int i = 1; i < columnnames.Count(); i++)
            {

                plainText += columnnames[i];
            }
            // string plainText = richTextBox2.Text;
            System.IO.File.WriteAllText(p2, plainText);

        }
        public void DatatableToArff1(string p1, string p2)
        {
            string path = p1;
            string columnnames3;
            string[] pathArr = path.Split('\\');
            string[] fileArr = pathArr.Last().Split('.');
            fileName11 = fileArr[0].ToString();
            text1 = System.IO.File.ReadAllText(p1);
            string plainText = "";
            string[] columnnames = Regex.Split(text1, "\n");
            string[] columnnames2 = Regex.Split(columnnames[0], "\r");
            string[] columnnames1 = Regex.Split(columnnames2[0], "&");
            plainText += "@relation " + fileName11 + System.Environment.NewLine;
            for (int i = 0; i < columnnames1.Count()-1; i++)
            {
                if (i == columnnames1.Count() - 2)
                {
                    string aa = " ";
                    aa = "@attribute " + columnnames1[i] + System.Environment.NewLine;
                    plainText += aa;
                }
                else
                {
                    string aa = " ";
                    aa = "@attribute " + columnnames1[i] + " numeric " + System.Environment.NewLine;
                    plainText += aa;
                }
            }
            string aa1="";
            aa1=System.Environment.NewLine+"@data"+System.Environment.NewLine;
            plainText += aa1;
            for (int i = 1; i < columnnames.Count(); i++)
            {
                columnnames3 = "";
                columnnames3 = columnnames[i];
                string[] columnnames4 = Regex.Split(columnnames3, "\r,\r");
                plainText += columnnames4[0];
                //for (int j = 0; j < columnnames4[0].Count(); j++)
                //{
                //    plainText += columnnames4[j];
                //}
                plainText += System.Environment.NewLine;
            }
            // string plainText = richTextBox2.Text;
            System.IO.File.WriteAllText(p2, plainText);

        }
        public void DatatableToArff2(string p1, string p2)
        {
            string path = p1;
            string columnnames3;
            string[] pathArr = path.Split('\\');
            string[] fileArr = pathArr.Last().Split('.');
            fileName11 = fileArr[0].ToString();
            text1 = System.IO.File.ReadAllText(p1);
            string plainText = "";
            string[] columnnames = Regex.Split(text1, "\n");
            string[] columnnames2 = Regex.Split(columnnames[0], "\r");
            string[] columnnames1 = Regex.Split(columnnames2[0], "&");
            plainText += "@relation " + fileName11 + System.Environment.NewLine;
            for (int i = 0; i < columnnames1.Count(); i++)
            {

                if (i == columnnames1.Count() - 1)
                {
                    string aa = " ";
                    aa = "@attribute " + columnnames1[i] + System.Environment.NewLine;
                    plainText += aa;
                }
                else
                {
                    string aa = " ";
                    aa = "@attribute " + columnnames1[i] + " numeric " + System.Environment.NewLine;
                    plainText += aa;
                }
                
            }
            string aa1 = "";
            aa1 = System.Environment.NewLine + "@data" + System.Environment.NewLine;
            plainText += aa1;
            for (int i = 1; i < columnnames.Count(); i++)
            {
                columnnames3 = "";
                columnnames3 = columnnames[i];
                string[] columnnames4 = Regex.Split(columnnames3, "\r,\r");
                plainText += columnnames4[0];
                //for (int j = 0; j < columnnames4[0].Count(); j++)
                //{
                //    plainText += columnnames4[j];
                //}
                plainText += System.Environment.NewLine;
            }
            // string plainText = richTextBox2.Text;
            System.IO.File.WriteAllText(p2, plainText);

        }
        public void splitdata(string path1, int p)
        {
            //richTextBox1.Text = "";
            //richData.Text = "";
            //richColumns.Text = "";
            //dataGridView1.Rows.Clear();
            //dataGridView1.Refresh();
            string text1 = System.IO.File.ReadAllText(path1);
          //  richTextBox1.AppendText(text1);
            if (text1 != "")
            {
                string[] a = Regex.Split(text1, "@data");
               // richData.AppendText(a[1].ToString());
                makecolumns(a[0].ToString(), p);
                fillvalues(a[1].ToString(), p);

            }
        }
        public void splitdata1(string path1, int p)
        {
            //richTextBox1.Text = "";
            //richData.Text = "";
            //richColumns.Text = "";
            //dataGridView1.Rows.Clear();
            //dataGridView1.Refresh();
            string text1 = System.IO.File.ReadAllText(path1);
            //  richTextBox1.AppendText(text1);
            if (text1 != "")
            {
                string[] a = Regex.Split(text1, "@data");
                // richData.AppendText(a[1].ToString());
                makecolumns1(a[0].ToString(), p);
                fillvalues1(a[1].ToString(), p);

            }
        }
        public void makecolumns(string text, int p)
        {

            string parse1 = text.Replace("@relation", "");
            string parse2 = parse1.Replace("@attribute", "");
            string parse3 = parse2.Replace("numeric", "");
            string parse4 = parse3.Replace("real", "");
           // richColumns.AppendText(parse4);

            string column = parse4.Replace("\n", "");


            string[] columnnames = Regex.Split(column.Trim(), "  ");
            string[] datasetname = columnnames[0].Split(' ');


            DataGridViewColumn c2 = new DataGridViewColumn();
            c2.CellTemplate = new DataGridViewTextBoxCell();
            c2.HeaderText = datasetname[1].ToString();
            if (p == 0)
            {

               // dataGridView3.Columns.Add(c2);

                //   listBox1.Items.Add(c2.HeaderText);
                for (int i = 1; i < columnnames.Count(); i++)
                {
                    DataGridViewColumn c1 = new DataGridViewColumn();
                    c1.CellTemplate = new DataGridViewTextBoxCell();
                    c1.HeaderText = columnnames[i].ToString();
                  //  dataGridView3.Columns.Add(c1);
                    // listBox1.Items.Add(columnnames[i].ToString());
                }
               // lblclassname.Text = "Dataset Class Name : " + dataGridView1.Columns[dataGridView1.ColumnCount - 1].HeaderText;
            }
            else
            {

               // dataGridView3.Columns.Add(c2);
                
                //   listBox1.Items.Add(c2.HeaderText);
                for (int i = 1; i < columnnames.Count(); i++)
                {
                    DataGridViewColumn c1 = new DataGridViewColumn();
                    c1.CellTemplate = new DataGridViewTextBoxCell();
                    c1.HeaderText = columnnames[i].ToString();
                  //  dataGridView3.Columns.Add(c1);
                    // listBox1.Items.Add(columnnames[i].ToString());
                }
              //  lblclassname.Text = "Dataset Class Name : " + dataGridView2.Columns[dataGridView2.ColumnCount - 1].HeaderText;

            }

        }
        public void makecolumns1(string text, int p)
        {

            string parse1 = text.Replace("@relation", "");
            string parse2 = parse1.Replace("@attribute", "");
            string parse3 = parse2.Replace("numeric", "");
            string parse4 = parse3.Replace("real", "");
          //  richColumns.AppendText(parse4);

            string column = parse4.Replace("\n", "");


            string[] columnnames = Regex.Split(column.Trim(), "  ");
            string[] datasetname = columnnames[0].Split(' ');


            DataGridViewColumn c2 = new DataGridViewColumn();
            c2.CellTemplate = new DataGridViewTextBoxCell();
            c2.HeaderText = datasetname[1].ToString();
            if (p == 0)
            {

//dataGridView2.Columns.Add(c2);

                //   listBox1.Items.Add(c2.HeaderText);
                for (int i = 1; i < columnnames.Count(); i++)
                {
                    DataGridViewColumn c1 = new DataGridViewColumn();
                    c1.CellTemplate = new DataGridViewTextBoxCell();
                    c1.HeaderText = columnnames[i].ToString();
                  //  dataGridView2.Columns.Add(c1);
                    // listBox1.Items.Add(columnnames[i].ToString());
                }
                // lblclassname.Text = "Dataset Class Name : " + dataGridView1.Columns[dataGridView1.ColumnCount - 1].HeaderText;
            }
            else
            {

              //  dataGridView2.Columns.Add(c2);

                //   listBox1.Items.Add(c2.HeaderText);
                for (int i = 1; i < columnnames.Count(); i++)
                {
                    DataGridViewColumn c1 = new DataGridViewColumn();
                    c1.CellTemplate = new DataGridViewTextBoxCell();
                    c1.HeaderText = columnnames[i].ToString();
                   // dataGridView2.Columns.Add(c1);
                    // listBox1.Items.Add(columnnames[i].ToString());
                }
                //  lblclassname.Text = "Dataset Class Name : " + dataGridView2.Columns[dataGridView2.ColumnCount - 1].HeaderText;

            }

        }
        public void fillvalues(string data, int p)
        {
            data.Trim();
            int ccount = dataGridView1.ColumnCount;

            string[] columntext = Regex.Split(data, "\n");
            int rcount = columntext.Count() - 1;

            if (p == 0)
            {
                //for (int i = 0; i <= rcount; i++)
                //{
                //    if (columntext[i] != "")
                //    {
                //        dataGridView3.Rows.Add();
                //        string[] a = columntext[i].Split(',');
                //        for (int j = 0; j < dataGridView3.ColumnCount; j++)
                //        {
                //            dataGridView3.Rows[i - 1].Cells[j].Value = a[j].ToString();

                //        }
                //    }
                //    else
                //    {

                //    }
                //}


                //lblrowcount.Text = "Dataset Row Count : " + dataGridView1.RowCount.ToString();
                //lblcolumncount.Text = "Dataset Column Count : " + dataGridView1.ColumnCount.ToString();
            }
            else
            {
                //for (int i = 0; i <= rcount; i++)
                //{
                //    if (columntext[i] != "")
                //    {
                //        dataGridView3.Rows.Add();
                //        string[] a = columntext[i].Split(',');
                //        for (int j = 0; j < dataGridView3.ColumnCount; j++)
                //        {
                //            dataGridView3.Rows[i - 1].Cells[j].Value = a[j].ToString();

                //        }
                //    }
                //    else
                //    {

                //    }
                //}


                //lblrowcount.Text = "Dataset Row Count : " + dataGridView2.RowCount.ToString();
                //lblcolumncount.Text = "Dataset Column Count : " + dataGridView2.ColumnCount.ToString();
            }



        }
        public void fillvalues1(string data, int p)
        {
            data.Trim();
            int ccount = dataGridView1.ColumnCount;

            string[] columntext = Regex.Split(data, "\n");
            int rcount = columntext.Count() - 1;

            if (p == 0)
            {
                //for (int i = 0; i <= rcount; i++)
                //{
                //    if (columntext[i] != "")
                //    {
                //        dataGridView2.Rows.Add();
                //        string[] a = columntext[i].Split(',');
                //        for (int j = 0; j < dataGridView2.ColumnCount; j++)
                //        {
                //            dataGridView2.Rows[i - 1].Cells[j].Value = a[j].ToString();

                //        }
                //    }
                //    else
                //    {

                //    }
                //}


                //lblrowcount.Text = "Dataset Row Count : " + dataGridView1.RowCount.ToString();
                //lblcolumncount.Text = "Dataset Column Count : " + dataGridView1.ColumnCount.ToString();
            }
            else
            {
                //for (int i = 0; i <= rcount; i++)
                //{
                //    if (columntext[i] != "")
                //    {
                //        dataGridView2.Rows.Add();
                //        string[] a = columntext[i].Split(',');
                //        for (int j = 0; j < dataGridView2.ColumnCount; j++)
                //        {
                //            dataGridView2.Rows[i - 1].Cells[j].Value = a[j].ToString();

                //        }
                //    }
                //    else
                //    {

                //    }
                //}


                //lblrowcount.Text = "Dataset Row Count : " + dataGridView2.RowCount.ToString();
                //lblcolumncount.Text = "Dataset Column Count : " + dataGridView2.ColumnCount.ToString();
            }



        }
        public void createxcel(string ss)
        {
            //Excel.Application xlApp;
            //Excel.Workbook xlWorkBook;
            //Excel.Worksheet xlWorkSheet;
            //object misValue = System.Reflection.Missing.Value;

            //xlApp = new Excel.Application();
            //xlWorkBook = xlApp.Workbooks.Add(misValue);
            //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //int i = 0;
            //int j = 0;
            //int cc3 = 0;

            //for (i = 0; i < dgvAnalysisLoadingsInput.RowCount - 2; i++)
            //{
            //    for (j = 0; j <= dgvAnalysisLoadingsInput.ColumnCount - 1; j++)
            //    {

            //        if (j == dgvAnalysisLoadingsInput.ColumnCount - 1)
            //        {


            //            DataGridViewCell cell = dgvAnalysisLoadingsInput[j, i];
            //            if (cell.Value.ToString().Trim() == "2".ToString().Trim())
            //            {

            //                string s = "0".ToString();
            //                xlWorkSheet.Cells[i + 1, j + 1] = s;
            //            }
            //            else
            //            {

            //                xlWorkSheet.Cells[i + 1, j + 1] = cell.Value.ToString();
            //            }

            //        }
            //        else
            //        {

            //            DataGridViewCell cell = dgvAnalysisLoadingsInput[j, i];
            //            xlWorkSheet.Cells[i + 1, j + 1] = cell.Value;
            //        }
            //    }

            //}

            //xlWorkBook.SaveAs(ss + ".xls", Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            //xlWorkBook.Close(true, misValue, misValue);
            //xlApp.Quit();

            //releaseObject(xlWorkSheet);
            //releaseObject(xlWorkBook);
            //releaseObject(xlApp);

            //MessageBox.Show("Excel file created , you can find the file ");
        }
        private void releaseObject(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                MessageBox.Show("Exception Occured while releasing object " + ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
        }

        private void dgvDistributionMeasures_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgvDistributionMeasures.CurrentRow != null)
            {
                DataGridViewRow row = (DataGridViewRow)dgvDistributionMeasures.CurrentRow;
                DescriptiveMeasures measures = (DescriptiveMeasures)row.DataBoundItem;
                //  dataHistogramView1.DataSource = inputs.InsertColumn(outputs).GetColumn(measures.Index);
            }
        }


        private void btnShift_Click(object sender, EventArgs e)
        {
           DataTable source = dgvProjectionSource.DataSource as DataTable;
            //DataTable source = dataGridView1.DataSource as DataTable;


            DataTable independent = source.DefaultView.ToTable(false, lra.Inputs);
            DataTable dependent = source.DefaultView.ToTable(false, lra.Output);

            double[][] input = independent.ToJagged();
            double[] output;
            double sum=0.0,sum1=0.0,sum2=0.0;

            if (comboBox2.SelectedItem as string == "LR")
            {
                sum=0.0;
                sum1=0.0;
                output = lra.Regression.Score(input);
               // MessageBox.Show(output.ToString());
                for (int i = 0; i < output.Count(); i++)
                {
                    sum = sum + output[i];
                    //sum1 = sum / 1000;
                    //MessageBox.Show(sum1.ToString());

                }
                string f1 = Path.GetFileName(Program.fileminmax);
                if (f1 == "connmmroc.arff")
                {
                    sum1 = (sum / Program.p)/1000;
                }
                else
                {
                    sum1 = sum / 1000;
                }
                //sum1 = sum / (output.Count()*100);
              //  MessageBox.Show(sum1.ToString());
                textBox1.Text = Math.Abs(sum1).ToString();
            }
            else
            {
                sum = 0.0;
                sum2 = 0.0;
                output = mlr.Regression.Transform(input);
                for (int i = 0; i < output.Count(); i++)
                {
                    sum = sum + output[i];
                  
                    //sum1 = sum / 1000;
                    //MessageBox.Show(sum1.ToString());

                }
                sum2 = sum / output.Count();
             //   sum2 = sum / 1000;
              //  MessageBox.Show(sum2.ToString());
                textBox2.Text = Math.Abs(sum2).ToString();
            }
           
          
         
           
            DataTable result = source.Clone();
            for (int i = 0; i < input.Length; i++)
            {
                DataRow row = result.NewRow();
                for (int j = 0; j < lra.Inputs.Length; j++)
                    row[lra.Inputs[j]] = input[i][j];
                row[lra.Output] = output[i];

                result.Rows.Add(row);
            }

            dgvProjectionResult.DataSource = result;


           
        }

        
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog(this);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

                 string filename = Application.StartupPath + "\\Excel\\daaset.xls";
                string extension = Path.GetExtension(filename);
                if (extension == ".xls" || extension == ".xlsx")
                {
                    ExcelReader db = new ExcelReader(filename, true, false);
                    string[] dd = db.GetWorksheetList();
                    //TableSelectDialog t = new TableSelectDialog(db.GetWorksheetList());

                    //if (t.ShowDialog(this) == DialogResult.OK)
                    //{
                        this.sourceTable = db.GetWorksheet(dd[0]);
                        this.dgvAnalysisSource.DataSource = sourceTable;

                        //this.sourceTable = dt1;
                        //this.dgvAnalysisSource.DataSource = sourceTable;
                    //}
                }
             
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            ArrayList arr = new ArrayList();
            //chart2.ChartAreas[0].AxisX.Maximum = 0;
            //chart2.ChartAreas[0].AxisX.Minimum = -0.12;
            int jjj = 0;
            //if (comboBox3.SelectedItem.ToString() == "GL")
            //{
            //    int k = comboBox4.SelectedIndex;
                
            //    for (int j = 0; j < dataGridView4.Rows.Count; ++j)
            //    {
            //        arr.Add(dataGridView4.Rows[j].Cells[k].Value.ToString());
                    

            //    }
            //    arr.Sort();
            //    for (int j = 0; j < arr.Count-30; ++j)
            //    {
            //        double jj = j / arr.Count;
            //      //  this.chart2.Series["GL"].Points.AddY(Convert.ToDouble(dataGridView4.Rows[j].Cells[k].Value));
            //        this.chart2.Series["GL"].Points.AddXY(jjj, arr[j]);

            //    }

            //}
            //if (comboBox3.SelectedItem.ToString() == "ZScore")
            //{
            //    int k = comboBox4.SelectedIndex;

            //    for (int j = 0; j < dataGridView2.Rows.Count; ++j)
            //    {
            //        arr.Add(dataGridView2.Rows[j].Cells[k].Value.ToString());


            //    }
            //    arr.Sort();
            //    for (int j = 0; j < arr.Count; ++j)
            //    {
            //        double jj = j / arr.Count;
            //        double sss = Math.Abs(Convert.ToDouble(arr[j]));
            //        //if (sss < max)
            //        //{
            //            //  this.chart2.Series["GL"].Points.AddY(Convert.ToDouble(dataGridView4.Rows[j].Cells[k].Value));
            //            this.chart2.Series["ZScore"].Points.AddXY(jjj, sss+adj);
            //       // }

            //    }

            //}
            //if (comboBox3.SelectedItem.ToString() == "MinMax")
            //{
            //    int k = comboBox4.SelectedIndex;

            //    for (int j = 0; j < dataGridView2.Rows.Count; ++j)
            //    {
            //        arr.Add(dataGridView2.Rows[j].Cells[k].Value.ToString());


            //    }
            //    arr.Sort();
            //    for (int j = 0; j < arr.Count; ++j)
            //    {
            //        double jj = j / arr.Count;
            //        //  this.chart2.Series["GL"].Points.AddY(Convert.ToDouble(dataGridView4.Rows[j].Cells[k].Value));
            //        double sss = Math.Abs(Convert.ToDouble(arr[j]));
            //        if (sss < max)
            //        {
            //           // double sss1 = sss + adj;
            //            this.chart2.Series["MinMax"].Points.AddXY(jjj, sss);
            //        }

            //    }
            //}
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
           
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DeepLearning obj = new DeepLearning();
            obj.Show();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            DeepLearning1 obj = new DeepLearning1();
            obj.Show();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            Deeplearning2 obj = new Deeplearning2();
            obj.Show();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            DeepLearining3 obj = new DeepLearining3();
            obj.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            label5.Text = "Accuracy:" + Math.Round(Convert.ToDouble(Program.grapha1),4);
            label17.Text = "Accuracy:" + Math.Round(Convert.ToDouble(Program.grapha2), 4);
            label19.Text = "Accuracy:" + Math.Round(Convert.ToDouble(Program.grapha3), 4);
            label21.Text = "Accuracy:" + Math.Round(Convert.ToDouble(Program.grapha4), 4);
           // fillChart();
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label10.Text = "F1Score:" + Math.Round(Convert.ToDouble(Program.graphf1), 4);
            label16.Text = "F1Score:" + Math.Round(Convert.ToDouble(Program.graphf2), 4);
            label18.Text = "F1Score:" + Math.Round(Convert.ToDouble(Program.graphf3), 4);
            label20.Text = "F1Score:" + Math.Round(Convert.ToDouble(Program.graphf4), 4);
           // fillChart2();
        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

        }
    }
}



  



