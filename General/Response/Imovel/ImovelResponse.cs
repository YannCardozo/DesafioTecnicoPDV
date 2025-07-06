using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace General.Response.Imovel
{
    public class ImovelResponse
    {
        public string Tipo { get; set; }
        public string Endereco { get; set; }
        public decimal ValorLocacao { get; set; }
        public string? Status { get; set; }
        public int id { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
    }
}
