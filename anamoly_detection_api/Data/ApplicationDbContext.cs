using anamoly_detection_api.Models;
using Microsoft.EntityFrameworkCore;

namespace anamoly_detection_api.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
