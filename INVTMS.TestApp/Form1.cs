using INVTMS.BsLogic;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace INVTMS.TestApp
{
    public partial class Form1 : Form
    {
        private InvRefController controller;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            controller = new InvRefController(@"data source=198.1.1.3;uid=ecssystem;password=ecssystem;database=EDIDATA1");

            dataGridView1.DataSource = controller.GetQueue();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var items = (List<InvReference>)dataGridView1.DataSource;

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ShipmentType == "E")
                {
                    var invoices = controller.GetExpInvoice(items[i]);

                    var buffer = controller.ExportToBuffer(invoices);
                    System.IO.File.WriteAllBytes(items[i].InvoiceNo + ".zip", buffer);
                }
            }

            MessageBox.Show("OK");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Get invoice file.

            var m = new CommercialInvoiceController("");
            var inv = m.GetInvoice(@"C:\temp\CTI-TMS\InvoiceShipment\inbox\CPK04165.zip");
            dataGridView1.DataSource = inv;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var inv = (List<CommercialInvoice>)dataGridView1.DataSource;
            var controller = new CommercialInvoiceController(@"data source=.\SQLExpress;uid=sa;password=cti2016;database=DEMO_TMS");

            string errorString = string.Empty;
            var result = controller.Insert(inv, out errorString);

            MessageBox.Show(result.ToString() + "/" + errorString);
        }
    }
}