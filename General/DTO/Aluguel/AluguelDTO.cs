using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.DTO.Aluguel
{
    public class AluguelDTO
    {
        [Required]
        public int ImovelId { get; set; }
        [Required]
        public int UsuarioId { get; set; }
        [Required(ErrorMessage = "Informe o valor de locação do ALUGUEL.")]
        public decimal ValorLocacao { get; set; }
        [Required(ErrorMessage = "Informe a data de início do ALUGUEL.")]
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
    }
}
