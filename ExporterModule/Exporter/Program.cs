using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using exhelper;

namespace Exporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Boolean Ret;
            Application.EnableVisualStyles();

            Ret = AppSupport.Initialize(args);
            if (!Ret)
                return;

            FrmWF objWF= new FrmWF();
            objWF.Show();
            objWF.RunAgent();
        }
    }
}
