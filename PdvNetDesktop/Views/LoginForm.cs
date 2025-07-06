using General.DTO.Auth;
using General.Response.Auth;
using PdvNetDesktop.Components;
using PdvNetDesktop.Sessao;
using PdvNetDesktop.Utilitarios;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Forms;

namespace PdvNetDesktop
{
    public partial class LoginForm : Form
    {
        /* ---------- CONTROLES ---------- */
        private Label lblEmail, lblSenha;
        private TextBox txtEmail, txtSenha;
        private Button btnLogin;
        private PictureBox picLogo;


        private readonly SpinnerOverlay overlay;

        /* ---------- Caminhos ---------- */
        private readonly string logoPath = DiretorioPastas.ObterLogo();

        public LoginForm()
        {
            InitializeComponent();
            overlay = new SpinnerOverlay(this);
        }

        private void InitializeComponent()
        {
            /* Form */
            ClientSize = new Size(360, 220);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Imobiliaria - PdvNet";

            /* Logo */
            picLogo = new PictureBox
            {
                Size = new Size(64, 64),
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point((ClientSize.Width - 64) / 2, 10)
            };
            if (File.Exists(logoPath))
                picLogo.Image = Image.FromFile(logoPath);
            Controls.Add(picLogo);

            int yStart = 90;

            /* Email */
            lblEmail = new Label 
            {
                Text = "E-mail/CPF:",
                Location = new Point(15, yStart),
                Size = new Size(70, 20)
            };




            txtEmail = new TextBox 
            {
                Location = new Point(90, yStart - 3),
                Width = 220,

            };

            /* Senha */
            lblSenha = new Label
            { 
                Text = "Senha:",
                Location = new Point(30, yStart + 40),
                Size = new Size(60, 20)
            };
            txtSenha = new TextBox
            {
                Location = new Point(90, yStart + 37),
                Width = 220,
                PasswordChar = '●'
            };

            /* Botão */
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(235, yStart + 80),
                Size = new Size(75, 32)
            };
            btnLogin.Click += btnLogin_Click;
            AcceptButton = btnLogin;

            Controls.AddRange(new Control[] { lblEmail, txtEmail, lblSenha, txtSenha, btnLogin });

            /* Dados de teste (remova no release) */
            txtEmail.Text = "yann_cardozo@hotmail.com";
            txtSenha.Text = "Chaons26196460!@";
        }

        /* ---------- Clique Login ---------- */
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string senha = txtSenha.Text;
            string cpf = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha e-mail e senha.", "Aviso",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                overlay.Show();   // exibe spinner

                var dto = new LoginDTO { Email = email, CPF = cpf, Senha = senha };

                var resp = await SessaoWinForms.Http.PostAsJsonAsync("api/Auth/Login", dto);

                if (!resp.IsSuccessStatusCode)
                {
                    var erro = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Falha no login: {erro}", "Erro",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var loginOk = await resp.Content.ReadFromJsonAsync<LoginResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                SessaoWinForms.Jwt = loginOk!.token;
                SessaoWinForms.Http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", SessaoWinForms.Jwt);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de comunicação: {ex.Message}", "Erro",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                overlay.Hide();  // some com spinner
            }
        }
    }
}