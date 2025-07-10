using General.DTO.Aluguel;
using General.DTO.Imovel;
using General.Response.Aluguel;
using General.Response.Usuario;
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
    public class AluguelController : ControllerBase
    {
        private readonly Contexto _contextoDB;

        public AluguelController(Contexto contextoDB)
        {
            _contextoDB = contextoDB;
        }


        [Authorize]
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

        [Authorize]
        [HttpGet("GetAllById/{id}")]
        public async Task<IActionResult> ListarAlugueisDoLocatario(int id)
        {
            try
            {
                var alugueis = await _contextoDB.Alugueis.Where(u => u.UsuarioId == id).ToListAsync();
                if (alugueis.Count <= 0)
                {
                    return NotFound("Não encontrado Aluguéis para esse Locatário.");
                }


                foreach(var aluguel in alugueis)
                {
                    var ImovelLocalizado = await _contextoDB.Imoveis.AsNoTracking().FirstOrDefaultAsync(u => u.Id == aluguel.ImovelId);
                    var LocatarioLocalizado = await _contextoDB.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == aluguel.UsuarioId);
                    if(ImovelLocalizado != null)
                    {
                        aluguel.Imovel = ImovelLocalizado;
                    }
                    if (LocatarioLocalizado != null)
                    {
                        aluguel.Usuario = LocatarioLocalizado;
                    }

                }

                return Ok(alugueis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao listar ALUGUEIS: {ex.Message}");
            }
        }


        [Authorize]
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

        [Authorize]
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
                    DataTermino = Model.DataTermino,
                    DataCriacao = DateTime.UtcNow,
                    DataAtualizacao = DateTime.UtcNow.AddHours(-2)
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

        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> AtualizarAluguel([FromBody] AluguelResponse model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if(model.Id <= 0)
                {
                    return BadRequest("ID inválido para atualizar o ALUGUEL.");
                }

                if(model.ValorLocacao <= 0)
                {
                    return BadRequest("Valor de locação inválido para atualizar o ALUGUEL.");
                }

                var aluguelexistente = await _contextoDB.Alugueis.FirstOrDefaultAsync(u => u.Id == model.Id);
                if(aluguelexistente == null)
                {
                    return NotFound("Não foi possível ATUALIZAR esse ALUGUEL pois não foi ENCONTRADO.");
                }


                    aluguelexistente.Id = model.Id;
                    aluguelexistente.ImovelId = model.ImovelId;
                    aluguelexistente.UsuarioId = model.UsuarioId;
                    aluguelexistente.ValorLocacao = model.ValorLocacao;
                    aluguelexistente.DataInicio = model.DataInicio;
                    aluguelexistente.DataTermino = model.DataTermino;
                    aluguelexistente.DataAtualizacao = DateTime.UtcNow.AddHours(-2);



                _contextoDB.Alugueis.Update(aluguelexistente);
                await _contextoDB.SaveChangesAsync();


                return Ok("ALUGUEL atualizado com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest($@"Erro ao ATUALIZAR ALUGUEL: {ex.Message}");
            }

        }



        [Authorize]
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
