using System;
using System.IO;
using System.Linq;

namespace FilesHelper
{
    public class Helpers
    {
        public static void CleanOldFiles(string folder, string extension)
        {
            Directory.GetFiles(folder.Replace(@"\\", @"\")).Select(f => new FileInfo(f))
                .Where(fi => fi.CreationTime < DateTime.Now.AddDays(-1) && fi.Extension.ToLower() == extension).ToList().ForEach(f => f.Delete());
        }
    }
}
