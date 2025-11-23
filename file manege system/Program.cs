using System;
using System.Windows.Forms;

namespace file_manege_system
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // We just run Form1. 
            // Form1 now handles its own events, variables, and logic.
            Application.Run(new Form1());
        }
    }
}