using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.EntityFrameworkCore
{
    public static class AstraLabDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AstraLabDbContext> builder, string connectionString)
        {
            builder.UseNpgsql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<AstraLabDbContext> builder, DbConnection connection)
        {
            builder.UseNpgsql(connection);
        }
    }
}
