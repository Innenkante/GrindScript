using System;
using System.Diagnostics;
using MetroFramework.Forms;

namespace SoG.ModLauncher
{
    public partial class MainWindow : MetroForm
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            Process.Start("GrindScriptLauncher.exe");
        }
    }
}
