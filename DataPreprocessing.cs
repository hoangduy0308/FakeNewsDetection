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
using Regression.Linear;

namespace Regression
{
    public partial class DataPreprocessing : Form
    {
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
        public DataPreprocessing()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            label1.Text = "Data Preprocessing Started....Waiting...";
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
            label1.Text = " Binary Preprocessing Started....Waiting...";
            createxcel();
            label1.Text = " Data  Preprocessing Completed.";
           // button3.Enabled = true;
        }
        public DataTable ReadCsvFile()
        {

            DataTable dtCsv = new DataTable();
            string Fulltext;


            using (StreamReader sr = new StreamReader(Program.fpath1))
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
                                  //  richColumns.AppendText(rowValues[j]);
                                 //   richColumns.AppendText("\n");
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
                                       // richData.AppendText(rowValues[k].ToString());
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
            label2.Text = "Row count: " + count;
            return dtCsv;
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
                        string sa1 = calculatevector(sw);
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
        public string calculatewordtovectorentropy(string w)
        {
            ArrayList ALPHABET_ITEMS_ARRAYLIST = new ArrayList();
            string MESSAGE = w;
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

        private void DataPreprocessing_Load(object sender, EventArgs e)
        {
            fdata1 = Program.filepath1;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
