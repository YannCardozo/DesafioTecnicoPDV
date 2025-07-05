using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.DTO.Usuario
{
    //model DTO de uso do registrar novos usuarios!
    public class RegistrarDTO
    {
        [Required, EmailAddress(ErrorMessage = "Insira um email válido.")]
        public string? Email { get; set; }
        [Required]
        [Length(11, 11, ErrorMessage = "CPF deve ter 11 dígitos.")]
        public string? CPF { get; set; }
        [Required]
        public string Senha { get; set; }
        [Required]
        [Compare("Senha", ErrorMessage = "A senha informada está diferente da senha confirmada.")]
        public string? ConfirmarSenha { get; set; }
        [Required]
        public string Perfil { get; set; }
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Telefone { get; set; }

    }
}
