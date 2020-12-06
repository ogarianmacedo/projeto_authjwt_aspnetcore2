using AuthJWT_ASPNETCore2.Models;
using AuthJWT_ASPNETCore2.V1.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Text;

namespace AuthJWT_ASPNETCore2.V1.Repositories
{
    public class UsuarioRepository : IUsuario
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsuarioRepository(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser FindById(string id)
        {
            return _userManager.FindByIdAsync(id).Result;
        }

        public ApplicationUser FindByEmailSenha(string email, string senha)
        {
            var usuario = _userManager.FindByEmailAsync(email).Result;
            if (_userManager.CheckPasswordAsync(usuario, senha).Result)
            {
                return usuario;
            }
            else
            {
                throw new Exception("Usuário não encontrado na base de dados!");
            }
        }

        public void Cadastrar(ApplicationUser usuario, string senha)
        {
            var result = _userManager.CreateAsync(usuario, senha).Result;

            if (!result.Succeeded)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var erro in result.Errors)
                {
                    sb.Append(erro.Description);
                }

                throw new Exception($"Erro ao cadastrar usuário:  { sb.ToString() }");
            }
        }
    }
}
