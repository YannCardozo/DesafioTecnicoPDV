using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.DTO.Imovel
{
    public class ImovelDTO
    {
        public string? Tipo { get; set; }
        [Required(ErrorMessage = "Preencha STATUS.")]
        [StringLength(155, ErrorMessage = "Endereco pode ter no máximo 20 caracteres")]
        public string Endereco { get; set; }
        [Required(ErrorMessage = "Preencha STATUS.")]
        [StringLength(20, ErrorMessage = "O status pode ter no máximo 20 caracteres")]
        public string Status { get; set; }
        [Required(ErrorMessage = "Preencha o valor de locação.")]
        public decimal ValorLocacao { get; set; }
    }
}
