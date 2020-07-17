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

        private void runButton_Click(object sender, EventArgs e)
        {
            var secretsOfGrindea = Process.GetProcessesByName("Secrets of Grindea").First();

            if(secretsOfGrindea == null)
                return;

            var injector = new Injection(secretsOfGrindea.Handle);


            injector.Inject("modloader.dll"); 
            
        }


    }
}
