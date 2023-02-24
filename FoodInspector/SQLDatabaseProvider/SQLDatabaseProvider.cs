using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace FoodInspector.SQLDatabaseProvider
{
    public class SQLDatabaseProvider : ISQLDatabaseProvider
    {
        public string ConnectionString { get; set; } = string.Empty;

        private SqlConnection _SQLConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SQLDatabaseProvider(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<SQLDatabaseProvider>();

            // Running stored procedure:
            // https://stackoverflow.com/questions/1260952/how-to-execute-a-stored-procedure-within-c-sharp-program

            // Authentication to SQL server
            // https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme?view=azure-dotnet

            // Authentication to SQL server view Managed Identity
            // https://blog.novanet.no/passwordless-connectionstring-to-azure-sql-database-using-managed-identity/

            string connectionstring = _configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            this.ConnectionString = connectionstring;

            _logger.LogInformation($"SQLDatabaseProvider connection string: {connectionstring}");

            //_SQLConnection = new SqlConnection(connectionstring);

            //var credential = new Azure.Identity.DefaultAzureCredential();
            //var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" }));
            //_SQLConnection.AccessToken = token.Token;
        }

        public void WriteRecord(CommonFunctionality.Model.InspectionData inspectionData)
        {
            string connectionstring = _configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
            _logger.LogInformation($"[WriteRecord] Connection string: {connectionstring}");
            _logger.LogInformation($"[WriteRecord] Database name: {_SQLConnection.Database}");

            _SQLConnection.Open();

            // 1.  create a command object identifying the stored procedure
            SqlCommand cmd = new SqlCommand("Insert_InspectionData", _SQLConnection);

            // 2. set the command object so it knows to execute a stored procedure
            cmd.CommandType = CommandType.StoredProcedure;

            // 3. add parameter to command, which will be passed to the stored procedure
            cmd.Parameters.Add(new SqlParameter("@Name", inspectionData.Name));
            cmd.Parameters.Add(new SqlParameter("@Program_Identifier", inspectionData.Program_Identifier));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Date", inspectionData.Inspection_Date));
            cmd.Parameters.Add(new SqlParameter("@Description", inspectionData.Description));
            cmd.Parameters.Add(new SqlParameter("@Address", inspectionData.Address));
            cmd.Parameters.Add(new SqlParameter("@City", inspectionData.City));
            cmd.Parameters.Add(new SqlParameter("@Zip_Code", inspectionData.Zip_Code));
            cmd.Parameters.Add(new SqlParameter("@Phone", inspectionData.Phone));
            cmd.Parameters.Add(new SqlParameter("@Longitude", inspectionData.Longitude));
            cmd.Parameters.Add(new SqlParameter("@Latitude", inspectionData.Latitude));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Business_Name", inspectionData.Inspection_Business_Name));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Type", inspectionData.Inspection_Type));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Score", inspectionData.Inspection_Score));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Result", inspectionData.Inspection_Result));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Closed_Business", inspectionData.Inspection_Closed_Business));
            cmd.Parameters.Add(new SqlParameter("@Violation_Points", inspectionData.Violation_Points));
            cmd.Parameters.Add(new SqlParameter("@Business_Id", inspectionData.Business_Id));
            cmd.Parameters.Add(new SqlParameter("@Inspection_Serial_Num", inspectionData.Inspection_Serial_Num));
            cmd.Parameters.Add(new SqlParameter("@Grade", inspectionData?.Grade ?? "test"));

            // execute the command
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                // iterate through results, printing each to console
                while (rdr.Read())
                {
                    Console.WriteLine("Product: {0,-35} Total: {1,2}", rdr["ProductName"], rdr["Total"]);
                }
            }

            _SQLConnection.Close();
        }
    }
}
