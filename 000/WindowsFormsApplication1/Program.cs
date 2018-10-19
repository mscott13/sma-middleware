using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyncMon
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool result;
            using (var mutex = new System.Threading.Mutex(true, "1000001100101000001", out result))
            {
                if (!result)
                {
                    MessageBox.Show("An instance is already running", "SASM");
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Monitor_());
            }
           
        }
    }
}
