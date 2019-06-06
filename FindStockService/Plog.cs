using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FindStockService
{
    class Plog
    {
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Config                                                          |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private static string LogName = ConfigurationManager.AppSettings["LogName"];
        public static string nameFile = $@"\{LogName}-{DateTime.Now.ToString("yyyyMMdd")}.log";
        public static string strPath = AppDomain.CurrentDomain.BaseDirectory + @"\logs";
        public static string fullPath = strPath + nameFile;
        public static bool writer = false;
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Constructor                                                          |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public Plog(bool input = false)
        {
            writer = input;
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Main Function                                                   |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        public void LOGI(string input, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            SetTimeFile();
            string result = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            result += $" [INFO] [{caller}@{lineNumber}] ";
            result += input;
            File.AppendAllLines(fullPath, new[] { result });
            if (writer)
                Console.WriteLine(result);
        }
        public void LOGE(string input, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            SetTimeFile();
            string result = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            result += $" [ERROR] [{caller}@{lineNumber}] ";
            result += input;
            File.AppendAllLines(fullPath, new[] { result });
            if (writer)
                Console.WriteLine(result);
        }
        public void LOGW(string input, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            SetTimeFile();
            string result = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            result += $" [WARN] [{caller}@{lineNumber}] ";
            result += input;
            File.AppendAllLines(fullPath, new[] { result });
            if (writer)
                Console.WriteLine(result);
        }
        public void LOGD(string input, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            SetTimeFile();
            string result = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            result += $" [DEBUG] [{caller}@{lineNumber}] ";
            result += input;
            File.AppendAllLines(fullPath, new[] { result });
            if (writer)
                Console.WriteLine(result);
        }
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        // | Other Function                                                   |
        // = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = = =
        private void SetTimeFile()
        {
            nameFile = $@"\{LogName}-{DateTime.Now.ToString("yyyyMMdd")}.log";
            strPath = AppDomain.CurrentDomain.BaseDirectory + @"\logs";
            fullPath = strPath + nameFile;
            // Create folder logs
            bool exists = System.IO.Directory.Exists(strPath);
            if (!exists)
                System.IO.Directory.CreateDirectory(strPath);
        }
    }
}
