using INVTMS.XMLBookingWebService.Controllers;
using Newtonsoft.Json;
using System;
using System.Web.Script.Services;
using System.Web.Services;

namespace INVTMS.XMLBookingWebService
{
    /// <summary>
    /// Summary description for AuthenticationService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    // [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    [System.Web.Script.Services.ScriptService]
    public class AuthenticationService : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public void Login(string userName, string password)
        {
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                throw new Exception("500 : Required username and password.");
            }

            var controller = new AuthenController();
            var user = controller.Login(userName, password);
            Context.Response.Write(JsonConvert.SerializeObject(user));
        }
    }
}