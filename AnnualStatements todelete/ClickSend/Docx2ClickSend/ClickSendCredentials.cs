namespace Docx2ClickSend
{
    public class ClickSendCredentials
    {
        public ClickSendCredentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
