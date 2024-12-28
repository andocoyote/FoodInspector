namespace FoodInspector.Providers.EmailMessageProvider
{
    public class EmailMessageOptions
    {
        public EmailMessageOptions()
        {
            SenderAccount = string.Empty;
            ReceiverAccount = string.Empty;
        }

        public string SenderAccount { get; set; }

        public string ReceiverAccount { get; set; }
    }
}
