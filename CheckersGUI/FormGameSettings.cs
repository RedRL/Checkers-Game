using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckersGUI
{
    public partial class FormGameSettings : Form
    {
        private eBoardSize m_BoardSize;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitializeComponent();
            this.buttonDone.Click += new EventHandler(buttonDone_Click);
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        internal string Player1Name
        {
            get { return textBoxPlayer1.Text; }
        }

        internal string Player2Name
        {
            get { return checkBoxPlayer2.Checked ? textBoxPlayer2.Text : "Computer"; }
        }

        internal bool gameAgainstComputer
        {
            get { return !checkBoxPlayer2.Checked; }
        }

        internal void PlaceInCenterScreen()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        internal eBoardSize BoardSize
        {
            get { return m_BoardSize; }
        }

        private void radioButton6x6_CheckedChanged(object sender, EventArgs e)
        {
            m_BoardSize = eBoardSize.SixOnSix;
        }

        private void radioButton8x8_CheckedChanged(object sender, EventArgs e)
        {
            m_BoardSize = eBoardSize.EightOnEight;
        }

        private void radioButton10x10_CheckedChanged(object sender, EventArgs e)
        {
            m_BoardSize = eBoardSize.TenOnTen;
        }

        private void checkBoxPlayer2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPlayer2.Checked)
            {
                textBoxPlayer2.Enabled = true;
                textBoxPlayer2.Text = "";
            }
            else
            {
                textBoxPlayer2.Enabled = false;
                textBoxPlayer2.Text = "[Computer]";
            }
        }

        private void FormGameSettings_Load(object sender, EventArgs e)
        {

        }
    }
}
