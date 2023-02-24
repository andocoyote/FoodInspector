using CommonFunctionality.Model;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Models
{
    public class FoodInspectorDatabaseContext : DbContext
    {
        public FoodInspectorDatabaseContext(DbContextOptions<FoodInspectorDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<InspectionData> Todo { get; set; }
    }
}
