using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Models
{
    public class FoodInspectorDatabaseContext : DbContext
    {
        public FoodInspectorDatabaseContext(DbContextOptions<FoodInspectorDatabaseContext> options)
            : base(options)
        {
        }

        public DbSet<FoodInspector.Model.InspectionData> Todo { get; set; }
    }
}
