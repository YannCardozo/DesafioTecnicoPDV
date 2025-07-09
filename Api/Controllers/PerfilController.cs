using General.DTO.Perfil;
using General.Response.Imovel;
using General.Response.Perfil;
using InfraData.Context;
using InfraData.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfilController : ControllerBase
    {
        private readonly Contexto _contextoDB;
        private readonly UserManager<Usuario> _userManager;

        public PerfilController(Contexto contextoDB, UserManager<Usuario> userManager)
        {
            _contextoDB = contextoDB;
            _userManager = userManager;
        }
        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [Authorize]
        [HttpGet("GetAll")]
        public async Task<IActionResult> ListarPerfis()
        {
            try
            {
                var PerfisCriados = _contextoDB.Roles.AsNoTracking().OrderBy(i => i.Id).ToList();
                return Ok(PerfisCriados);
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao LISTAR PERFIS: {ex.Message}");
            }
        }



        //alterar para Authorization forçando o estado de autenticação do usuario para LOGADO.
        [Authorize]
        [HttpPost("Create")]
        public async Task<IActionResult> CriarPerfil([FromBody] PerfilDTO Model)
        {
            try
            {
                if (string.IsNullOrEmpty(Model.Perfil))
                {
                    return BadRequest("Perfil está vazio.");
                }

                var perfilExistente = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Name == Model.Perfil);
                if (perfilExistente != null)
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

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] PerfilResponse Model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var perfilExistente = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Name == Model.Perfil);

                if (perfilExistente == null)
                {
                    return NotFound("Não encontrado o PERFIL para ATUALIZAR.");
                }

                perfilExistente.Name = Model.Perfil;

                _contextoDB.Roles.Update(perfilExistente);
                await _contextoDB.SaveChangesAsync();

                return Ok("Perfil ATUALIZADO com sucesso!");

            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar Perfis: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("Delete/{id}")]
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
        [Authorize]
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
                else if (string.IsNullOrEmpty(Model.Email))
                {
                    verificausuario = await _userManager.Users.FirstOrDefaultAsync(u => u.CPF == Model.CPF);
                }

                if (verificausuario == null)
                {
                    return BadRequest($@"Erro ao ASSOCIAR PERFIL: USUÁRIO NÃO ENCONTRADO.");
                }

                var verificaperfil = await _contextoDB.Roles.FirstOrDefaultAsync(p => p.Name == Model.Perfil);
                if (verificaperfil == null)
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
