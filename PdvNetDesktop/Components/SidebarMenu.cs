using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdvNetDesktop.Components
{
    public partial class SidebarMenu : UserControl
    {
        public event Action<MenuOption>? OptionSelected;

        public SidebarMenu()
        {
            InitializeComponent();
        }

        public enum MenuOption { Imovel, Aluguel, Perfis, Usuarios }

        private Button btnImovel, btnAluguel, btnPerfis, btnUsuarios;

        private void InitializeComponent()
        {
            Dock = DockStyle.Left;
            Width = 180;
            BackColor = Color.FromArgb(32, 41, 64);     // azul-escuro

            Font menuFont = new Font("Segoe UI", 10, FontStyle.Bold);
            int top = 40;

            btnImovel = CriarBotao("Imóveis", MenuOption.Imovel, top); top += 50;
            btnAluguel = CriarBotao("Aluguéis", MenuOption.Aluguel, top); top += 50;
            btnPerfis = CriarBotao("Perfis", MenuOption.Perfis, top); top += 50;
            btnUsuarios = CriarBotao("Usuários", MenuOption.Usuarios, top);

            Controls.AddRange(new Control[]
            { btnImovel, btnAluguel, btnPerfis, btnUsuarios });
        }

        private Button CriarBotao(string texto, MenuOption opt, int top)
        {
            var btn = new Button
            {
                Text = texto,
                Tag = opt,
                Width = 160,
                Height = 40,
                Top = top,
                Left = 10,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(45, 56, 90),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) => OptionSelected?.Invoke(opt);
            return btn;
        }
    }
}
