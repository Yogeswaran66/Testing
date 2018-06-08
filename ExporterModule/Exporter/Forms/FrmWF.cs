using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JSONXLib;
using API;
using System.IO;
using exhelper;
using Support;

namespace Exporter
{
    public partial class FrmWF : Form, IFLoadTestReporter
    {
        public Boolean mStop;
        Int32 Count = 0;

        public FrmWF()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void RunAgent()
        {
            DoExport objDoExport = new DoExport(this);
            
            objDoExport.InitialiseAllApiObj();
            objDoExport.DoAgentOps();
        }

        public void SetCurrentItem(String Appdata1)
        {
            lblAppdata1.Text = Appdata1;
        }

        public void SetCount()
        {            
            Count++;
            lblTotalCount.Text = Count.ToString();            
        }

        public void PreparetoProcessItem()
        {
            lblAppdata1.Text = "";
        }

        public void CompletedItem()
        {
            this.Refresh();         
        }

        public Boolean CanContinueProcessing()
        {
            Application.DoEvents();
            return (mStop == false);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            mStop = true;
            AppSupport.objLog.WriteLog("Agent", "Stopped");
        }

        private void FrmWF_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }        
    }
}
