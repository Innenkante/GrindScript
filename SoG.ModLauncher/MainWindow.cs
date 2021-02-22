using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework.Forms;

namespace SoG.ModLauncher
{
    public partial class MainWindow : MetroForm
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void runButton_Click_OLD(object sender, EventArgs e)
        {
            // Old function
            var secretsOfGrindea = Process.GetProcessesByName("Secrets of Grindea").First();

            if(secretsOfGrindea == null)
                return;

            var injector = new Injection(secretsOfGrindea.Handle);


            injector.Inject("ModLoader.dll"); 
            
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            var existingProcesses = Process.GetProcessesByName("Secrets of Grindea");

            if (existingProcesses.Count() > 0)
            {
                MessageBox.Show("There's more than one Secrets of Grindea instance!\n Keep in mind only game instances started through the launcher will be patched.", "Multiple Instances", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            var secretsOfGrindea = Process.Start("Secrets Of Grindea.exe");

            Thread.Sleep(1000);

            var injector = new Injection(secretsOfGrindea.Handle);

            injector.Inject("ModLoader.dll");

        }

    }
}
