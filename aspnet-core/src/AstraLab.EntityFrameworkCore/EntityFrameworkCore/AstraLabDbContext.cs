using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using AstraLab.Authorization.Roles;
using AstraLab.Authorization.Users;
using AstraLab.MultiTenancy;

namespace AstraLab.EntityFrameworkCore
{
    public class AstraLabDbContext : AbpZeroDbContext<Tenant, Role, User, AstraLabDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public AstraLabDbContext(DbContextOptions<AstraLabDbContext> options)
            : base(options)
        {
        }
    }
}
