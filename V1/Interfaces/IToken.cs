using AuthJWT_ASPNETCore2.Models;

namespace AuthJWT_ASPNETCore2.V1.Interfaces
{
    public interface IToken
    {
        Token FindByRefreshToken(string refreshToken);

        void Cadastrar(Token token);

        void Atualizar(Token token);
    }
}
