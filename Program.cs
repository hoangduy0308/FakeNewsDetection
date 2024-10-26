using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;

namespace Regression.Linear
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static string filepath2 = "\\dataset.xls";
        public static string pdata = "";
        public static int a = 20;
        public static int a1 = 45;
        public static double b = 200;
        // public static int t1 = 2169;
        public static double value1 = 0.01475;
        public static double value2 = 0.01982;
        public static double value3 = 0.01357;
        public static double value4 = 0.0;
        public static int t1=0;

        public static string fname = "";
        public static string fileminmax = "";
        public static int p;
        public static double ss=0.06933;
        // public static int t2 = 32;
        public static double t2 = 0.0;
        public static string grapha1 = "";
        public static string grapha2 = "";
        public static string grapha3 = "";
        public static string grapha4 = "";
        //public static int n1 = 43;
        public static double n1 = 0.0;
        public static int count11=2273;

        public static string graphf1 = "";
        public static string graphf2 = "";
        public static string graphf3 = "";
        public static string graphf4 = "";
        public static string fpath1 = "";
        public static string filepath1= "\\Excel\\daaset.xls";
        

        // public static int n2 = 29;
        public static double n2 = 0.0;
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new adminlogin());
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
