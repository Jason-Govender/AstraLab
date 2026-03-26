using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace AstraLab.EntityFrameworkCore
{
    public static class AstraLabDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<AstraLabDbContext> builder, string connectionString)
        {
            builder.UseSqlServer(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<AstraLabDbContext> builder, DbConnection connection)
        {
            builder.UseSqlServer(connection);
        }
    }
}
