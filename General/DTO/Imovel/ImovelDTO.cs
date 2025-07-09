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
        [Required(ErrorMessage = "Preencha Endereco.")]
        [StringLength(155, ErrorMessage = "Endereco pode ter no máximo 155 caracteres")]
        [RegularExpression(@"^[A-Za-z0-9\sºª]+$", ErrorMessage = "Endereço só pode conter letras, números, espaços e os símbolos º e ª")]
        public string Endereco { get; set; }
        [Required(ErrorMessage = "Preencha STATUS.")]
        [StringLength(20, ErrorMessage = "O status pode ter no máximo 20 caracteres")]
        public string Status { get; set; }
        [Required(ErrorMessage = "Preencha o valor de locação.")]
        public decimal ValorLocacao { get; set; }

        
        public string? ImagemBase64 { get; set; }
    }
}
