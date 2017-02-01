using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BackupGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Reload();
            if (args != null)
            {
                Application.Run(new Form1(args));
            }
            else
                Application.Run(new Form1());
            Properties.Settings.Default.Save();
        }
    }
}