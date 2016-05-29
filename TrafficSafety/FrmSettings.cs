using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TrafficSafety.Model;

namespace TrafficSafety
{
    public partial class FrmSettings : Form
    {
        public FrmSettings()
        {
            InitializeComponent();
        }

        private void FrmSettings_Load(object sender, EventArgs e)
        {
            txt_flowRatio.Text = GlobalConst.FLOW_RATIO.ToString();
            txt_straightRatio.Text = GlobalConst.STRAIGHT_RATIO.ToString();
            txt_turningRatio.Text = GlobalConst.TURNING_RATIO.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GlobalConst.FLOW_RATIO = Convert.ToDouble(txt_flowRatio.Text);
            GlobalConst.STRAIGHT_RATIO = Convert.ToDouble(txt_straightRatio.Text);
            GlobalConst.TURNING_RATIO = Convert.ToDouble(txt_turningRatio.Text);
            this.Close();
        }
    }
}
