using System;
using System.Windows.Forms;

namespace CampusFacilitiesIssueRegister
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                IssueRepository.InitializeDatabase();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("The database could not be started. Confirm SQL Server Express LocalDB is installed.\n\n" + ex.Message,
                    "Database startup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
