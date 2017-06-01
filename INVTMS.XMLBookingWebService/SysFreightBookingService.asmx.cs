using INVTMS.XMLBookingWebService.Controllers;
using Newtonsoft.Json;
using System;
using System.Web.Script.Services;
using System.Web.Services;

namespace INVTMS.XMLBookingWebService
{
    /// <summary>
    /// Summary description for SysFreightBookingService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    [System.Web.Script.Services.ScriptService]
    public class SysFreightBookingService : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void GetShipment(string token, string invoiceNo)
        {
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";

            if (string.IsNullOrEmpty(token))
                throw new Exception("Require token key.");
        }
    }
}