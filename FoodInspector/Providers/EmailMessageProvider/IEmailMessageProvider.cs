namespace FoodInspector.Providers.EmailMessageProvider
{
    public interface IEmailMessageProvider
    {
        Task SendEmailAsync(string messageBody);
    }
}