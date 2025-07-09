using General.DTO.Perfil;
using General.DTO.Usuario;              // PerfilDTO
using General.Response.Perfil;
using General.Response.Usuario;         // PerfilResponse & UsuarioResponse
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
    public sealed partial class PerfilView : UserControl
    {
        /* --------------------------- dados ------------------------------- */
        private readonly BindingList<PerfilResponse> _perfis = new();
        private List<UsuarioResponse> _usuarios = new(); // ← e-mails para associação

        public PerfilView()
        {
            InitializeComponent();

            Load += async (_, _) =>
            {
                _usuarios = await ObterUsuarios();   // e-mails p/ combo
            };

            _ = CarregarAsync();
        }

        /* ---------------------- controles ------------------------------- */
        private ToolStrip strip;
        private ToolStripButton btnRefresh, btnNovo, btnAssociar, btnFiltrar;
        private ToolStripTextBox txtFiltro;

        private Panel panelCentral;
        private Label lblTitulo;
        private DataGridView dgv;

        private StatusStrip status;
        private ToolStripStatusLabel lblTotal;

        private Panel pnlLoad;
        private PictureBox picLoader;

        /* ==========================  UI  ================================= */
        #region Init/Layout
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            panelCentral = new Panel { Dock = DockStyle.Fill };
            Controls.Add(panelCentral);

            lblTitulo = new Label
            {
                Text = "Perfis de Usuário",
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold)
            };
            panelCentral.Controls.Add(lblTitulo);

            strip = new ToolStrip
            {
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.Transparent
            };

            btnRefresh = new ToolStripButton("🔄") { ToolTipText = "Atualizar" };
            btnRefresh.Click += async (_, _) => await CarregarAsync();

            btnNovo = new ToolStripButton("➕  Novo perfil");
            btnNovo.Click += async (_, _) => await AbrirCriacao();

            btnAssociar = new ToolStripButton("👤➕  Associar usuário");
            btnAssociar.Click += async (_, _) => await AbrirAssociacao();

            strip.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, btnNovo, btnAssociar,
                new ToolStripSeparator(), new ToolStripLabel("Pesquisar:")
            });

            txtFiltro = new ToolStripTextBox { Width = 200 };
            txtFiltro.TextChanged += (_, _) => Filtrar();
            strip.Items.Add(txtFiltro);

            btnFiltrar = new ToolStripButton("🔍");
            btnFiltrar.Click += (_, _) => Filtrar();
            strip.Items.Add(btnFiltrar);

            panelCentral.Controls.Add(strip);

            dgv = new DoubleBufferedGrid
            {
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
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

            status = new StatusStrip { BackColor = Color.Gainsboro };
            lblTotal = new ToolStripStatusLabel("0 perfil(is)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            pnlLoad = new Panel { BackColor = Color.FromArgb(140, Color.White), Visible = false };
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
            int w = Math.Min(700, panelCentral.ClientSize.Width - 40);

            dgv.Bounds = new Rectangle(cx - w / 2, top, w, h < 300 ? 300 : h);
        }
        #endregion

        /* =====================  GRID  ==================================== */
        private void CriarColunas()
        {
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { DataPropertyName = "id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Perfil", HeaderText = "Perfil", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewButtonColumn { Text = "Detalhes", UseColumnTextForButtonValue = true, Width = 70 },
                new DataGridViewButtonColumn { Text = "Editar", UseColumnTextForButtonValue = true, Width = 50 },
                new DataGridViewButtonColumn { Text = "Excluir", UseColumnTextForButtonValue = true, Width = 60 }
            );
            dgv.CellClick += Dgv_CellClick;
        }

        /* ===================== carga & filtro ============================ */
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _perfis.Clear();

                var lista = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<PerfilResponse>>(
                        "api/Perfil/GetAll",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                lista?.ForEach(_perfis.Add);

                dgv.DataSource = _perfis;
                lblTotal.Text = $"{_perfis.Count} perfil(is)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar perfis: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { ToggleLoading(false); }
        }

        private void Filtrar()
        {
            string termo = txtFiltro.Text.Trim().ToLower();
            var dados = string.IsNullOrWhiteSpace(termo)
                ? _perfis
                : _perfis.Where(p => (p.Perfil ?? "").ToLower().Contains(termo));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} perfil(is)";
        }

        /* ===================  grid actions =============================== */
        private void Dgv_CellClick(object? _, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex] is not DataGridViewButtonColumn btn) return;

            var p = (PerfilResponse)dgv.Rows[e.RowIndex].DataBoundItem;

            switch (btn.Text)
            {
                case "Detalhes": MostrarDetalhes(p); break;
                case "Editar": AbrirEdicao(p); break;
                case "Excluir": ExcluirRegistro(p); break;
            }
        }

        #region Criar perfil
        private async Task AbrirCriacao()
        {
            using var f = NovaTela("Novo perfil");

            var lblNome = new Label { Text = "Nome do perfil:", Left = 20, Top = 30 };
            var txtNome = new TextBox { Left = 20, Top = 55, Width = 300 };

            var btnSave = new Button { Text = "Salvar", Left = 120, Top = 100, Width = 80 };
            var btnCancel = new Button { Text = "Cancelar", Left = 210, Top = 100, Width = 80, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[] { lblNome, txtNome, btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                var dto = new PerfilDTO { Perfil = txtNome.Text.Trim() };

                if (!Validar(dto)) return;

                try
                {
                    var rsp = await SessaoWinForms.Http.PostAsJsonAsync("api/Perfil/Create", dto);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Perfil criado com sucesso!",
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

        #region Associar perfil a usuário (agora com ComboBox de e-mails)
        private async Task AbrirAssociacao()
        {
            if (_perfis.Count == 0) await CarregarAsync();
            if (_usuarios.Count == 0) _usuarios = await ObterUsuarios();

            using var f = NovaTela("Associar perfil a usuário");

            var lblPerfil = new Label { Text = "Perfil:", Left = 20, Top = 25 };
            var cmbPerfil = new ComboBox
            {
                Left = 20,
                Top = 45,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _perfis,
                DisplayMember = "Perfil",
                ValueMember = "Perfil"
            };

            var lblUser = new Label { Text = "Usuário (e-mail):", Left = 20, Top = 85 };
            var cmbUser = new ComboBox
            {
                Left = 20,
                Top = 105,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _usuarios,
                DisplayMember = "Email",
                ValueMember = "Email"
            };

            var btnSave = new Button { Text = "Associar", Left = 110, Top = 150, Width = 80 };
            var btnCancel = new Button { Text = "Cancelar", Left = 195, Top = 150, Width = 80, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[] { lblPerfil, cmbPerfil, lblUser, cmbUser, btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                var dto = new PerfilDTO
                {
                    Perfil = cmbPerfil.SelectedValue?.ToString(),
                    Email = cmbUser.SelectedValue?.ToString()
                };

                if (!Validar(dto)) return;

                try
                {
                    var rsp = await SessaoWinForms.Http.PostAsJsonAsync("api/Perfil/Associar", dto);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Perfil associado ao usuário com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private static void MostrarDetalhes(PerfilResponse p)
        {
            MessageBox.Show($"ID: {p.id}\n\nPerfil: {p.Perfil}",
                            "Detalhes do perfil", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AbrirEdicao(PerfilResponse p)
        {
            using var f = NovaTela($"Editar perfil {p.id}");

            var lblNome = new Label { Text = "Nome do perfil:", Left = 20, Top = 30 };
            var txtNome = new TextBox { Left = 20, Top = 55, Width = 300, Text = p.Perfil };

            var btnSave = new Button { Text = "Salvar", Left = 120, Top = 100, Width = 80 };
            var btnCancel = new Button { Text = "Cancelar", Left = 210, Top = 100, Width = 80, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[] { lblNome, txtNome, btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                p.Perfil = txtNome.Text.Trim();

                if (!Validar(p)) return;

                try
                {
                    var rsp = await SessaoWinForms.Http.PutAsJsonAsync("api/Perfil/Update", p);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Perfil atualizado com sucesso!",
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
        private async void ExcluirRegistro(PerfilResponse p)
        {
            if (MessageBox.Show($"Confirma excluir o perfil \"{p.Perfil}\"?",
                                "Confirmação", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var rsp = await SessaoWinForms.Http.DeleteAsync($"api/Perfil/Delete/{p.id}");
                if (rsp.IsSuccessStatusCode)
                {
                    _perfis.Remove(p);
                    lblTotal.Text = $"{_perfis.Count} perfil(is)";
                    MessageBox.Show("Perfil excluído com sucesso!",
                                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show($"Falha ao excluir: {(int)rsp.StatusCode}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de comunicação: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        /* ====================  helpers / utils =========================== */
        private static bool Validar(object obj)
        {
            var ctx = new ValidationContext(obj);
            var res = new List<ValidationResult>();
            if (Validator.TryValidateObject(obj, ctx, res, true)) return true;

            MessageBox.Show(string.Join("\n", res.Select(r => r.ErrorMessage)),
                            "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

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
                var pb = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                MessageBox.Show(string.Join("\n", pb.Errors.SelectMany(kvp => kvp.Value)),
                                "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var txt = await resp.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ({(int)resp.StatusCode}):\n{txt}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Form NovaTela(string titulo) => new()
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = new Size(340, 260),
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }

        /* ----------- lista de usuários para combo ----------------------- */
        private static async Task<List<UsuarioResponse>> ObterUsuarios()
            => await SessaoWinForms.Http
                     .GetFromJsonAsync<List<UsuarioResponse>>(
                         "api/Usuarios/GetAll",
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<UsuarioResponse>();
    }
}
