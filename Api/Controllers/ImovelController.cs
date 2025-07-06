using General.DTO.Imovel;
using InfraData.Context;
using InfraData.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImovelController : ControllerBase
    {
        private readonly Contexto _contextoDB;
        public ImovelController(Contexto contextoDB)
        {
            _contextoDB = contextoDB;
        }

        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> ListarImoveis()
        {
            try
            {
                var imoveis = await _contextoDB.Imoveis.ToListAsync();

                return Ok(imoveis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar imóveis: {ex.Message}");
            }
        }


        [AllowAnonymous]
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> ObterImovel(int id)
        {
            try
            {
                var imoveis = await _contextoDB.Imoveis.FirstOrDefaultAsync(i => i.Id == id);
                if(imoveis == null)
                {
                    return NotFound("Imóvel não encontrado.");
                }

                return Ok(imoveis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar imóveis: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<IActionResult> CadastrarImovel([FromBody] ImovelDTO Model)
        {
            try
            {

                Imoveis NovoImovel = new()
                {
                    Tipo = Model.Tipo,
                    Endereco = Model.Endereco,
                    Status = Model.Status,
                    ValorLocacao = Model.ValorLocacao
                };


                var sucesso = await _contextoDB.Imoveis.AddAsync(NovoImovel);
                await _contextoDB.SaveChangesAsync();

                return Ok("Imovel Cadastrado com sucesso!");

            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar imóveis: {ex.Message}");
            }
        }
        [AllowAnonymous]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletarImovel(int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                var imovelparadeletar = await _contextoDB.Imoveis.FirstOrDefaultAsync(u => u.Id == id);

                if(imovelparadeletar == null)
                {
                    return BadRequest("Imóvel não encontrado para remover.");
                }

                var deletado = _contextoDB.Imoveis.Remove(imovelparadeletar);
                await _contextoDB.SaveChangesAsync();

                return Ok("Imóvel removido com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar imóveis: {ex.Message}");
            }
        }
    }
}
