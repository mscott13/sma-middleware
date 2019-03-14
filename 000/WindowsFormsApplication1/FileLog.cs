using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SyncMon
{
    public static class FileLog
    {
        private static string file_name = "middleware_log.txt";
        private static string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private static string path = directory + "\\" + file_name;

        public static void Write(string msg)
        {
            var str_builder = new StringBuilder();
            string result = str_builder.Append(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": ")
                            .Append(msg)
                            .AppendLine()
                            .AppendLine()
                            .ToString();

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                File.AppendAllText(path, result);
            }
            else
            {
                File.AppendAllText(path, result);
            }
        }
    }
}
