namespace CallCreditWrapper
{
    /// <summary>
    /// encapsulate the api result, using a boolean -> Succeeded,
    /// string for the error at failure
    /// or a string for the credit score at success
    /// </summary>
    public class CallCreditResult
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public string creditScoreCategory { get; set; }
        public string creditScoreText { get; set; }
        public string creditScoreExplanation { get; set; }
}
}