using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace DocxFromTemplateGenerator
{
    /// <summary>
    /// Helper class with extension methods used to make the code much cleaner and more organized
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Used to create a json property using a key, which populate the value from a dictionary
        /// or uses the Default value you provide if it couldn't find a value with that key in the dictionary
        /// IMPORTANT NOTE: the function uses the defaultvalue type to parse the value it finds at the dictionary
        /// </summary>
        /// <typeparam name="T">supports int and string</typeparam>
        /// <param name="ClickSendElements">a dictionary with the values you trying to convert to jproperty</param>
        /// <param name="Key">the key of the property</param>
        /// <param name="DefaultValue">the default value for the property if not found</param>
        /// <returns></returns>
        public static JProperty GetElementOrDefaultValue<T>(this Dictionary<string, string> ClickSendElements, string Key, T DefaultValue)
        {
            if (ClickSendElements.ContainsKey(Key))
            {
                var value = ClickSendElements[Key];
                if (DefaultValue is string)
                    return new JProperty(Key, value);
                else if (DefaultValue is int && int.TryParse(value, out int NumericValue))
                    return new JProperty(Key, NumericValue);
                else
                    return new JProperty(Key, value);
            }
            else
                return new JProperty(Key, DefaultValue);
        }



        public static void CleanOldFiles(string folder, string extension)
        {
            System.IO.Directory.GetFiles(folder.Replace(@"\\", @"\")).Select(f => new FileInfo(f))
                .Where(fi => fi.CreationTime < DateTime.Now.AddDays(-1) && fi.Extension.ToLower() == extension).ToList().ForEach(f => f.Delete());
        }
    }
}
