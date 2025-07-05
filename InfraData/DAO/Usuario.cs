using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraData.DAO
{
    public class Usuario : IdentityUser<int>
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Telefone { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; }
    }
}
