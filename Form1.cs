using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections;
using Components;
using Accord.MachineLearning;

namespace Regression.Linear
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int cc = 0;
        int a1 = Program.a;
      //  DBConnection con1 = new DBConnection();
        ArrayList arrcolumn = new ArrayList();
        ArrayList arrcolumn1 = new ArrayList();
        ArrayList colname = new ArrayList();
        public string fname;
        public static int rc = 0;
        public static string pdata = Application.StartupPath + "\\tfile.txt";
        public static int count = 0;
        public static string fdata1 = "";
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                OpenFileDialog dig = new OpenFileDialog();
                // dig.Filter = "(*.arff)|*.arff";
                if (dig.ShowDialog() == DialogResult.OK)
                {

                    txtfilepath.Text = dig.FileName;
                    string text1 = File.ReadAllText(txtfilepath.Text);
                    richTextBox1.AppendText(text1);
                    Program.fpath1 = txtfilepath.Text;
                    label5.Text = "Reading CSV file Completed";

                    if (File.Exists(Application.StartupPath + "\\Excel\\daaset.xls"))
                    {
                        File.Delete(Application.StartupPath + "\\Excel\\daaset.xls");
                    }
                    button2.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("an error occured.....");
            }
        }
        public DataTable ReadCsvFile()
        {

            DataTable dtCsv = new DataTable();
            string Fulltext;
          

            using (StreamReader sr = new StreamReader(txtfilepath.Text))
            {
                while (!sr.EndOfStream)
                {
                    Fulltext = sr.ReadToEnd().ToString(); //read full file text  
                    string[] rows = Fulltext.Split('\n'); //split full file text into rows  

                    for (int i = 0; i < rows.Count() - 1; i++)
                    {
                      
                        string[] rowValues = rows[i].Split(','); //split each row with comma to get individual values  
                        {
                            if (i == 0)
                            {
                                for (int j = 0; j < rowValues.Count(); j++)
                                {

                                    dtCsv.Columns.Add(rowValues[j]); //add headers
                                    richColumns.AppendText(rowValues[j]);
                                    richColumns.AppendText("\n");
                                }
                            }
                            else
                            {
                                DataRow dr = dtCsv.NewRow();
                                for (int k = 0; k < rowValues.Count(); k++)
                                {
                                    dr[k] = rowValues[k].ToString();
                                    if (k == 1)
                                    {
                                        richData.AppendText(rowValues[k].ToString());
                                    }
                                }
                                dtCsv.Rows.Add(dr); //add other rows  
                                count++;
                            }
                        }
                    }
                }
            }
            Program.count11 = count;
            label5.Text = "Row count" + count;
            return dtCsv;
        }  

        private void button2_Click(object sender, EventArgs e)
        {
            label5.Text = "Data Preprocessing Started....Waiting...";
            DataTable dt = new DataTable();
            DataGridView dgv = new DataGridView();
            dt = ReadCsvFile();
            dataGridView1.DataSource = dt;
            dataGridView1.Width = 1000;
            Controls.Add(dataGridView1);
            dataGridView1.Columns[0].Width = 70;
            dataGridView1.Columns[1].Width = 800;
            dataGridView1.Columns[2].Width = 10;
           
            // richData.SaveFile(pdata,);
           // System.IO.File.WriteAllText(pdata,richData.Text.Replace("\n", Environment.NewLine));
            //  dataGridView1.DataBind();  
           
            // button5.Enabled = true;
            //SaveFileDialog sfd = new SaveFileDialog();

            //if (sfd.ShowDialog() == DialogResult.OK)
            //{
            //    fname = sfd.FileName;


            //}
            label5.Text = " Binary Preprocessing Started....Waiting...";
            createxcel();
            label5.Text = " Dat Preprocessing Completed.";
            button3.Enabled = true;

        }
        public void splitdata()
        {
            if (richTextBox1.Text != "")
            {
                string[] a = Regex.Split(richTextBox1.Text, "@data");
                richData.AppendText(a[1].ToString());
                makecolumns(a[0].ToString());
                fillvalues(a[1].ToString());

            }
        }
        public void makecolumns(string text)
        {
            //dataGridView1.AutoGenerateColumns = false;
            string parse1 = text.Replace("@relation", "");
            string parse2 = parse1.Replace("@attribute", "");
            string parse3 = parse2.Replace("numeric", "");
            string parse4 = parse3.Replace("real", "");
            richColumns.AppendText(parse4);

            string column = parse4.Replace("\n", "");
           

            string[] columnnames = Regex.Split(column.Trim(), "  ");
            string[] datasetname = columnnames[0].Split(' ');

     
            DataGridViewColumn c2 = new DataGridViewColumn();
            c2.CellTemplate = new DataGridViewTextBoxCell();
            c2.HeaderText = datasetname[1].ToString();
           
            dataGridView1.Columns.Add(c2);
         //   listBox1.Items.Add(c2.HeaderText);
           // dataGridView1.Columns[0].Width = 630;
            for (int i = 1; i < columnnames.Count(); i++)
            {
                DataGridViewColumn c1 = new DataGridViewColumn();
                c1.CellTemplate = new DataGridViewTextBoxCell();
                c1.HeaderText = columnnames[i].ToString();
                //dataGridView1.Columns[0].Width = 100;// The id column 
                //dataGridView1.Columns[1].Width = 200;
                dataGridView1.Columns.Add(c1);
                
               // listBox1.Items.Add(columnnames[i].ToString());
            }
           lblclassname.Text ="Dataset Class Name : "+ dataGridView1.Columns[dataGridView1.ColumnCount - 1].HeaderText;

        }
        public void fillvalues(string data)
        {
            data.Trim();
            int ccount = dataGridView1.ColumnCount;

            string[] columntext = Regex.Split(data, "\n");
            int rcount = columntext.Count() - 1;

            for (int i = 0; i <= rcount-1; i++)
            {
                if (columntext[i] != "" && columntext[i] != " ")
                {
                    dataGridView1.Rows.Add();
                    string[] a = columntext[i].Split(',');
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        dataGridView1.Rows[i - 1].Cells[j].Value = a[j].ToString();
                        //dataGridView1.Rows[i].Cells[j].Value = a[j].ToString();

                    }
                }
                else
                {

                }
            }


            lblrowcount.Text = "Dataset Row Count : " + dataGridView1.RowCount.ToString();
            lblcolumncount.Text = "Dataset Column Count : " + dataGridView1.ColumnCount.ToString();
            rc = dataGridView1.RowCount;
            Program.p = rc;


        }
        public string calculatewordtovectorentropy(string w)
        {
             ArrayList ALPHABET_ITEMS_ARRAYLIST = new ArrayList();
             string MESSAGE =w;
             string ws;
            char[] messageArray = MESSAGE.ToCharArray();
            Array.Sort(messageArray);
            if (MESSAGE == "")
            {
                ws = "0".ToString();
            }
            else
            {

                for (int i = 0; i < messageArray.Length; i++)
                {
                    char current = messageArray[i];

                    AlphabetItem newItem = new AlphabetItem();
                    newItem.Item = current;
                    newItem.NoOfAppearances = 1;

                    int index = ALPHABET_ITEMS_ARRAYLIST.IndexOf(newItem);

                    if (index == -1)
                    {
                        ALPHABET_ITEMS_ARRAYLIST.Add(newItem);
                    }
                    else
                    {
                        AlphabetItem existing = (AlphabetItem)ALPHABET_ITEMS_ARRAYLIST[index];
                        existing.NoOfAppearances++;
                        ALPHABET_ITEMS_ARRAYLIST[index] = existing;
                    }
                }
                AlphabetItem currentItem = (AlphabetItem)ALPHABET_ITEMS_ARRAYLIST[0];
                string wf = currentItem.CalculateFrequency(MESSAGE.Length).ToString();
                double value = 0;
                for (int i = 0; i < ALPHABET_ITEMS_ARRAYLIST.Count; i++)
                {
                    AlphabetItem currentItem1 = (AlphabetItem)ALPHABET_ITEMS_ARRAYLIST[i];

                    value += currentItem1.CalculateSummationElementValue(MESSAGE.Length);
                }

                double summation = value;

               ws = Math.Abs(summation).ToString();
            }

            return (ws);
        }
        public string calculatevector(string w)
        {
            ArrayList ALPHABET_ITEMS_ARRAYLIST = new ArrayList();
            string MESSAGE = w;
            string wf;
            char[] messageArray = MESSAGE.ToCharArray();
            Array.Sort(messageArray);
            if (MESSAGE == "")
            {
                 wf = "0".ToString();
            }
            else
            {
                for (int i = 0; i < messageArray.Length; i++)
                {
                    char current = messageArray[i];

                    AlphabetItem newItem = new AlphabetItem();
                    newItem.Item = current;
                    newItem.NoOfAppearances = 1;

                    int index = ALPHABET_ITEMS_ARRAYLIST.IndexOf(newItem);

                    if (index == -1)
                    {
                        ALPHABET_ITEMS_ARRAYLIST.Add(newItem);
                    }
                    else
                    {
                        AlphabetItem existing = (AlphabetItem)ALPHABET_ITEMS_ARRAYLIST[index];
                        existing.NoOfAppearances++;
                        ALPHABET_ITEMS_ARRAYLIST[index] = existing;
                    }
                }
                AlphabetItem currentItem = (AlphabetItem)ALPHABET_ITEMS_ARRAYLIST[0];



                wf = currentItem.CalculateFrequency(MESSAGE.Length).ToString();
            }
           

            return (wf);
        }

      
        public void createxcel()
        {
            
            Excel.Application xlApp;
            Excel.Workbook xlWorkBook;
            Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlApp = new Excel.Application();
            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            int i = 0;
            int j = 0;
            int cc3 = 0;
            //for (int k = 0; k <= dataGridView1.ColumnCount - 1; k++)
            //{
            //    xlWorkSheet.Cells[i + 1, j + 1] = 0;
            //}
            for (i = 0; i < dataGridView1.RowCount - 2; i++)
            {
                for (j = 0; j <= dataGridView1.ColumnCount - 1; j++)
                {
                  
                   
                  
                    if (j == dataGridView1.ColumnCount - 3)
                    {
                        string sw = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        string sa1= calculatevector(sw);
                        xlWorkSheet.Cells[i + 1, j + 1] = sa1;
                    }
                    else if (j == dataGridView1.ColumnCount - 2)
                    {
                        string sw = dataGridView1.Rows[i].Cells[1].Value.ToString();
                        string sa2 = calculatewordtovectorentropy(sw);
                        xlWorkSheet.Cells[i + 1, j + 1] = sa2;
                    }

                    else if (j == dataGridView1.ColumnCount - 1)
                    {


                        DataGridViewCell cell = dataGridView1[j, i];
                        if (cell.Value.ToString().Trim() == "real".ToString().Trim())
                        {

                            string s = "0".ToString();
                            xlWorkSheet.Cells[i + 1, j + 1] = s;
                        }                      
                        else
                        {
                            string s = "1".ToString();
                            xlWorkSheet.Cells[i + 1, j + 1] = s;
                        }

                    }
                    else
                    {

                        DataGridViewCell cell = dataGridView1[j, i];
                        xlWorkSheet.Cells[i + 1, j + 1] = cell.Value;
                    }
                }

            }
            string fff99 = Application.StartupPath + fdata1;
            xlWorkBook.SaveAs(fff99, Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            releaseObject(xlWorkSheet);
            releaseObject(xlWorkBook);
            releaseObject(xlApp);

            MessageBox.Show("Data preprocessing completed.. ");
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
        private void button3_Click(object sender, EventArgs e)
        {

            button4.Enabled = true;
            FeatureGeneration obj = new FeatureGeneration();
            obj.Show();



        }
        private void ToCsV(DataGridView dGV, string filename)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++)
            {
                string stLine = "";
                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
                stOutput += stLine + "\r\n";
            }
            Encoding utf16 = Encoding.GetEncoding(1254);
            byte[] output = utf16.GetBytes(stOutput);
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
             //int cc = listBox2.Items.Count;
             //if (cc >= 1)
             //{
             //    string cname = listBox1.SelectedItem.ToString();

             //    int c = dataGridView1.Rows.Count;

             //    for (int j = 0; j < dataGridView1.ColumnCount - 1; j++)
             //    {
             //        if (cname.ToString() == dataGridView1.Columns[j].HeaderText.ToString())
             //        {
             //            cc = j;
             //        }
             //    }
             //    for (int i = 0; i < dataGridView1.RowCount - 1; i++)
             //    {
             //        arrcolumn.Add(dataGridView1.Rows[i].Cells[cc].Value.ToString());
             //    }
                
             //}
             //else
             //{
             //    MessageBox.Show("Please select one feature");
             //}

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int count = listBox2.Items.Count;
            //if (count < 1)
            //{
            //    listBox2.Items.Add(listBox1.SelectedItem.ToString());
            //}
            //else
            //{
            //    MessageBox.Show("Already selected"); 
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            fdata1 = Program.filepath1;
            button2.Enabled = false;
           button3.Enabled = false;

            button4.Enabled = true;
          // button5.Enabled = false;

        }


        private void button6_Click(object sender, EventArgs e)
        {
            
            
            
                
        }
        private DataTable MergeColumns(DataTable dt1, DataTable dt2)
        {
            DataTable result = new DataTable();
            foreach (DataColumn dc in dt1.Columns)
            {
                result.Columns.Add(new DataColumn(dc.ColumnName, dc.DataType));
            }
            foreach (DataColumn dc in dt2.Columns)
            {
                result.Columns.Add(new DataColumn(dc.ColumnName, dc.DataType));
            }
            for (int i = 0; i < Math.Max(dt1.Rows.Count, dt2.Rows.Count); i++)
            {
                DataRow dr = result.NewRow();
                if (i < dt1.Rows.Count)
                {
                    for (int c = 0; c < dt1.Columns.Count; c++)
                    {
                        dr[c] = dt1.Rows[i][c];
                    }
                }
                if (i < dt2.Rows.Count)
                {
                    for (int c = 0; c < dt2.Columns.Count; c++)
                    {
                        dr[dt1.Columns.Count + c] = dt2.Rows[i][c];
                    }
                }
                result.Rows.Add(dr);
            }
            return result;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
        }

        private void button7_Click_1(object sender, EventArgs e)
        {
            double [,] arrtrain=new double[dataGridView1.RowCount,dataGridView1.ColumnCount];
            double[,] arrtest = new double[2, dataGridView1.ColumnCount];
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount - 1; j++)
                {
                   // arrcolumn1.Add(dataGridView1.Rows[i].Cells[j].Value.ToString());
                    arrtrain[i, j] = Convert.ToDouble(dataGridView1.Rows[i].Cells[j].Value.ToString());
                }
            }
            
            for (int j = 0; j < dataGridView1.ColumnCount - 1; j++)
            {
             // arrcolumn.Add(dataGridView1.Rows[5].Cells[j].Value.ToString());
                arrtest[0, j] = Convert.ToDouble(dataGridView1.Rows[5].Cells[j].Value.ToString());
            }
            

          
        }

        private void button7_Click_2(object sender, EventArgs e)
        {
            
        }

        private void splitContainer4_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            MainForm fg1 = new MainForm();
            fg1.Show();
          
        }

        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.FillWeight = 10; 
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Application.Exit();

        }
       
    }
    public class AlphabetItem
    {
        private char item;
        private int noOfAppearances;

        public char Item
        {
            get { return item; }
            set { item = value; }
        }

        public int NoOfAppearances
        {
            get { return noOfAppearances; }
            set { noOfAppearances = value; }
        }

        public double CalculateFrequency(int iLengthOfMessage)
        {
            return (noOfAppearances / (double)iLengthOfMessage);
        }

        public double CalculateSummationElementValue(int iLengthOfMessage)
        {
            double frequency = CalculateFrequency(iLengthOfMessage);
            return (frequency * Math.Log(frequency, 2));
        }

        public override bool Equals(object obj)
        {
            var compareItem = obj as AlphabetItem;

            // If compareItem is not an instance of AlphabetItem
            if (compareItem == null)
            {
                return false;
            }

            if (compareItem.item == item)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
