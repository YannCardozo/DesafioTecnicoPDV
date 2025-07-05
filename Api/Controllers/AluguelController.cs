using General.DTO.Aluguel;
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
    public class AluguelController : ControllerBase
    {
        private readonly Contexto _contextoDB;

        public AluguelController(Contexto contextoDB)
        {
            _contextoDB = contextoDB;
        }


        [AllowAnonymous]
        [HttpGet("GetAll")]
        public async Task<IActionResult> ListarAlugueis()
        {
            try
            {
                var alugueis = await _contextoDB.Alugueis.ToListAsync();

                return Ok(alugueis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar ALUGUEIS: {ex.Message}");
            }
        }


        [AllowAnonymous]
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> ObterAluguel(int id)
        {
            try
            {
                var aluguel = await _contextoDB.Alugueis.FirstOrDefaultAsync(i => i.Id == id);
                if (aluguel == null)
                {
                    return NotFound("Aluguel não encontrado.");
                }

                return Ok(aluguel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao obter ALUGUEL: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<IActionResult> CadastrarAluguel([FromBody] AluguelDTO Model)
        {
            try
            {

                var usuarioverifica = await _contextoDB.Users.FirstOrDefaultAsync(u => u.Id == Model.UsuarioId);
                if(usuarioverifica == null)
                {
                    return BadRequest("Usuário não encontrado para cadastrar o ALUGUEL.");
                }
                var imovelverifica = await _contextoDB.Imoveis.FirstOrDefaultAsync(i => i.Id == Model.ImovelId);
                if (imovelverifica == null)
                {
                    return BadRequest("Imovel não encontrado para cadastrar o ALUGUEL.");
                }

                Aluguel NovoAluguel = new()
                {
                    UsuarioId = Model.UsuarioId,
                    ImovelId = Model.ImovelId,
                    ValorLocacao = Model.ValorLocacao,
                    DataInicio = Model.DataInicio,
                    DataTermino = Model.DataTermino
                };


                var sucesso = await _contextoDB.Alugueis.AddAsync(NovoAluguel);
                await _contextoDB.SaveChangesAsync();

                return Ok("Aluguel Cadastrado com sucesso! " + sucesso);

            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao cadastrar ALUGUEL: {ex.Message}");
            }
        }
        [AllowAnonymous]
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletarAluguel(int id)
        {
            try
            {

                var imovelparadeletar = await _contextoDB.Alugueis.FirstOrDefaultAsync(u => u.Id == id);

                if (imovelparadeletar == null)
                {
                    return BadRequest("Aluguel não encontrado para remover.");
                }

                var deletado = _contextoDB.Alugueis.Remove(imovelparadeletar);
                await _contextoDB.SaveChangesAsync();

                return Ok("Aluguel removido com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao deletar ALUGUEL: {ex.Message}");
            }
        }
    }
}
