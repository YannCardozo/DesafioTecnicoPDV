using PdvNetDesktop.Components;
using System.Drawing;
using System.Net.Http.Json;
using System.Windows.Forms;

namespace PdvNetDesktop.Views
{
    public partial class FormPrincipal : Form
    {
        private SidebarMenu sidebar;
        private Panel pnlContent;
        private Panel pnlLoading;
        private PictureBox picLoader;

        public FormPrincipal()
        {
            InitializeComponent();
            Text = "PdvNet – Gerenciamento Imobiliário";
        }

        private void InitializeComponent()
        {
            // janela principal
            ClientSize = new Size(1666, 900);
            StartPosition = FormStartPosition.CenterScreen;

            // 1) menu lateral
            sidebar = new SidebarMenu();
            sidebar.OptionSelected += Sidebar_OptionSelected;
            Controls.Add(sidebar);

            // 2) painel de conteúdo (à direita do menu)
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(pnlContent);

            // 3) overlay de loading
            pnlLoading = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(150, Color.White),
                Visible = false
            };
            picLoader = new PictureBox
            {
                Image = Image.FromFile(Utilitarios.DiretorioPastas.ObterLogo()), // retorna Image
                SizeMode = PictureBoxSizeMode.CenterImage,
                Dock = DockStyle.Fill
            };
            pnlLoading.Controls.Add(picLoader);
            pnlContent.Controls.Add(pnlLoading);

            // carrega a primeira opção ao abrir
            Shown += (_, _) => Sidebar_OptionSelected(SidebarMenu.MenuOption.Imovel);
        }

        // ── troca a view de acordo com a opção clicada
        private void Sidebar_OptionSelected(SidebarMenu.MenuOption opt)
        {
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(pnlLoading); // mantém overlay dentro

            UserControl view = opt switch
            {
                SidebarMenu.MenuOption.Imovel => new ImoveisView(),
                SidebarMenu.MenuOption.Aluguel => new AluguelView(),
                SidebarMenu.MenuOption.Perfis => new PerfilView(),
                SidebarMenu.MenuOption.Usuarios => new UsuariosView(),
                _ => new UserControl()
            };

            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
            view.BringToFront();
        }
    }
}
