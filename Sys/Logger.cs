using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RacingGame.Sys
{
    /// <summary>
    /// Singleton used to handle logs
    /// Author: David Sundelius
    /// </summary>
    class Logger
    {
        private static Logger instance = new Logger();

        private Logger()
        {
        }

        private string getPath()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        public static Logger getInstance()
        {
            return instance;
        }

        public void print(string message)
        {
            System.IO.StreamWriter file = System.IO.File.AppendText(getPath() + "/log.txt");
            try
            {
                string logLine = System.String.Format("{0:G}: {1}", System.DateTime.Now, message);
                file.WriteLine(logLine);
            }
            finally
            {
                file.Close();
            }
        }

        public void clear()
        {
            try
            {
                System.IO.File.Delete(getPath()+"/log.txt");
            }
            catch(Exception)
            {
            }
        }
    }
}
