using AuthJWT_ASPNETCore2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthJWT_ASPNETCore2.Database
{
    public class Context : IdentityDbContext<ApplicationUser>
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        public DbSet<Token> Tokens { get; set; }
    }
}
