namespace CommonFunctionality.StorageAccount
{
    public class StorageAccountOptions
    {
        public StorageAccountOptions()
        {
            StorageAccountKey = string.Empty;
        }

        /// <summary>
        /// The access key for the stfoodinspector Azure Storage Account.
        /// </summary>
        public string StorageAccountKey { get; set; }
    }
}
