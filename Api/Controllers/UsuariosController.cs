using General.DTO.Usuario;
using InfraData.Context;
using InfraData.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public class UsuariosController : ControllerBase
    {
        //controladora de caso de uso destinado a autenticação, autorização e criação de usuarios.
        //PERFIS, LOGIN , REGISTRO.
        private readonly Contexto _contextoDB;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuraciones;
        public UsuariosController(Contexto contexto, UserManager<Usuario> userManager,
                                    SignInManager<Usuario> login, IConfiguration configuraciones)
        {
            _contextoDB = contexto;
            _userManager = userManager;
            _signInManager = login;
            _configuraciones = configuraciones;
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO Model)
        {
            try
            {
                Usuario usuariolocalizado = null;

                if (!string.IsNullOrWhiteSpace(Model.CPF))
                {
                    usuariolocalizado = await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == Model.CPF);
                }
                else if (!string.IsNullOrWhiteSpace(Model.Email))
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
                        expires: DateTime.UtcNow.AddHours(2),
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
            catch(Exception ex)
            {
                return BadRequest($@"Erro ao logar: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] RegistrarDTO Model)
        {
            try
            {
                Usuario usuariolocalizado = null;
                if (string.IsNullOrEmpty(Model.CPF))
                {
                    usuariolocalizado = await _userManager.FindByEmailAsync(Model.Email);
                }
                else if (string.IsNullOrEmpty(Model.Email))
                {
                    usuariolocalizado = await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == Model.CPF);
                }


                if (usuariolocalizado != null)
                {
                    return Conflict("Usuário já cadastrado com esse CPF ou Email.");
                }

                var telefone_duplicado = await _userManager.Users.AnyAsync(u => u.Telefone == Model.Telefone);
                if (telefone_duplicado != null)
                {
                    return Conflict("Telefone já cadastrado.");
                }


                //a partir daqui o usuário foi VALIDADO e sera criado no banco.
                //CPF será o USERNAME
                Usuario NovoUsuario = new();
                NovoUsuario.Email = Model.Email;
                NovoUsuario.CPF = Model.CPF;
                NovoUsuario.UserName = Model.CPF;
                NovoUsuario.Nome = Model.Email;
                NovoUsuario.Telefone = Model.Telefone;

                var resultado_criado = await _userManager.CreateAsync(NovoUsuario, Model.Senha);

                if (!resultado_criado.Succeeded)
                {
                    return BadRequest(resultado_criado.Errors);
                }

                await _contextoDB.SaveChangesAsync();


                return Ok("Usuário registrado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao REGISTRAR USUARIO: {ex.Message}");
            }
        }
        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [AllowAnonymous]
        [HttpGet("ListarUsuarios")]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                var usuarioscomperfis = await _contextoDB.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.CPF,
                        u.Telefone,
                        Perfis = _contextoDB.UserRoles
                            .Where(ur => ur.UserId == u.Id)
                            .Join(_contextoDB.Roles,
                                  ur => ur.RoleId,
                                  r => r.Id,
                                  (ur, r) => r.Name)
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(usuarioscomperfis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao LISTAR USUÁRIOS: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpDelete("DeletarUsuario/{id}")]
        public async Task<IActionResult> DeletarUsuario(int id)
        {
            try
            {
                var deletarusuario = await _contextoDB.Users.FirstOrDefaultAsync(u => u.Id == id);
                if(deletarusuario == null)
                {
                    return NotFound("Usuário não encontrado.");
                }


                _contextoDB.Users.Remove(deletarusuario);
                await _contextoDB.SaveChangesAsync();
                return Ok("Usuário deletado com sucesso!");
            }
            catch(Exception ex)
            {
                return BadRequest($@"Erro ao DELETAR USUÁRIO: {ex.Message}");
            }

        }




        //-------------------------------------------------PERFIS----------------------------------------------------

        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [AllowAnonymous]
        [HttpGet("ListarPerfis")]
        public async Task<IActionResult> ListarPerfis()
        {
            try
            {
                var PerfisCriados = _contextoDB.Roles.ToList();
                return Ok(PerfisCriados);
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao LISTAR PERFIS: {ex.Message}");
            }
        }



        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [AllowAnonymous]
        [HttpPost("CriarPerfil")]
        public async Task<IActionResult> CriarPerfil([FromBody] PerfilDTO Model)
        {
            try
            {
                if(string.IsNullOrEmpty(Model.Perfil))
                {
                    return BadRequest("Perfil está vazio.");
                }

                var perfilExistente = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Name == Model.Perfil);
                if(perfilExistente != null)
                {
                    return Conflict("Perfil já cadastrado.");
                }


                var novoPerfil = new IdentityRole<int>
                {
                    Name = Model.Perfil,
                    NormalizedName = Model.Perfil.ToUpper()
                };

                var resultado = await _contextoDB.Roles.AddAsync(novoPerfil);

                await _contextoDB.SaveChangesAsync();

                return Ok(@$"Perfil: {Model.Perfil} cadastrado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao Criar PERFIL: {ex.Message}");
            }
        }


        [AllowAnonymous]
        [HttpDelete("DeletarPerfil/{id}")]
        public async Task<IActionResult> DeletarPerfil(int id)
        {
            try
            {
                var perfil = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Id == id);
                if (perfil == null)
                {
                    return NotFound("Perfil não encontrado.");
                }
                var userRoles = await _contextoDB.UserRoles.Where(ur => ur.RoleId == id).ToListAsync();
                _contextoDB.UserRoles.RemoveRange(userRoles);
                _contextoDB.Roles.Remove(perfil);
                await _contextoDB.SaveChangesAsync();
                return Ok("Perfil deletado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao DELETAR PERFIL: {ex.Message}");
            }
        }

        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [AllowAnonymous]
        [HttpPost("AssociarPerfil")]
        public async Task<IActionResult> AssociarPerfil([FromBody] PerfilDTO Model)
        {
            try
            {
                Usuario verificausuario = null;
                if (string.IsNullOrEmpty(Model.CPF))
                {
                    verificausuario = await _userManager.FindByEmailAsync(Model.Email);
                }
                else if(string.IsNullOrEmpty(Model.Email))
                {
                    verificausuario = await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == Model.CPF);
                }
                
                if(verificausuario == null)
                {
                    return BadRequest($@"Erro ao ASSOCIAR PERFIL: USUÁRIO NÃO ENCONTRADO.");
                }

                var verificaperfil = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Name == Model.Perfil);
                if(verificaperfil == null)
                {
                    return BadRequest($@"Erro ao ASSOCIAR PERFIL: PERFIL NÃO ENCONTRADO.");
                }


                await _contextoDB.UserRoles.AddAsync(new IdentityUserRole<int>
                {
                    UserId = verificausuario.Id,
                    RoleId = verificaperfil.Id
                });

                await _contextoDB.SaveChangesAsync();
                return Ok("Perfil Associado corretamente ao Usuário.");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao Associar Perfil do Usuario: {ex.Message}");
            }
        }

    }
}
