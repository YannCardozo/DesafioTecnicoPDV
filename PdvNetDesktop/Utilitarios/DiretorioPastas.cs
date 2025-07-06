using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Utilitarios
{
    public static class DiretorioPastas
    {
        public static string ObterLogo()
                    => EncontrarArquivoEmResources("logo.png");

        /// <summary>
        /// Sobe pelas pastas a partir do executável até encontrar Resources\loader.gif
        /// e retorna esse caminho físico.
        /// </summary>
        public static string ObterLoading()
            => EncontrarArquivoEmResources("loader.gif");

        private static string EncontrarArquivoEmResources(string nomeArquivo)
        {
            // Começa da pasta onde está o executável:
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            // Sobe enquanto houver diretório pai
            while (dir != null)
            {
                // Monta o possível caminho: <dir>\Resources\nomeArquivo
                var candidate = Path.Combine(dir.FullName, "Resources", nomeArquivo);
                if (File.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }

            // Se não achou, joga exceção ou retorna string vazia
            throw new FileNotFoundException(
                $"Não foi possível localizar '{nomeArquivo}' em nenhuma pasta Resources acima de '{AppContext.BaseDirectory}'.");
        }

    }
}
