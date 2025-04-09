using System;
using System.IO;
using System.Collections.Generic;

namespace Functions_for_Dynamics_Operations
{
    public class LogfileUtility
    {
        public string Filename()
        {
            return $@"C:\Temp\TimeTracker{DateTime.Now.Year}_{DateTime.Now.Month}_{DateTime.Now.Day}.Log";
        }

        public void WriteLog(string log)
        {
            ExistsCreate();

            using (var writer = File.AppendText(Filename()))
            {   // append the line  
                writer.WriteLine(log);
            }
        }

        public IEnumerable<string> ReadLog()
        {
            ExistsCreate();

            return File.ReadLines(Filename());
        }

        protected void ExistsCreate()
        {
            if (!File.Exists(Filename()))
            {
                using (FileStream file = File.Create(Filename()))
                {
                    ;
                }
            }
        }

        public static string LogLine(string model, string solution, string project, string activeWindow, string activeDocument)
        {
            return $"{DateTime.Now}|{model}|{solution}|{project}|{activeWindow}|{activeDocument}";
        }
    }
}
