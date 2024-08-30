using EfCoreUnitOfWork.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreUnitOfWork.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<PersonEntity> Persons { get; set; }
        public DbSet<NotificationEntity> Notifications { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
