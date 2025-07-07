using General.Response.Usuario;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PdvNetDesktop.Sessao;
using PdvNetDesktop.Utilitarios;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Mvc;

namespace PdvNetDesktop.Views
{
    public partial class UsuariosView : UserControl
    {
        /* ───────── dados ───────── */
        private readonly BindingList<UsuarioResponse> _usuarios = new();
        private List<string> PERFIS = new();          // nomes dos perfis

        public UsuariosView()
        {
            InitializeComponent();

            /* buscas assíncronas que não travam a UI */
            Load += async (_, _) =>
            {
                PERFIS = await ObterPerfis();         // ← agora sem .Result / .GetAwaiter()
            };

            _ = CarregarAsync();                      // carga inicial da grid
        }

        /* ───────── CONTROLES ───────── */
        private ToolStrip strip;
        private ToolStripButton btnRefresh, btnNovo, btnFiltrar;
        private ToolStripTextBox txtFiltro;

        private Panel panelCentral;
        private Label lblTitulo;
        private DataGridView dgv;

        private StatusStrip status;
        private ToolStripStatusLabel lblTotal;

        private Panel pnlLoad;
        private PictureBox picLoader;

        #region Init & Layout
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            /* painel base */
            panelCentral = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(panelCentral);

            /* título */
            lblTitulo = new Label
            {
                Text = "Lista de Usuários",
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black
            };
            panelCentral.Controls.Add(lblTitulo);

            /* ToolStrip */
            strip = new ToolStrip
            {
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.Transparent,
                CanOverflow = false
            };
            btnRefresh = new ToolStripButton("🔄") { ToolTipText = "Atualizar" };
            btnRefresh.Click += async (_, _) => await CarregarAsync();

            btnNovo = new ToolStripButton("➕  Novo") { ToolTipText = "Cadastrar novo usuário" };
            btnNovo.Click += async (_, _) => await AbrirCriacao();

