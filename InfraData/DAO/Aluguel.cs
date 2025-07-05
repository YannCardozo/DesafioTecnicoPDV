using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfraData.DAO
{
    public class Aluguel : EntidadeBase<int>
    {
        public int ImovelId { get; set; }
        public int UsuarioId { get; set; }

        public Imoveis Imovel { get; set; }
        public Usuario Usuario { get; set; }
        public decimal ValorLocacao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataTermino { get; set; }
    }
}
