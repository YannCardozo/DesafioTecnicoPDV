using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Sessao
{
    public static class SessaoWinForms
    {
        public static string Jwt { get; set; }
        public static HttpClient Http { get; } = new()
        {
            BaseAddress = new Uri("https://localhost:7288/")
        };
    }
}
