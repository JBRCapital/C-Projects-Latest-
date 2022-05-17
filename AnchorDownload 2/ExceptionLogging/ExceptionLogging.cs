using System;
using System.Diagnostics;
using System.IO;

namespace ExceptionLogging
{
    public static class ExceptionLogging
    {
        public static void Write(Exception exception)
        {
            try
            {
                string logfile = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".log";

                if (File.Exists(logfile))
                {
                    using (var writer = new StreamWriter(logfile, true))
                    {
                        writer.WriteLine(
                            "=>{0} An Error occurred: {1}  Message: {2}{3}",
                            DateTime.Now,
                            exception.StackTrace,
                            exception.Message,
                            Environment.NewLine
                            );
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
