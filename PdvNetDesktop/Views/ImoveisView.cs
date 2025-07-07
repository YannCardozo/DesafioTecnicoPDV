using General.Response.Imovel;
using PdvNetDesktop.Sessao;
using PdvNetDesktop.Utilitarios;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using General.DTO.Imovel;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace PdvNetDesktop.Views
{
    public partial class ImoveisView : UserControl
    {
        /* ───── Fonte de dados ───── */
        private readonly BindingList<ImovelResponse> _imoveis = new();

        public ImoveisView()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }

        /* ───── CONTROLES ───── */
        #region Designer fields
        private ToolStrip strip;
        private ToolStripButton btnRefresh;
        private ToolStripButton btnNovo;
        private ToolStripButton btnFiltrar;
        private ToolStripTextBox txtFiltro;

        private Panel panelCentral;
        private DataGridView dgv;
        private Label lblTitulo;

        private StatusStrip status;
        private ToolStripStatusLabel lblTotal;

        private Panel pnlLoad;
        private PictureBox picLoader;
        #endregion

        /* ───── UI & LAYOUT ───── */
        #region Init
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            /* Painel base */
            panelCentral = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(panelCentral);

            /* Título */
            lblTitulo = new Label
            {
                Text = "Lista de Imóveis",
                AutoSize = true,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.Black
            };
            panelCentral.Controls.Add(lblTitulo);

            /* ToolStrip (centralizado manualmente) */
            strip = new ToolStrip
            {
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.Transparent,
                CanOverflow = false
            };
            btnRefresh = new ToolStripButton("🔄") { ToolTipText = "Atualizar" };
            btnRefresh.Click += async (_, _) => await CarregarAsync();

            btnNovo = new ToolStripButton("➕  Novo") { ToolTipText = "Cadastrar novo imóvel" };
            btnNovo.Click += (_, _) => AbrirCriacao();

            strip.Items.Add(btnRefresh);
            strip.Items.Add(btnNovo);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(new ToolStripLabel("Pesquisar:"));

            txtFiltro = new ToolStripTextBox { Width = 220 };
            txtFiltro.TextChanged += (_, _) => Filtrar();
            strip.Items.Add(txtFiltro);

            btnFiltrar = new ToolStripButton("🔍") { ToolTipText = "Filtrar" };
            btnFiltrar.Click += (_, _) => Filtrar();
            strip.Items.Add(btnFiltrar);

            panelCentral.Controls.Add(strip);

            /* DataGrid */
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

            /* StatusStrip */
            status = new StatusStrip { BackColor = Color.Gainsboro, Dock = DockStyle.Bottom };
            lblTotal = new ToolStripStatusLabel("0 registro(s)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            /* Overlay Loading */
            pnlLoad = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(140, Color.White), Visible = false };
            picLoader = new PictureBox
            {
                Image = Image.FromFile(Utilitarios.DiretorioPastas.ObterLoading()),
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.CenterImage
            };
            pnlLoad.Controls.Add(picLoader);
            panelCentral.Controls.Add(pnlLoad);

            panelCentral.Resize += (_, _) => Reposicionar();
            Reposicionar();
        }

        private void Reposicionar()
        {
            int cx = panelCentral.ClientSize.Width / 2;

            // Barra
            strip.Location = new Point(cx - strip.PreferredSize.Width / 2, 10);

            // Título
            lblTitulo.Location = new Point(cx - lblTitulo.Width / 2, strip.Bottom + 10);

            // Grid
            int top = lblTitulo.Bottom + 10;
            int h = panelCentral.ClientSize.Height - top - 10;

            // <— aqui aumentamos de 700 para 900
            int maxGridWidth = 900;
            int w = Math.Min(maxGridWidth, panelCentral.ClientSize.Width - 40);

            dgv.Bounds = new Rectangle(
                cx - w / 2,
                top,
                w,
                h < 300 ? 300 : h
            );
        }
        #endregion

        /* ───── GRID ───── */
        private void CriarColunas()
        {
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { DataPropertyName = "id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "tipo", HeaderText = "Tipo", Width = 120 },
                new DataGridViewTextBoxColumn { DataPropertyName = "endereco", HeaderText = "Endereço", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { DataPropertyName = "status", HeaderText = "Status", Width = 90 },
                new DataGridViewTextBoxColumn { DataPropertyName = "DataCriacao", HeaderText = "Criado", Width = 90 },
                new DataGridViewTextBoxColumn { DataPropertyName = "DataAtualizacao", HeaderText = "Atualizado", Width = 90 },
                new DataGridViewButtonColumn { Text = "Detalhes", UseColumnTextForButtonValue = true, Width = 70 },
                new DataGridViewButtonColumn { Text = "Editar", UseColumnTextForButtonValue = true, Width = 50 },
                new DataGridViewButtonColumn { Text = "Excluir", UseColumnTextForButtonValue = true, Width = 60 }
            );

            dgv.CellFormatting += (s, e) =>
            {
                if (dgv.Columns[e.ColumnIndex].DataPropertyName == "status" &&
                    e.Value is string st)
                {
                    e.CellStyle.ForeColor = st.Equals("Disponível", StringComparison.OrdinalIgnoreCase)
                                            ? Color.Green
                                            : Color.Firebrick;
                }
            };

            dgv.CellClick += Dgv_CellClick;
        }

        /* ───── CARREGAR / FILTRAR ───── */
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _imoveis.Clear();
                var lista = await SessaoWinForms.Http.GetFromJsonAsync<List<ImovelResponse>>(
                                "api/Imovel/GetAll",
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                lista?.ForEach(_imoveis.Add);

                dgv.DataSource = _imoveis;
                lblTotal.Text = $"{_imoveis.Count} registro(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar imóveis: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { ToggleLoading(false); }
        }

        private void Filtrar()
        {
            string termo = txtFiltro.Text.Trim().ToLower();
            var dados = string.IsNullOrWhiteSpace(termo)
                ? _imoveis
                : _imoveis.Where(i =>
                       (i.Endereco ?? "").ToLower().Contains(termo) ||
                       (i.Tipo ?? "").ToLower().Contains(termo) ||
                       (i.Status ?? "").ToLower().Contains(termo));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} registro(s)";
        }

        /* ───── AÇÕES GRID ───── */
        private void Dgv_CellClick(object? _, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex] is not DataGridViewButtonColumn btn) return;

            var imv = (ImovelResponse)dgv.Rows[e.RowIndex].DataBoundItem;
            switch (btn.Text)
            {
                case "Detalhes": MostrarDetalhes(imv); break;
                case "Editar": MostrarEditar(imv); break;
                case "Excluir": ExcluirRegistro(imv); break;
            }
        }

        /* ───── CRUD COMPLETO (herdado do código antigo) ───── */

        #region Criar
        private void AbrirCriacao()
        {
            using var f = new Form
            {
                Text = "Cadastrar imóvel",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var txtTipo = new TextBox { Left = 20, Top = 40, Width = 340 };
            var txtEnd = new TextBox { Left = 20, Top = 100, Width = 340, Multiline = true, Height = 80 };
            var cmbStat = new ComboBox { Left = 20, Top = 200, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStat.Items.AddRange(new[] { "Disponível", "Alugado" });
            cmbStat.SelectedIndex = 0;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = 240, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = 240, Width = 75, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[]
            {
                new Label{Text="Tipo:",      Left=20, Top=20}, txtTipo,
                new Label{Text="Endereço:",  Left=20, Top=80}, txtEnd,
                new Label{Text="Status:",    Left=20, Top=180}, cmbStat,
                btnSave, btnCancel
            });

            btnSave.Click += async (_, _) =>
            {
                // 1) Monta um objeto fortemente tipado e valido pelas mesmas DataAnnotations do seu DTO
                var novoDto = new ImovelDTO
                {
                    Tipo = txtTipo.Text.Trim(),
                    Endereco = txtEnd.Text.Trim(),
                    Status = cmbStat.SelectedItem?.ToString() ?? "",
                    ValorLocacao = 0m
                };

                // 2) Validação cliente usando DataAnnotations
                var ctx = new ValidationContext(novoDto);
                var results = new List<ValidationResult>();
                bool valido = Validator.TryValidateObject(novoDto, ctx, results, true);
                if (!valido)
                {
                    // agrega todas as mensagens de erro e exibe
                    var msg = string.Join("\n", results.Select(r => r.ErrorMessage));
                    MessageBox.Show(msg, "Erros de validação",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3) Chama a API
                try
                {
                    var resp = await SessaoWinForms.Http.PostAsJsonAsync("api/Imovel/Create", novoDto);

                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Imóvel criado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else
                    {
                        // tenta desserializar um ValidationProblemDetails (ModelState)
                        if (resp.Content.Headers.ContentType?.MediaType == "application/problem+json")
                        {
                            var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                            var erros = problem.Errors
                                             .SelectMany(kvp => kvp.Value)
                                             .ToList();
                            MessageBox.Show(string.Join("\n", erros),
                                            "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            // cai aqui para string simples ou outra coisa
                            var texto = await resp.Content.ReadAsStringAsync();
                            MessageBox.Show($"Falha ao criar ({(int)resp.StatusCode}):\n{texto}",
                                            "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
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

        #region Detalhes / Editar / Excluir
        private static void MostrarDetalhes(ImovelResponse imv)
        {
            using var f = new Form
            {
                Text = $"Detalhes do imóvel {imv.id}",
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
                Text = $"ID: {imv.id}\n\nTipo: {imv.Tipo}\n\nEndereço:\n{imv.Endereco}\n\nStatus: {imv.Status}"
            };
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 35 };

            f.Controls.Add(lbl);
            f.Controls.Add(btnOk);
            f.AcceptButton = btnOk;

            f.ShowDialog();
        }

        private void MostrarEditar(ImovelResponse imv)
        {
            using var f = new Form
            {
                Text = $"Editar dados do imóvel {imv.id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var txtTipo = new TextBox { Text = imv.Tipo, Left = 20, Top = 40, Width = 340 };
            var txtEnd = new TextBox { Text = imv.Endereco, Left = 20, Top = 100, Width = 340, Multiline = true, Height = 80 };
            var cmbStat = new ComboBox { Left = 20, Top = 200, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStat.Items.AddRange(new[] { "Disponível", "Alugado" });
            cmbStat.SelectedItem = imv.Status;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = 240, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = 240, Width = 75, DialogResult = DialogResult.Cancel };

            f.Controls.AddRange(new Control[]
            {
                new Label{Text="Tipo:",     Left=20, Top=20},  txtTipo,
                new Label{Text="Endereço:", Left=20, Top=80},  txtEnd,
                new Label{Text="Status:",   Left=20, Top=180}, cmbStat,
                btnSave, btnCancel
            });

            btnSave.Click += async (_, _) =>
            {
                // 1) Preenche DTO a ser enviado
                var dto = new ImovelResponse
                {
                    id = imv.id,
                    Tipo = txtTipo.Text.Trim(),
                    Endereco = txtEnd.Text.Trim(),
                    Status = cmbStat.SelectedItem?.ToString() ?? "",
                    ValorLocacao = imv.ValorLocacao
                };

                // 2) Validação cliente por DataAnnotations
                var ctx = new ValidationContext(dto);
                var results = new List<ValidationResult>();
                if (!Validator.TryValidateObject(dto, ctx, results, true))
                {
                    var msg = string.Join("\n", results.Select(r => r.ErrorMessage));
                    MessageBox.Show(msg, "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3) Chama API de atualização
                try
                {
                    var resp = await SessaoWinForms.Http.PutAsJsonAsync("api/Imovel/Update", dto);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Imóvel atualizado com sucesso!",
                                        "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        await CarregarAsync();
                        f.Close();
                    }
                    else if (resp.Content.Headers.ContentType?.MediaType == "application/problem+json")
                    {
                        // tratta ValidationProblemDetails
                        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                        var erros = problem.Errors.SelectMany(kvp => kvp.Value);
                        MessageBox.Show(string.Join("\n", erros),
                                        "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        var texto = await resp.Content.ReadAsStringAsync();
                        MessageBox.Show($"Falha ao atualizar ({(int)resp.StatusCode}):\n{texto}",
                                        "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro de comunicação: {ex.Message}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            f.ShowDialog();
        }

        private async void ExcluirRegistro(ImovelResponse imv)
        {
            if (MessageBox.Show($"Confirma excluir imóvel {imv.id}?",
                                "Confirmação",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var resp = await SessaoWinForms.Http.DeleteAsync($"api/Imovel/Delete/{imv.id}");
                if (resp.IsSuccessStatusCode)
                {
                    _imoveis.Remove(imv);
                    lblTotal.Text = $"{_imoveis.Count} registro(s)";
                    MessageBox.Show(
                        "Imóvel excluído com sucesso!",
                        "Sucesso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else MessageBox.Show($"Falha ao excluir: {(int)resp.StatusCode}");
            }
            catch (Exception ex) { MessageBox.Show($"Erro: {ex.Message}"); }
        }
        #endregion

        /* ───── UTILS ───── */
        private void ToggleLoading(bool show)
        {
            pnlLoad.Visible = show;
            pnlLoad.BringToFront();
            dgv.Enabled =
            strip.Enabled =
            status.Enabled = !show;
        }

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }
    }
}
