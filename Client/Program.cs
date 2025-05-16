using Client.Forms;
using Client.Models;
using Client.Communication;

namespace Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm("client");
            Application.Run(mainForm);
        }
    }
}