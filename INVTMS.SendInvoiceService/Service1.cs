using INVTMS.BsLogic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace INVTMS.SendInvoiceService
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
            Logger.EventLog("Start service");
        }

        protected override void OnStop()
        {
            timer1.Stop();
            Logger.EventLog("Stop service");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;

            try
            {
                string connectionString = ConfigurationManager.AppSettings["Connection"];
                string path = ConfigurationManager.AppSettings["Path"];
                ExistsPath(path);

                var controller = new InvRefController(connectionString);
                var invoices = controller.GetQueue();

                for (int i = 0; i < invoices.Count; i++)
                {
                    InvReference invoice = invoices[i];
                    List<CommercialInvoice> commercialInvoices = null;

                    Logger.EventLog(invoice.ToString());

                    if (invoice.ShipmentType == "E")
                        commercialInvoices = controller.GetExpInvoice(invoice);
                    else
                        commercialInvoices = controller.GetImpInvoice(invoice);

                    if (commercialInvoices != null)
                    {
                        // Export to zip file.
                        var buffer = controller.ExportToBuffer(commercialInvoices);
                        string destFileName = OutputFileName(path, invoice.InvoiceNo);
                        File.WriteAllBytes(destFileName, buffer);
                        Logger.EventLog(string.Format("{0} {1} ==> OK", invoice.ToString(), destFileName));

                        // Delete invoice reference.
                        controller.Delete(invoice);
                    }
                } //End loop.
            }
            catch (Exception ex)
            {
                Logger.ErrorLog(ex.Message);
            }
            timer1.Enabled = true;
        }

        private void ExistsPath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private string OutputFileName(string path, string invoiceNo)
        {
            return Path.Combine(path, RemoveSpecialChars(invoiceNo) + ".zip");
        }

        private string RemoveSpecialChars(string input)
        {
            return Regex.Replace(input, @"[\./]", string.Empty);
        }
    }
}