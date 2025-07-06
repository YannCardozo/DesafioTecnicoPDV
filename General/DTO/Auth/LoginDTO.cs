using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.DTO.Auth
{
    //model DTO do caso de uso do login, responsavel por ser a model do FRONT para a api usuarios controller
    //LOGIN
    public class LoginDTO
    {
        public string? Email { get; set; }
        public string? CPF { get; set; }
        public string Senha { get; set; }
    }
}
