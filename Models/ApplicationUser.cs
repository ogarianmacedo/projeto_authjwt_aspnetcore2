using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthJWT_ASPNETCore2.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual ICollection<Token> Tokens { get; set; }
    }
}
