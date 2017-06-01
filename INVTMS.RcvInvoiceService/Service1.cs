using INVTMS.BsLogic;
using System;
using System.Configuration;
using System.IO;
using System.ServiceProcess;

namespace INVTMS.RcvInvoiceService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);
            timer1.Start();
            Logger.EventLog("Start Service");
        }

        protected override void OnStop()
        {
            timer1.Stop();
            Logger.EventLog("Stop Service");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;

            try
            {
                string connectionString = ConfigurationManager.AppSettings["Connection"];
                string location = ConfigurationManager.AppSettings["Path"];

                var controller = new CommercialInvoiceController(connectionString);
                var files = controller.GetFiles(location);

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];

                    var invoices = controller.GetInvoice(file.FullName);

                    if (invoices == null)
                    {
                        Logger.ErrorLog(file.Name + "no record.");
                        continue;
                    }

                    string errorString = string.Empty;
                    var insResult = controller.Insert(invoices, out errorString);

                    if (insResult)
                    {
                        file.Delete();
                        Logger.EventLog(file.Name + " ==> OK");
                    }
                    else
                    {
                        ErrorMessage(location, file.FullName);
                        Logger.ErrorLog(file.Name + " ==> " + errorString);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex.Message);
            }

            timer1.Enabled = true;
        }

        private void ErrorMessage(string location, string sourceFileName)
        {
            string errorPath = Path.Combine(location, "error");

            if (!Directory.Exists(errorPath))
                Directory.CreateDirectory(errorPath);

            try
            {
                FileInfo fi = new FileInfo(sourceFileName);
                string destinationFileName = Path.Combine(errorPath, fi.Name);

                File.Move(sourceFileName, destinationFileName);
            }
            catch { }
        }
    }
}