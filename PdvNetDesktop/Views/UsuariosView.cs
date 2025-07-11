using General.DTO.Usuario;
using General.Response.Usuario;
using Microsoft.AspNetCore.Mvc;
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

namespace PdvNetDesktop.Views
{
    public partial class UsuariosView : UserControl
    {

        private readonly BindingList<UsuarioResponse> _usuarios = new();
        private List<string> PERFIS = new();

        public UsuariosView()
        {
            InitializeComponent();
            Load += async (_, _) => PERFIS = await ObterPerfis();
            _ = CarregarAsync();
        }

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

            panelCentral = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(panelCentral);

            lblTitulo = new Label
            {
                Text = "Lista de Locatários",
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black
            };
            panelCentral.Controls.Add(lblTitulo);

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

            status = new StatusStrip { BackColor = Color.Gainsboro, Dock = DockStyle.Bottom };
            lblTotal = new ToolStripStatusLabel("0 registro(s)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            pnlLoad = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(140, Color.White), Visible = false };
            picLoader = new PictureBox
            {
                Image = Image.FromFile(DiretorioPastas.ObterLoading()),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Dock = DockStyle.Fill
            };
            pnlLoad.Controls.Add(picLoader);
            panelCentral.Controls.Add(pnlLoad);

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
            int w = Math.Min(944, panelCentral.ClientSize.Width - 40);

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
                new DataGridViewTextBoxColumn { DataPropertyName = "Telefone", HeaderText = "Tel.", Width = 120 },
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

                var brutos = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<UsuarioResponse>>(
                        "api/Usuarios/GetAll",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (brutos != null)
                    foreach (var b in brutos)
                        _usuarios.Add(new UsuarioResponse
                        {
                            id = b.id,
                            Nome = b.Nome,
                            Email = b.Email,
                            CPF = b.CPF,
                            Telefone = b.Telefone,
                            Perfil = b.perfis?.FirstOrDefault() ?? "Sem Perfil"
                        });

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
            string t = txtFiltro.Text.Trim().ToLower();
            var dados = string.IsNullOrWhiteSpace(t)
                ? _usuarios
                : _usuarios.Where(u =>
                       (u.Nome ?? "").ToLower().Contains(t) ||
                       (u.Email ?? "").ToLower().Contains(t) ||
                       (u.CPF ?? "").Contains(t) ||
                       (u.Telefone ?? "").Contains(t) ||
                       (u.Perfil ?? "").ToLower().Contains(t));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} registro(s)";
        }
        #endregion

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
            if (PERFIS.Count == 0) PERFIS = await ObterPerfis();

            using var f = NovaTelaUsuario("Cadastrar usuário");

            int top = 40;
            Control L(string txt) => new Label { Text = txt, Left = 20, Top = top - 18 };
            TextBox T(bool pass = false)
                => new() { Left = 20, Top = top, Width = 340, PasswordChar = pass ? '●' : '\0' };

            var txtNome = T(); f.Controls.AddRange(new Control[] { L("Nome"), txtNome }); top += 55;
            var txtMail = T(); f.Controls.AddRange(new Control[] { L("E-mail"), txtMail }); top += 55;
            var txtCPF = T(); f.Controls.AddRange(new Control[] { L("CPF"), txtCPF }); top += 55;
            var txtTel = T(); f.Controls.AddRange(new Control[] { L("Telefone"), txtTel }); top += 55;
            var txtPass = T(true); f.Controls.AddRange(new Control[] { L("Senha"), txtPass }); top += 55;
            var txtConf = T(true); f.Controls.AddRange(new Control[] { L("Confirmar"), txtConf }); top += 55;

