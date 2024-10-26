using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using Regression.Linear;


namespace Regression
{
    public partial class FeatureGeneration : Form
    {
        public FeatureGeneration()
        {
            InitializeComponent();
        }
        public static string pdata = Application.StartupPath + "\\tfile.txt";
        public static string sdata = Application.StartupPath + "\\swords.txt";
        ArrayList listDataSource = new ArrayList();
        ArrayList listDataSource1 = new ArrayList();
        ArrayList alist= new ArrayList();
        private void FeatureGeneration_Load(object sender, EventArgs e)
        {
            //txtfilepath.Text = pdata;
            string text1 = File.ReadAllText(pdata);
            richTextBox1.AppendText(text1);



        }
        static Dictionary<string, bool> _stops = new Dictionary<string, bool>
        {
            { "a", true },
            { "about", true },
            { "above", true },
            { "across", true },
            { "after", true },
            { "afterwards", true },
            { "again", true },
            { "against", true },
            { "all", true },
            { "almost", true },
            { "alone", true },
            { "along", true },
            { "already", true },
            { "also", true },
            { "although", true },
            { "always", true },
            { "am", true },
            { "among", true },
            { "amongst", true },
            { "amount", true },
            { "an", true },
            { "and", true },
            { "another", true },
            { "any", true },
            { "anyhow", true },
            { "anyone", true },
            { "anything", true },
            { "anyway", true },
            { "anywhere", true },
            { "are", true },
            { "around", true },
            { "as", true },
            { "at", true },
            { "back", true },
            { "be", true },
            { "became", true },
            { "because", true },
            { "become", true },
            { "becomes", true },
            { "becoming", true },
            { "been", true },
            { "before", true },
            { "beforehand", true },
            { "behind", true },
            { "being", true },
            { "below", true },
            { "beside", true },
            { "besides", true },
            { "between", true },
            { "beyond", true },
            { "bill", true },
            { "both", true },
            { "bottom", true },
            { "but", true },
            { "by", true },
            { "call", true },
            { "can", true },
            { "cannot", true },
            { "cant", true },
            { "co", true },
            { "computer", true },
            { "con", true },
            { "could", true },
            { "couldnt", true },
            { "cry", true },
            { "de", true },
            { "describe", true },
            { "detail", true },
            { "do", true },
            { "done", true },
            { "down", true },
            { "due", true },
            { "during", true },
            { "each", true },
            { "eg", true },
            { "eight", true },
            { "either", true },
            { "eleven", true },
            { "else", true },
            { "elsewhere", true },
            { "empty", true },
            { "enough", true },
            { "etc", true },
            { "even", true },
            { "ever", true },
            { "every", true },
            { "everyone", true },
            { "everything", true },
            { "everywhere", true },
            { "except", true },
            { "few", true },
            { "fifteen", true },
            { "fify", true },
            { "fill", true },
            { "find", true },
            { "fire", true },
            { "first", true },
            { "five", true },
            { "for", true },
            { "former", true },
            { "formerly", true },
            { "forty", true },
            { "found", true },
            { "four", true },
            { "from", true },
            { "front", true },
            { "full", true },
            { "further", true },
            { "get", true },
            { "give", true },
            { "go", true },
            { "had", true },
            { "has", true },
            { "have", true },
            { "he", true },
            { "hence", true },
            { "her", true },
            { "here", true },
            { "hereafter", true },
            { "hereby", true },
            { "herein", true },
            { "hereupon", true },
            { "hers", true },
            { "herself", true },
            { "him", true },
            { "himself", true },
            { "his", true },
            { "how", true },
            { "however", true },
            { "hundred", true },
            { "i", true },
            { "ie", true },
            { "if", true },
            { "in", true },
            { "inc", true },
            { "indeed", true },
            { "interest", true },
            { "into", true },
            { "is", true },
            { "it", true },
            { "its", true },
            { "itself", true },
            { "keep", true },
            { "last", true },
            { "latter", true },
            { "latterly", true },
            { "least", true },
            { "less", true },
            { "ltd", true },
            { "made", true },
            { "many", true },
            { "may", true },
            { "me", true },
            { "meanwhile", true },
            { "might", true },
            { "mill", true },
            { "mine", true },
            { "more", true },
            { "moreover", true },
            { "most", true },
            { "mostly", true },
            { "move", true },
            { "much", true },
            { "must", true },
            { "my", true },
            { "myself", true },
            { "name", true },
            { "namely", true },
            { "neither", true },
            { "never", true },
            { "nevertheless", true },
            { "next", true },
            { "nine", true },
            { "no", true },
            { "nobody", true },
            { "none", true },
            { "nor", true },
            { "not", true },
            { "nothing", true },
            { "now", true },
            { "nowhere", true },
            { "of", true },
            { "off", true },
            { "often", true },
            { "on", true },
            { "once", true },
            { "one", true },
            { "only", true },
            { "onto", true },
            { "or", true },
            { "other", true },
            { "others", true },
            { "otherwise", true },
            { "our", true },
            { "ours", true },
            { "ourselves", true },
            { "out", true },
            { "over", true },
            { "own", true },
            { "part", true },
            { "per", true },
            { "perhaps", true },
            { "please", true },
            { "put", true },
            { "rather", true },
            { "re", true },
            { "same", true },
            { "see", true },
            { "seem", true },
            { "seemed", true },
            { "seeming", true },
            { "seems", true },
            { "serious", true },
            { "several", true },
            { "she", true },
            { "should", true },
            { "show", true },
            { "side", true },
            { "since", true },
            { "sincere", true },
            { "six", true },
            { "sixty", true },
            { "so", true },
            { "some", true },
            { "somehow", true },
            { "someone", true },
            { "something", true },
            { "sometime", true },
            { "sometimes", true },
            { "somewhere", true },
            { "still", true },
            { "such", true },
            { "system", true },
            { "take", true },
            { "ten", true },
            { "than", true },
            { "that", true },
            { "the", true },
            { "their", true },
            { "them", true },
            { "themselves", true },
            { "then", true },
            { "thence", true },
            { "there", true },
            { "thereafter", true },
            { "thereby", true },
            { "therefore", true },
            { "therein", true },
            { "thereupon", true },
            { "these", true },
            { "they", true },
            { "thick", true },
            { "thin", true },
            { "third", true },
            { "this", true },
            { "those", true },
            { "though", true },
            { "three", true },
            { "through", true },
            { "throughout", true },
            { "thru", true },
            { "thus", true },
            { "to", true },
            { "together", true },
            { "too", true },
            { "top", true },
            { "toward", true },
            { "towards", true },
            { "twelve", true },
            { "twenty", true },
            { "two", true },
            { "un", true },
            { "under", true },
            { "until", true },
            { "up", true },
            { "upon", true },
            { "us", true },
            { "very", true },
            { "via", true },
            { "was", true },
            { "we", true },
            { "well", true },
            { "were", true },
            { "what", true },
            { "whatever", true },
            { "when", true },
            { "whence", true },
            { "whenever", true },
            { "where", true },
            { "whereafter", true },
            { "whereas", true },
            { "whereby", true },
            { "wherein", true },
            { "whereupon", true },
            { "wherever", true },
            { "whether", true },
            { "which", true },
            { "while", true },
            { "whither", true },
            { "who", true },
            { "whoever", true },
            { "whole", true },
            { "whom", true },
            { "whose", true },
            { "why", true },
            { "will", true },
            { "with", true },
            { "within", true },
            { "without", true },
            { "would", true },
            { "yet", true },
            { "you", true },
            { "your", true },
            { "yours", true },
            { "yourself", true },
            { "yourselves", true }
    };
        private void button2_Click(object sender, EventArgs e)
        {
            //string path = @"\mobile.txt";
            //string text2 = File.ReadAllText(Application.StartupPath + path);
            //splitdata1(text2);
            label5.Text = "Feature Extration Started....";
            DataTable dt = new DataTable();
            DataGridView dgv = new DataGridView();
            dt = ReadCsvFile();
            dataGridView1.DataSource = dt;
            label5.Text = "Feature Biniarization Completed....";
        }
        public void splitdata1(string tt)
        {
            string column = tt.Replace("\n", "");
            string[] columnnames = Regex.Split(column.Trim(), ";");
            for (int i = 0; i < columnnames.Count(); i++)
            {
                string[] columntext = columnnames[i].Split(':');

                ListViewItem it2 = new ListViewItem(columntext[0]);
                it2.SubItems.Add(columntext[1].ToString());
                it2.SubItems.Add(columntext[2].ToString());
                //listView1.Items.Add(it2);


            }
        }
        static char[] _delimiters = new char[]
        {
            ' ',
            ',',
            ';',
            '.'
        };