            strip.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, btnNovo, new ToolStripSeparator(), new ToolStripLabel("Pesquisar:")
            });

            txtFiltro = new ToolStripTextBox { Width = 220 };
            txtFiltro.TextChanged += (_, _) => Filtrar();
            strip.Items.Add(txtFiltro);

            btnFiltrar = new ToolStripButton("🔍") { ToolTipText = "Filtrar" };
            btnFiltrar.Click += (_, _) => Filtrar();
            strip.Items.Add(btnFiltrar);

            panelCentral.Controls.Add(strip);

            /* grid */
            dgv = new DoubleBufferedGrid
            {
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                Dock = DockStyle.None
            };
            dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 56, 90),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            dgv.EnableHeadersVisualStyles = false;
            dgv.RowTemplate.Height = 24;
            panelCentral.Controls.Add(dgv);
            CriarColunas();

            /* status strip */
            status = new StatusStrip { BackColor = Color.Gainsboro, Dock = DockStyle.Bottom };
            lblTotal = new ToolStripStatusLabel("0 registro(s)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            /* overlay loading */
            pnlLoad = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(140, Color.White), Visible = false };
            picLoader = new PictureBox
            {
                Image = Image.FromFile(DiretorioPastas.ObterLoading()),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Dock = DockStyle.Fill
            };
            pnlLoad.Controls.Add(picLoader);
            panelCentral.Controls.Add(pnlLoad);

            /* posicionamento dinâmico */
            panelCentral.Resize += (_, _) => Reposicionar();
            Reposicionar();
        }

        private void Reposicionar()
        {
            int cx = panelCentral.ClientSize.Width / 2;

            strip.Location = new Point(cx - strip.PreferredSize.Width / 2, 10);
            lblTitulo.Location = new Point(cx - lblTitulo.Width / 2, strip.Bottom + 10);

            int top = lblTitulo.Bottom + 10;
            int h = panelCentral.ClientSize.Height - top - 10;
            int w = Math.Min(825, panelCentral.ClientSize.Width - 40);

            dgv.Bounds = new Rectangle(cx - w / 2, top, w, h < 300 ? 300 : h);
        }
        #endregion

        #region Grid
        private void CriarColunas()
        {
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Nome", HeaderText = "Nome", Width = 160 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "E-mail", Width = 180 },
                new DataGridViewTextBoxColumn { DataPropertyName = "CPF", HeaderText = "CPF", Width = 120 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Perfil", HeaderText = "Perfil", Width = 80 },
                new DataGridViewButtonColumn { Text = "Detalhes", UseColumnTextForButtonValue = true, Width = 70 },
                new DataGridViewButtonColumn { Text = "Editar", UseColumnTextForButtonValue = true, Width = 50 },
                new DataGridViewButtonColumn { Text = "Excluir", UseColumnTextForButtonValue = true, Width = 60 }
            );
            dgv.CellClick += Dgv_CellClick;
        }
        #endregion

        #region Carregar / Filtrar
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _usuarios.Clear();

                /* desserializa no DTO da API… */
                var brutos = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<UsuarioResponse>>(
                        "api/Usuarios/GetAll",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                /* …e converte para o que o grid usa */
                if (brutos != null)
                {
                    foreach (var b in brutos)
                    {
                        _usuarios.Add(new UsuarioResponse
                        {
                            id = b.id,
                            Nome = b.Nome,    // ou outro campo que você queira exibir
                            Email = b.Email,
                            CPF = b.CPF,
                            Telefone = b.Telefone,
                            Perfil = b.perfis?.FirstOrDefault().ToString() ?? ""
                        });
                    }
                }

                dgv.DataSource = _usuarios;
                lblTotal.Text = $"{_usuarios.Count} registro(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar usuários: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { ToggleLoading(false); }
        }

        private void Filtrar()
        {
            string termo = txtFiltro.Text.Trim().ToLower();
            var dados = string.IsNullOrWhiteSpace(termo)
                ? _usuarios
                : _usuarios.Where(u =>
                       (u.Nome ?? "").ToLower().Contains(termo) ||
                       (u.Email ?? "").ToLower().Contains(termo) ||
                       (u.CPF ?? "").Contains(termo) ||
                       (u.Perfil ?? "").ToLower().Contains(termo));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} registro(s)";
        }
        #endregion

        /* clique em botões do grid */
        private void Dgv_CellClick(object? _, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex] is not DataGridViewButtonColumn btn) return;

            var u = (UsuarioResponse)dgv.Rows[e.RowIndex].DataBoundItem;
            switch (btn.Text)
            {
                case "Detalhes": MostrarDetalhes(u); break;
                case "Editar": _ = MostrarEditar(u); break;
                case "Excluir": ExcluirRegistro(u); break;
            }
        }

        /* ───────────────────────── CRUD ───────────────────────── */

        #region Criar
        private async Task AbrirCriacao()
        {
            /* garante que PERFIS já está carregado */
            if (PERFIS.Count == 0) PERFIS = await ObterPerfis();

            using var f = NovaTelaUsuario("Cadastrar usuário");

            var txtNome = new TextBox { Left = 20, Top = 40, Width = 340 };
            var txtEmail = new TextBox { Left = 20, Top = 100, Width = 340 };
            var txtCPF = new TextBox { Left = 20, Top = 160, Width = 340 };
            var txtSenha = new TextBox { Left = 20, Top = 220, Width = 340, PasswordChar = '●' };
            var txtConf = new TextBox { Left = 20, Top = 280, Width = 340, PasswordChar = '●' };

            var cmbPerfil = new ComboBox
            {
                Left = 20,
                Top = 340,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = PERFIS
            };

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = 390, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = 390, Width = 75, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[]
            {
                new Label{Text="Nome:",      Left=20, Top=20},      txtNome,
                new Label{Text="E-mail:",    Left=20, Top=80},      txtEmail,
                new Label{Text="CPF:",       Left=20, Top=140},     txtCPF,
                new Label{Text="Senha:",     Left=20, Top=200},     txtSenha,
                new Label{Text="Confirmar:", Left=20, Top=260},     txtConf,
                new Label{Text="Perfil:",    Left=20, Top=320},     cmbPerfil,
                btnSave, btnCancel
            });

            btnSave.Click += async (_, _) =>
            {
                if (txtSenha.Text != txtConf.Text)
                {
                    MessageBox.Show("Senha e confirmação não conferem.",
                                    "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dto = new
                {
                    Nome = txtNome.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    CPF = txtCPF.Text.Trim(),
                    Perfil = cmbPerfil.SelectedItem?.ToString() ?? "",
                    Senha = txtSenha.Text
                };

                var ctx = new ValidationContext(dto);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, ctx, results, true))
                {
                    MessageBox.Show(string.Join("\n", results.Select(r => r.ErrorMessage)),
                                    "Erros de validação",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var resp = await SessaoWinForms.Http.PostAsJsonAsync("api/Usuarios/Create", dto);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Usuário criado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else await ExibirErrosDaApi(resp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de comunicação: {ex.Message}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            f.ShowDialog();
        }
        #endregion

        #region Detalhes / Editar
        private static void MostrarDetalhes(UsuarioResponse u)
        {
            using var f = new Form
            {
                Text = $"Detalhes do usuário {u.id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(400, 260),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 10),
                Text = $"ID: {u.id}\n\nNome: {u.Nome}\n\nE-mail: {u.Email}\n\nCPF: {u.CPF}\n\nPerfil: {u.Perfil}"
            };
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 35 };
            f.Controls.Add(lbl);
            f.Controls.Add(btnOk);
            f.AcceptButton = btnOk;
            f.ShowDialog();
        }

        private async Task MostrarEditar(UsuarioResponse u)
        {
            /* garante que PERFIS já está carregado */
            if (PERFIS.Count == 0) PERFIS = await ObterPerfis();

            using var f = NovaTelaUsuario($"Editar usuário {u.id}");

            var txtNome = new TextBox { Text = u.Nome, Left = 20, Top = 40, Width = 340 };
            var txtEmail = new TextBox { Text = u.Email, Left = 20, Top = 100, Width = 340 };
            var txtCPF = new TextBox { Text = u.CPF, Left = 20, Top = 160, Width = 340, ReadOnly = true, BackColor = Color.Gainsboro };

            var txtSenha = new TextBox { Left = 20, Top = 220, Width = 340, PasswordChar = '●' };
            var txtConf = new TextBox { Left = 20, Top = 280, Width = 340, PasswordChar = '●' };

            var cmbPerfil = new ComboBox
            {
                Left = 20,
                Top = 340,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = PERFIS
            };
            cmbPerfil.SelectedItem = u.Perfil;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = 390, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = 390, Width = 75, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[]
            {
                new Label{Text="Nome:",      Left=20, Top=20},  txtNome,
                new Label{Text="E-mail:",    Left=20, Top=80},  txtEmail,
                new Label{Text="CPF:",       Left=20, Top=140}, txtCPF,
                new Label{Text="Senha:",     Left=20, Top=200}, txtSenha,
                new Label{Text="Confirmar:", Left=20, Top=260}, txtConf,
                new Label{Text="Perfil:",    Left=20, Top=320}, cmbPerfil,
                btnSave, btnCancel
            });

            btnSave.Click += async (_, _) =>
            {
                if (!string.IsNullOrEmpty(txtSenha.Text) || !string.IsNullOrEmpty(txtConf.Text))
                {
                    if (txtSenha.Text != txtConf.Text)
                    {
                        MessageBox.Show("Senha e confirmação não conferem.",
                                        "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                var dto = new
                {
                    Id = u.id,
                    Nome = txtNome.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Perfil = cmbPerfil.SelectedItem?.ToString() ?? "",
                    Senha = string.IsNullOrWhiteSpace(txtSenha.Text) ? null : txtSenha.Text
                };

                var ctx = new ValidationContext(dto);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, ctx, results, true))
                {
                    MessageBox.Show(string.Join("\n", results.Select(r => r.ErrorMessage)),
                                    "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var resp = await SessaoWinForms.Http.PutAsJsonAsync("api/Usuarios/Update", dto);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Usuário atualizado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else await ExibirErrosDaApi(resp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de comunicação: {ex.Message}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            f.ShowDialog();
        }
        #endregion

        #region Excluir
        private async void ExcluirRegistro(UsuarioResponse u)
        {
            if (MessageBox.Show($"Confirma excluir usuário {u.id}?",
                                "Confirmação", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var resp = await SessaoWinForms.Http.DeleteAsync($"api/Usuarios/Delete/{u.id}");
                if (resp.IsSuccessStatusCode)
                {
                    _usuarios.Remove(u);
                    lblTotal.Text = $"{_usuarios.Count} registro(s)";
                    MessageBox.Show("Usuário excluído com sucesso!",
                                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show($"Falha ao excluir: {(int)resp.StatusCode}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de comunicação: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        /* ───── Utils ───── */
        private void ToggleLoading(bool show)
        {
            pnlLoad.Visible = show;
            pnlLoad.BringToFront();
            dgv.Enabled =
            strip.Enabled =
            status.Enabled = !show;
        }

        private static async Task ExibirErrosDaApi(HttpResponseMessage resp)
        {
            if (resp.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                var errs = problem.Errors.SelectMany(kvp => kvp.Value);
                MessageBox.Show(string.Join("\n", errs),
                                "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ({(int)resp.StatusCode}):\n{txt}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Form NovaTelaUsuario(string titulo) =>
            new()
            {
                Text = titulo,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(440, 480),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }

        /* ───── Perfis ───── */
        private static async Task<List<string>> ObterPerfis()
        {
            try
            {
                var lista = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<PerfilResponse>>(
                        "api/Perfil/GetAll",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return lista?.Select(p => p.Perfil).ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problema ao obter perfis: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }
    }
}
