using General.DTO.Usuario;
using General.Response.Imovel;
using General.Response.Usuario;
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
        public UsuariosController(Contexto contexto, UserManager<Usuario> userManager, SignInManager<Usuario> loginManager)
        {
            _contextoDB = contexto;
            _userManager = userManager;
            _signInManager = loginManager;
        }

        

        [AllowAnonymous]
        [HttpPost("Create")]
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
                NovoUsuario.Nome = Model.Nome;
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

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> AtualizarUsuario([FromBody] UsuarioResponse model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1) Busca o usuário existente
            var usuario = await _contextoDB.Users.FirstOrDefaultAsync(u => u.Id == model.id);
            if (usuario == null)
            {
                return NotFound("Usuário não encontrado para atualizar.");
            }


            // 2) Se veio senha nova, faz a troca
            if (!string.IsNullOrEmpty(model.Senha))
            {
                // Para ChangePasswordAsync você precisa da senha antiga. Se não tiver, 
                // pode usar ResetPasswordAsync com um token.
                // Exemplo: reset com token
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var changePassResult = await _userManager.ResetPasswordAsync(usuario, token, model.Senha);
                if (!changePassResult.Succeeded)
                    return BadRequest(string.Join("; ", changePassResult.Errors.Select(e => e.Description)));
            }

            // 3) Se veio perfil novo, atualiza roles
            if (!string.IsNullOrEmpty(model.Perfil))
            {
                // obtém roles atuais
                var rolesAtuais = await _userManager.GetRolesAsync(usuario);
                // remove cada um
                if (rolesAtuais.Count > 0)
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(usuario, rolesAtuais);
                    if (!removeResult.Succeeded)
                        return BadRequest(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                }
                // adiciona o novo perfil
                var addResult = await _userManager.AddToRoleAsync(usuario, model.Perfil);
                if (!addResult.Succeeded)
                    return BadRequest(string.Join("; ", addResult.Errors.Select(e => e.Description)));
            }

            // 4) Atualiza demais campos
            usuario.Email = model.Email;
            usuario.CPF = model.CPF;
            usuario.Nome = model.Nome;
            usuario.Telefone = model.Telefone;

            // 5) Persiste as alterações
            var updateResult = await _userManager.UpdateAsync(usuario);
            if (!updateResult.Succeeded)
                return BadRequest(string.Join("; ", updateResult.Errors.Select(e => e.Description)));

            return Ok("Usuário atualizado com sucesso!");
        }


        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                //utilizando a LINQ para retornar os usuarios do banco com seus respectivos PERFIS vindo da tabela userroles do identity.
                var usuarioscomperfis = await _contextoDB.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Nome,
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
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> ObterUsuario(int id)
        {
            try
            {
                //utilizando a LINQ para retornar os usuarios do banco com seus respectivos PERFIS vindo da tabela userroles do identity.
                var usuariocomperfil = await _contextoDB.Users
                    .Where(u => u.Id == id)
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
                    .FirstOrDefaultAsync();

                if (usuariocomperfil == null)
                {
                    return NotFound("Usuário não encontrado.");
                }

                return Ok(usuariocomperfil);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao OBTER USUÁRIO: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpDelete("Delete/{id}")]
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
        

    }
}