        public void filldatavalue(string te)
        {
            listBox2.Items.Clear();
            string[] b = Regex.Split(te, ",");
            for (int i = 0; i < b.Count(); i++)
            {
                listBox2.Items.Add(b[i].ToString());
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //string inFileName = pdata;
            //StreamReader sr = new StreamReader(inFileName);
            //string text = System.IO.File.ReadAllText(inFileName);
            //Regex reg_exp = new Regex("[^a-zA-Z]");
            //text = reg_exp.Replace(text, " ");
            //string[] words = text.Split(new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries);
            //var word_query = (from string word in words orderby word select word).Distinct();
            //string[] result = word_query.ToArray();
            //int counter = 0;
            //string delim = " ,.";
            //string[] fields = null;
            //string line = null;
            //while (!sr.EndOfStream)
            //{
            //    line = sr.ReadLine(); //each time you read a line you should split it into the words  
            //    line.Trim();
            //    fields = line.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    counter += fields.Length; //and just add how many of them there is  
            //    foreach (string word in result)
            //    {
            //        CountStringOccurrences(text, word);
            //    }
            //}
            //sr.Close();
            label5.Text = "Stemming started Completed....";
            String line, word = "";
            int count1 = 0, maxCount = 0;
            ArrayList words = new ArrayList();
            ArrayList wordsa = new ArrayList();
            //Opens file in read mode  
            System.IO.StreamReader file = new System.IO.StreamReader(pdata);

            //Reads each line  
            while ((line = file.ReadLine()) != null)
            {
                String[] string1 = line.ToLower().Split(new Char[] { ',', '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //Adding all words generated in previous step into words  
                foreach (String s in string1)
                {
                    var words1 = s.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);
                    var found = new Dictionary<string, bool>();
                    StringBuilder builder = new StringBuilder();
                    foreach (string currentWord in words1)
                    {
                        // 5
                        // Convert to lowercase
                        string lowerWord = currentWord.ToLower();
                        // 6
                        // If this is a usable word, add it
                        if (!_stops.ContainsKey(lowerWord) &&
                            !found.ContainsKey(lowerWord))
                        {
                            builder.Append(currentWord).Append(' ');
                            found.Add(lowerWord, true);
                        }
                    }
                    string dest2 = builder.ToString().Trim();
                    words.Add(dest2);

                }
            }
            label5.Text = "conversion of the texts completed..";
            foreach (string aString in words)
            {
                if (!alist.Contains(aString))
                {
                    alist.Add(aString);
                }
            }
            //Determine the most repeated word in a file  

            //if(count1 >1)
            //    word = (String)words[i];
            //If maxCount is less than count then store value of count in maxCount   
            //and corresponding word to variable word  
            //if (count1 > maxCount)
            //{
            //    maxCount = count1;
            //    word = (String)words[i];
            //}
            // }
            label5.Text = " characters to lowercase letters completed..";
            for (int i = 0; i < alist.Count; i++)
            {
                listBox2.Items.Add(alist[i]);
            }
           // Console.WriteLine("Most repeated word: " + word);
            file.Close();

            //string path = @"\swords.txt";
            //string text3 = File.ReadAllText(Application.StartupPath + path);
            //filldatavalue(text3);
            label5.Text = " stopwords removal completed..";
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                int count = 0;
                double rating = 0;
                listBox2.SelectedIndex = i;
                string search = listBox2.SelectedItem.ToString();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    try
                    {
                        string dest = dataGridView1.Rows[row.Index].Cells[0].FormattedValue.ToString();
                        var words1 = dest.Split(_delimiters,StringSplitOptions.RemoveEmptyEntries);
                        var found = new Dictionary<string, bool>();
                        StringBuilder builder = new StringBuilder();
                        foreach (string currentWord in words1)
                        {
                            // 5
                            // Convert to lowercase
                            string lowerWord = currentWord.ToLower();
                            // 6
                            // If this is a usable word, add it
                            if (!_stops.ContainsKey(lowerWord) &&
                                !found.ContainsKey(lowerWord))
                            {
                                builder.Append(currentWord).Append(' ');
                                found.Add(lowerWord, true);
                            }
                        }
                        string dest1= builder.ToString().Trim();
                        if (dest1.IndexOf(search) > 0)
                        {
                            count++;
                            rating = rating + Convert.ToDouble(dataGridView1.Rows[row.Index].Cells[1].FormattedValue.ToString());

                        }
                    }
                    catch (System.Exception ex)
                    {

                    }
                }
                double rank = 0;
                if (rating == 0 || count == 0)
                {
                    rank = 0;
                }
                else
                {
                    rank = rating / count;
                    rank = System.Math.Round(rank, 1);
                }


                ListViewItem it2 = new ListViewItem(search);
                //it2.SubItems.Add(rating.ToString());
                it2.SubItems.Add(count.ToString());
                it2.SubItems.Add(rank.ToString());





                lstport.Items.Add(it2);

                listDataSource.Add(search);

                listDataSource1.Add(rank);


            }
            label5.Text = "tokenization completed..Data Ready for processing";
            Color prevBackColor = Color.LightBlue; // Member variable
            string prevHouseNumber = string.Empty;
           
        }
        public void CountStringOccurrences(string text, string word)
        {
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(word, i)) != -1)
            {
                i += word.Length;
                count++;
            }
            if (count == 10)
            {
                listBox2.Items.Add(word);
            }
          //  Console.WriteLine("{0} {1}", count, word);
        }
        public DataTable ReadCsvFile()
        {

            DataTable dtCsv = new DataTable();
            string Fulltext;
            if (Program.fpath1 == "")
            {
                MessageBox.Show("Feature Extraction work only After Data Preprocessing");
            }
            else
            {
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
                                            //richData.AppendText(rowValues[k].ToString());
                                        }
                                    }
                                    dtCsv.Rows.Add(dr); //add other rows  
                                }
                            }
                        }
                    }
                }

            }
            return dtCsv;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
