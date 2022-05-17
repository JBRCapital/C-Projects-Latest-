namespace ClicksendHelper
{
    public class ClickSendCredentials
    {
        public ClickSendCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
