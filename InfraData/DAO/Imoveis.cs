using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraData.DAO
{
    public class Imoveis : EntidadeBase<int>
    {
        //casa apartamento etc
        public string? Tipo { get; set; }
        public string Endereco { get; set; }
        public decimal ValorLocacao { get; set; }
        //disponivel ou alugado
        public string Status { get; set; }

        //imagem de IMOVEL
        public string? ImagemBase64 { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; } // 1 imóvel → N aluguéis
    }
}
