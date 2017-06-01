using INVTMS.XMLBookingWebService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace INVTMS.XMLBookingWebService.Controllers
{
    public class AuthenController
    {
        private readonly string _connectionString;
        private readonly string _appId;

        public AuthenController()
        {
            _connectionString = ConfigurationManager.AppSettings["AuthenConnection"];
            _appId = ConfigurationManager.AppSettings["AppID"];
        }

        public User Login(string userName, string password)
        {
            string sqlText = @"SELECT * FROM (
            SELECT A.[LoginName],A.[Password],A.[FullName],A.[PAYID], ISNULL(B.[APPLICATIONNAME],'') AS [APPLICATIONNAME],
            A.[Status] FROM [Global_User_Profile] A
            LEFT JOIN (SELECT [USERNAME],[APPLICATIONNAME] FROM [Global_User_Application] WHERE [STATUS]='Y') B
            ON A.[LoginName]=B.[USERNAME]) T
            WHERE T.[LoginName]=@Username
            AND T.[Password]=@Password
            AND T.[APPLICATIONNAME]=@AppId
            AND T.[Status]='Y'";

            DataSet dsResult = new DataSet();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlText;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                cmd.Parameters.AddWithValue("@Username", userName);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@AppId", _appId);

                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dsResult);
            }

            var userObj = new User();
            if (dsResult != null && dsResult.Tables[0].Rows.Count > 0)
            {
                var row = dsResult.Tables[0].Rows[0];
                userObj.Username = userName;
                userObj.Password = "";
                userObj.Fullname = row["FullName"].ToString();
                userObj.Token = TokenCrypt.GenerateToken(userObj.Username, password, DateTime.Now.Ticks);
            }

            return userObj;
        }
    }
}