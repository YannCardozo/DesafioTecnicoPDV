using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace General.Response.Perfil
{
    public class PerfilResponse
    {
        public int id { get; set; }
        [JsonPropertyName("Name")]
        public string Perfil { get; set; }
    }
}
