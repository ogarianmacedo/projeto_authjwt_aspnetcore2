using System.ComponentModel.DataAnnotations;

namespace AuthJWT_ASPNETCore2.V1.DTO
{
    public class UsuarioDTO
    {
        [Required(ErrorMessage = "Campo {0} é obrigatório!")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Campo {0} é obrigatório!")]
        [EmailAddress(ErrorMessage = "Insira um e-mail válido!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Campo {0} é obrigatório!")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Campo {0} é obrigatório!")]
        [Compare("Senha", ErrorMessage = "Senha e confirmação de senha estão diferentes!")]
        public string ConfirmarSenha { get; set; }
    }
}
