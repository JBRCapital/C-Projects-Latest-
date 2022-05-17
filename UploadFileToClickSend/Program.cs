using ClicksendHelper;


namespace UploadFileToClickSend
{
    class Program
    {
        static void Main(string[] args)
        {
            ClickSendUploadFile.UploadFileToSendClick(new ClickSendCredentials("JBR", "Hazard10"), string.Concat(@"C:\JBR\sample.pdf"), out string url);
        }
    }
}
