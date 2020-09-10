using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DBOperator;

namespace SagensVision.VisionTool
{
    public partial class HistoryDataFrm : Form
    {
        public HistoryDataFrm()
        {
            InitializeComponent();
        }
        DBShow dbshow1;
        private void HistoryDataFrm_Load(object sender, EventArgs e)
        {
            dbshow1 = new DBShow();
            this.Controls.Add(dbshow1);
            dbshow1.Dock = DockStyle.Fill;
            dbshow1.ConnectMdb(FormMain.dbpath);
        }

        private void HistoryDataFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            dbshow1.Dispose();
            FormMain.isShowHistoryFrm = false;
        }
    }
}
