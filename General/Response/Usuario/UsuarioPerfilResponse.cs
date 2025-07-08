using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace General.Response.Usuario
{
    public class UsuarioPerfilResponse
    {
        [JsonPropertyName("Name")]
        public string Perfil { get; set; }
    }
}
