namespace CallCreditWrapper
{
    /// <summary>
    /// this object encapsulate the phone number extension and 
    /// actual number, yet the api works just fine with just
    /// adding both up and assigning them to the No
    /// </summary>
    public class CallCreditPhoneNumber
    {
        public string Std { get; set; }
        public string No { get; set; }
    }
}