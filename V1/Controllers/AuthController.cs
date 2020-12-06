using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthJWT_ASPNETCore2.Models;
using AuthJWT_ASPNETCore2.V1.DTO;
using AuthJWT_ASPNETCore2.V1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthJWT_ASPNETCore2.V1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUsuario _usuarioRepository;
        private readonly IToken _tokenRepository;

        public AuthController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUsuario usuarioRepository,
            IToken tokenRepository
        ) {
            _signInManager = signInManager;
            _userManager = userManager;
            _usuarioRepository = usuarioRepository;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult Login([FromBody] UsuarioDTO usuarioDTO)
        {
            ModelState.Remove("Nome");
            ModelState.Remove("ConfirmarSenha");

            if (ModelState.IsValid)
            {
                ApplicationUser usuario = _usuarioRepository.FindByEmailSenha(
                    usuarioDTO.Email,
                    usuarioDTO.Senha
                );

                if (usuario != null)
                {
                    return GerarToken(usuario);
                }
                else
                {
                    return NotFound("Usuário não encontrado na base de dados!");
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPost("cadastrar")]
        [AllowAnonymous]
        public ActionResult Cadastrar([FromBody] UsuarioDTO usuarioDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser usuario = new ApplicationUser();
                usuario.FullName = usuarioDTO.Nome;
                usuario.Email = usuarioDTO.Email;
                usuario.UserName = usuarioDTO.Email;

                var result = _userManager.CreateAsync(usuario, usuarioDTO.Senha).Result;

                if (!result.Succeeded)
                {
                    List<string> erros = new List<string>();
                    foreach (var erro in result.Errors)
                    {
                        erros.Add(erro.Description);
                    }

                    return UnprocessableEntity(erros);
                }
                else
                {
                    return Ok(usuario);
                }
            }
            else
            {
                return UnprocessableEntity(ModelState);
            }
        }

        [HttpPost("renovar-token")]
        public ActionResult RenovarToken([FromBody] TokenDTO tokenDTO)
        {
            var refreshTokenDB = _tokenRepository.FindByRefreshToken(tokenDTO.RefreshToken);

            if (refreshTokenDB == null)
            {
                return NotFound();
            }

            // Atualiza Token
            refreshTokenDB.Atualizado = DateTime.Now;
            refreshTokenDB.Utilizado = true;
            _tokenRepository.Atualizar(refreshTokenDB);

            // Busca usuario do Token
            var usuario = _usuarioRepository.FindById(refreshTokenDB.UsuarioId);

            return GerarToken(usuario);
        }

        private ActionResult GerarToken(ApplicationUser usuario)
        {
            var token = Token(usuario);

            // Salva Token no banco de dados
            var tokenModel = new Token()
            {
                RefreshToken = token.RefreshToken,
                ExpirationToken = token.Expiration,
                ExpirationRefreshToken = token.ExpirationRefreshToken,
                Usuario = usuario,
                Criado = DateTime.Now,
                Utilizado = false
            };
            _tokenRepository.Cadastrar(tokenModel);

            return Ok(token);
        }

        private TokenDTO Token(ApplicationUser usuario)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("web-api-jwt-tasks"));
            var sign = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var exp = DateTime.UtcNow.AddHours(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: exp,
                signingCredentials: sign
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            var expRefreshToken = DateTime.UtcNow.AddHours(2);

            var tokenDTO = new TokenDTO
            {
                Token = tokenString,
                Expiration = exp,
                ExpirationRefreshToken = expRefreshToken,
                RefreshToken = refreshToken
            };

            return tokenDTO;
        }
    }
}
