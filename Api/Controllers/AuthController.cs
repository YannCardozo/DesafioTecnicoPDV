using General.DTO.Auth;
using InfraData.Context;
using InfraData.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuraciones;


        public AuthController(UserManager<Usuario> userManager,
                                     SignInManager<Usuario> signInManager, IConfiguration configuraciones)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuraciones = configuraciones;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO Model)
        {
            try
            {
                Usuario usuariolocalizado = null;

                //Model.Senha = "Chaons26196460!@";
                //Model.CPF = "13457498725";

                if (!string.IsNullOrWhiteSpace(Model.CPF) && !Model.CPF.Contains("@"))
                {
                    usuariolocalizado = await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == Model.CPF);
                }
                else if (!string.IsNullOrWhiteSpace(Model.Email) && Model.Email.Contains("@"))
                {
                    usuariolocalizado = await _userManager.FindByEmailAsync(Model.Email);
                }


                if (usuariolocalizado == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                var result = await _signInManager.PasswordSignInAsync(
                    usuariolocalizado.UserName,
                    Model.Senha,
                    isPersistent: false,
                    lockoutOnFailure: false);


                if (result.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(usuariolocalizado);


                    var authClaims = new List<Claim>
                    {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim("Usuario", usuariolocalizado.UserName),
                        new Claim("CPF", usuariolocalizado.CPF),
                        new Claim("Perfil", roles.First())
                    };


                    var token = new JwtSecurityToken(
                        issuer: _configuraciones["JWT:ValidIssuer"],
                        audience: _configuraciones["JWT:ValidAudience"],
                        claims: authClaims,
                        expires: DateTime.UtcNow.AddHours(-1),
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuraciones["JWT:Secret"])),
                            SecurityAlgorithms.HmacSha256)
                    );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }

                return Unauthorized("Dados de Login Incorretos.");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao logar: {ex.Message}");
            }
        }
    }
}
