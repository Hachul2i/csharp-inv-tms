using System;
using System.Globalization;
using System.IO;

namespace INVTMS.RcvInvoiceService
{
    public class Logger
    {
        static public void EventLog(string message)
        {
            string _destinationFile = string.Format(@"{0}\{1}_event.log", LogPath, DateTime.Now.ToString("yyyyMMdd", cultureInfo));
            Write(message, _destinationFile);
        }

        static public void EventLog(params string[] message)
        {
            string _destinationFile = string.Format(@"{0}\{1}_event.log", LogPath, DateTime.Now.ToString("yyyyMMdd", cultureInfo));

            for (int i = 0; i < message.Length; i++)
            {
                Write(message[i], _destinationFile);
            }
        }

        static public void ErrorLog(string message)
        {
            string _destinationFile = string.Format(@"{0}\{1}_error.log", LogPath, DateTime.Now.ToString("yyyyMMdd", cultureInfo));
            Write(message, _destinationFile);
        }

        static public void ErrorLog(params string[] message)
        {
            string _destinationFile = string.Format(@"{0}\{1}_error.log", LogPath, DateTime.Now.ToString("yyyyMMdd", cultureInfo));

            for (int i = 0; i < message.Length; i++)
            {
                Write(message[i], _destinationFile);
            }
        }

        static private void Write(string message, string destinationFile)
        {
            using (StreamWriter sw = new StreamWriter(new FileStream(destinationFile, FileMode.Append)))
            {
                try
                {
                    sw.WriteLine("{0} {1}", DateTime.Now.ToString(), message);
                }
                catch { }
            }
        }

        static private string LogPath
        {
            get
            {
                string _logPath = Path.Combine(AssemblyPath, "log");

                if (!Directory.Exists(_logPath))
                    Directory.CreateDirectory(_logPath);

                return _logPath;
            }
        }

        static private CultureInfo cultureInfo
        {
            get
            {
                return new CultureInfo("en-US");
            }
        }

        static public string AssemblyPath
        {
            get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); }
        }
    }
}