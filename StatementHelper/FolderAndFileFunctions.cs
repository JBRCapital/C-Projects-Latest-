using System.IO;

namespace AutoDocHelper
{
    public class FolderAndFileFunctions
    {
        public static void DeleteOutputFolderContents(string outputPath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(outputPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
    }
}
