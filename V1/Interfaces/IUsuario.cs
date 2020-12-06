using AuthJWT_ASPNETCore2.Models;

namespace AuthJWT_ASPNETCore2.V1.Interfaces
{
    public interface IUsuario
    {
        ApplicationUser FindById(string id);

        ApplicationUser FindByEmailSenha(string email, string senha);

        void Cadastrar(ApplicationUser usuario, string senha);
    }
}
