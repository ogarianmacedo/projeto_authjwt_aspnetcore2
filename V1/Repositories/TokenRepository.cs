using AuthJWT_ASPNETCore2.Database;
using AuthJWT_ASPNETCore2.Models;
using AuthJWT_ASPNETCore2.V1.Interfaces;
using System.Linq;

namespace AuthJWT_ASPNETCore2.V1.Repositories
{
    public class TokenRepository : IToken
    {
        private readonly Context _context;

        public TokenRepository(Context context)
        {
            _context = context;
        }

        public Token FindByRefreshToken(string refreshToken)
        {
            return _context.Tokens.FirstOrDefault(t => 
                t.RefreshToken == refreshToken
                && t.Utilizado == false
            );
        }

        public void Cadastrar(Token token)
        {
            _context.Tokens.Add(token);
            _context.SaveChanges();
        }

        public void Atualizar(Token token)
        {
            _context.Tokens.Update(token);
            _context.SaveChanges();
        }
    }
}