            var cmbPerfil = new ComboBox
            {
                Left = 20,
                Top = top,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = PERFIS
            };
            f.Controls.AddRange(new Control[] { L("Perfil"), cmbPerfil }); top += 70;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = top, Width = 75 };
            var btnCancel = new Button
            {
                Text = "Cancelar",
                Left = 285,
                Top = top,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                if (txtPass.Text != txtConf.Text)
                {
                    MessageBox.Show("Senha e confirmação não conferem.",
                                    "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                RegistrarDTO Model = new()
                {
                    Nome = txtNome.Text.Trim(),
                    Email = txtMail.Text.Trim(),
                    CPF = txtCPF.Text.Trim(),
                    Telefone = txtTel.Text.Trim(),
                    Perfil = cmbPerfil.SelectedItem?.ToString(),
                    Senha = txtPass.Text,
                    ConfirmarSenha = txtConf.Text
                };

                var ctx = new ValidationContext(Model);
                var res = new List<ValidationResult>();
                if (!Validator.TryValidateObject(Model, ctx, res, true))
                {
                    MessageBox.Show(string.Join("\n", res.Select(r => r.ErrorMessage)),
                                    "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var rsp = await SessaoWinForms.Http.PostAsJsonAsync("api/Usuarios/Create", Model);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Usuário criado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else await ExibirErrosDaApi(rsp);
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
                ClientSize = new Size(420, 320),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 10),
                Text =
                    $"ID:        {u.id}\n\n" +
                    $"Nome:      {u.Nome}\n\n" +
                    $"E-mail:    {u.Email}\n\n" +
                    $"CPF:       {u.CPF}\n\n" +
                    $"Telefone:  {u.Telefone}\n\n" +
                    $"Perfil:    {u.Perfil}"
            };
            var btnOk = new Button { Text = "OK", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK };
            f.Controls.AddRange(new Control[] { lbl, btnOk });
            f.AcceptButton = btnOk;
            f.ShowDialog();
        }

        private async Task MostrarEditar(UsuarioResponse u)
        {
            if (PERFIS.Count == 0) PERFIS = await ObterPerfis();

            using var f = NovaTelaUsuario($"Editar usuário {u.Nome}");

            int top = 40;
            Control L(string t) => new Label { Text = t, Left = 20, Top = top - 18 };
            TextBox T(string txt, bool ro = false)
                => new()
                {
                    Text = txt,
                    Left = 20,
                    Top = top,
                    Width = 340,
                    ReadOnly = ro,
                    BackColor = ro ? Color.Gainsboro : SystemColors.Window
                };

            var txtNome = T(u.Nome); f.Controls.AddRange(new[] { L("Nome"), txtNome }); top += 55;
            var txtMail = T(u.Email); f.Controls.AddRange(new[] { L("E-mail"), txtMail }); top += 55;
            var txtCPF = T(u.CPF, true); f.Controls.AddRange(new[] { L("CPF"), txtCPF }); top += 55;
            var txtTel = T(u.Telefone); f.Controls.AddRange(new[] { L("Telefone"), txtTel }); top += 55;

            var txtPass = T("", true); txtPass.PasswordChar = '●'; txtPass.ReadOnly = false;
            txtPass.BackColor = SystemColors.Window;
            f.Controls.AddRange(new[] { L("Senha"), txtPass }); top += 55;

            var txtConf = T("", true); txtConf.PasswordChar = '●'; txtConf.ReadOnly = false;
            f.Controls.AddRange(new[] { L("Confirmar"), txtConf }); top += 55;
            txtConf.BackColor = SystemColors.Window;

            var cmbPerfil = new ComboBox
            {
                Left = 20,
                Top = top,
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = PERFIS
            };
            cmbPerfil.SelectedItem = u.Perfil;
            f.Controls.AddRange(new[] { L("Perfil"), cmbPerfil }); top += 70;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = top, Width = 75 };
            var btnCancel = new Button
            {
                Text = "Cancelar",
                Left = 285,
                Top = top,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                if (!string.IsNullOrEmpty(txtPass.Text) ||
                    !string.IsNullOrEmpty(txtConf.Text))
                {
                    if (txtPass.Text != txtConf.Text)
                    {
                        MessageBox.Show("Senha e confirmação não conferem.",
                                        "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }



                UsuarioResponse UsuarioAtualizado = new()
                {
                    id = u.id,
                    Nome = txtNome.Text,
                    Email = txtMail.Text.Trim(),
                    Telefone = txtTel.Text.Trim(),
                    Perfil = cmbPerfil.SelectedItem?.ToString(),
                    Senha = txtPass.Text.Trim(),
                    ConfirmarSenha = txtConf.Text.Trim(),
                    CPF = txtCPF.Text.Trim()
                };

                var ctx = new ValidationContext(UsuarioAtualizado);
                var res = new List<ValidationResult>();
                if (!Validator.TryValidateObject(UsuarioAtualizado, ctx, res, true))
                {
                    MessageBox.Show(string.Join("\n", res.Select(r => r.ErrorMessage)),
                                    "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var rsp = await SessaoWinForms.Http.PutAsJsonAsync("api/Usuarios/Update", UsuarioAtualizado);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Usuário atualizado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else await ExibirErrosDaApi(rsp);
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
            dgv.Enabled = strip.Enabled = status.Enabled = !show;
        }

        private static async Task ExibirErrosDaApi(HttpResponseMessage resp)
        {
            if (resp.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                MessageBox.Show(string.Join("\n", problem.Errors.SelectMany(kvp => kvp.Value)),
                                "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ({(int)resp.StatusCode}):\n{txt}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Form NovaTelaUsuario(string titulo) => new()
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = new Size(450, 560),
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
                    .GetFromJsonAsync<List<UsuarioPerfilResponse>>(
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
