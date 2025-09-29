using AuthApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Data
{
    public class AuthAPIContext: DbContext
    {
        public AuthAPIContext(DbContextOptions<AuthAPIContext> options): base(options){}

        public DbSet<User> Users { get; set; } = null!;
    }
}
