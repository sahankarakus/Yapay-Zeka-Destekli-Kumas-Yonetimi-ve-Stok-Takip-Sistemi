using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class LoadEkran : Form
    {
        public LoadEkran()
        {
            InitializeComponent();
        }

        private void LoadEkran_Load(object sender, EventArgs e)
        {
            timer1.Start();

        }
        int i = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (i != 100)
            {
                i++;
            }
            else 
            {
                this.Close();
                   timer1.Stop();
                }

        }
    }
}
