namespace CommonFunctionality.AppToken
{
    public class AppTokenOptions
    {
        public AppTokenOptions()
        {
            KingCountyAppToken = string.Empty;
        }

        /// <summary>
        /// The app token for King Country food establishment inspection data.
        /// </summary>
        public string KingCountyAppToken { get; set; }
    }
}
