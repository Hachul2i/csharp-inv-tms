using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace INVTMS.XMLBookingWebService.Controllers
{
    public class TokenCrypt
    {
        private const string Alg = "HmacSHA256";
        private const string Salt = "FriywtpQxaPsX8NyaYhJ"; // Generated at https://www.random.org/strings

        private static int _expirationMinutes = 10;
        private static string _connectionString = string.Empty;
        private static string _applicationId = string.Empty;

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public static void SetApplicationId(string appId)
        {
            _applicationId = appId;
        }

        public static void SetExpirationTime(int expirationMinutes)
        {
            _expirationMinutes = expirationMinutes;
        }

        public static string GenerateToken(string username, string password, long ticks)
        {
            var bufferUsername = System.Text.Encoding.UTF8.GetBytes(username);
            var encodeUsername = Convert.ToBase64String(bufferUsername);

            var hash = string.Join(":", encodeUsername, ticks.ToString());
            var hashLeft = "";
            var hashRight = "";

            using (var hmac = HMAC.Create(Alg))
            {
                hmac.Key = Encoding.UTF8.GetBytes(GetHashedPassword(password));
                hmac.ComputeHash(Encoding.UTF8.GetBytes(hash));

                hashLeft = Convert.ToBase64String(hmac.Hash);
                hashRight = string.Join(":", encodeUsername, ticks.ToString());
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(":", hashLeft, hashRight)));
        }

        private static string GetHashedPassword(string password)
        {
            var key = string.Join(":", password, Salt);

            using (var hmac = HMAC.Create(Alg))
            {
                // Hash the key.
                hmac.Key = Encoding.UTF8.GetBytes(Salt);
                hmac.ComputeHash(Encoding.UTF8.GetBytes(key));

                return Convert.ToBase64String(hmac.Hash);
            }
        }

        public static bool IsTokenValid(string token)
        {
            var result = false;

            try
            {
                // Base64 decode the string, obtaining the token:username:timeStamp.
                var key = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                // Split the parts.
                var parts = key.Split(':');
                if (parts.Length == 3)
                {
                    // Get the hash message, username, and timestamp.
                    var hash = parts[0];
                    var username = parts[1];
                    var ticks = long.Parse(parts[2]);
                    var timeStamp = new DateTime(ticks);

                    // Update 18.01.2016
                    // Life time session.

                    var bufferUsername = Convert.FromBase64String(username);
                    var decodeUsername = System.Text.Encoding.UTF8.GetString(bufferUsername);

                    var password = string.Empty;
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();

                        var cmd = conn.CreateCommand();
                        cmd.CommandText = @"select top 1 RTRIM(LTRIM(a.[password])) from Global_User_Profile a, Global_User_Application b
                                            where a.loginname=@username
                                            and b.applicationname=@appid
                                            and b.status='Y'
                                            and a.[status]='Y'";

                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@username", decodeUsername);
                        cmd.Parameters.AddWithValue("@appid", _applicationId);

                        var reader = cmd.ExecuteScalar();

                        if (reader != null)
                            password = reader.ToString();
                    }

                    if (!string.IsNullOrEmpty(password))
                    {
                        // Hash the message with the key to generate a token.
                        var computedToken = GenerateToken(decodeUsername, password.ToLower(), ticks);

                        // Compare the computed token with the one supplied and ensure they match.
                        result = (token == computedToken);
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }
    }
}