using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MedicalCamp;

namespace MedicalCamp
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
            Application.Run(new MainForm());
            //Application.Run(new Form3());

            /*http://download.microsoft.com/download/0/5/D/05DCCDB5-57E0-4314-A016-874F228A8FAD/SSCERuntime_x86-ENU.exe
             * SELECT     id, Tabid, BeforeStock, AfterStock, Balance, Month, Year, Expiry, Date
FROM         BalanceDetails
ORDER BY Tabid DESC
             * SELECT     id, TabletName, Description
FROM         Tablets
ORDER BY id DESC
             */
        }

    }
}
