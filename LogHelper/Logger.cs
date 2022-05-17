using System;
using System.Linq;
using System.Text;

namespace LogHelper
{
    public static class Logger
    {
        public static void WriteOutput(string output, StringBuilder stringBuilder)
        {
            Console.WriteLine(output);
            if (stringBuilder != null) { stringBuilder.AppendLine(output); }
        }

        public static string GetExceptionDetails(Exception exception)
        {
            var properties = exception.GetType()
                                    .GetProperties();
            var fields = properties
                             .Select(property => new {
                                 Name = property.Name,
                                 Value = property.GetValue(exception, null)
                             })
                             .Select(x => String.Format(
                                 "{0} = {1}",
                                 x.Name,
                                 x.Value != null ? x.Value.ToString() : String.Empty
                             ));
            return String.Join("\n", fields);
        }
    }
}
