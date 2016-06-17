using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsApplication1
{
    static class Program
    {
        public static string XMLDocPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "CLBCalcs");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CheckForDataFolder();

            Application.Run(new CLBRPIForm());

            CheckForDataFolder();

        }

        private static void CheckForDataFolder()
        {


            if (!Directory.Exists(XMLDocPath))
            {
                System.IO.Directory.CreateDirectory(XMLDocPath);

                XDocument doc = new XDocument(new XElement("teams"));
                doc.Save(XMLDocPath + "\\RatingsData.xml");

            }
        }
    }
}
