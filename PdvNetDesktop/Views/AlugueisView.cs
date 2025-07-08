using General.DTO.Aluguel;
using General.Response.Aluguel;
using General.Response.Imovel;
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
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdvNetDesktop.Views
{
    public sealed partial class AluguelView : UserControl
    {
        /* classe adaptada para o grid (contém Imovel / Locatario resolvidos) */
        private sealed class AluguelGridRow : AluguelResponse
        {
            public string? Imovel { get; set; }
            public string? Locatario { get; set; }
        }

        /* ------------ dados em memória ----------------------------------- */
        private readonly BindingList<AluguelGridRow> _alugueis = new();
        private List<ImovelResponse> _imoveis = new();
        private List<UsuarioResponse> _locatarios = new();

        /* ------------ ctor ------------------------------------------------ */
        public AluguelView()
        {
            InitializeComponent();

            /* ↓ só carregamos o grid DEPOIS que imoveis e usuários chegarem */
            Load += async (_, _) =>
            {
                try
                {
                    _imoveis = await ObterImoveis();
                    _locatarios = await ObterLocatarios();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha ao baixar dados auxiliares: {ex.Message}",
                                    "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                await CarregarAsync();   // agora as listas estão populadas
            };
        }

        /* ------------ CONTROLES ------------------------------------------ */
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

        /* ================================================================== */
        #region Init & layout
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            panelCentral = new Panel { Dock = DockStyle.Fill };
            Controls.Add(panelCentral);

            lblTitulo = new Label
            {
                Text = "Lista de Aluguéis",
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

            btnNovo = new ToolStripButton("➕  Novo") { ToolTipText = "Registrar aluguel" };
            btnNovo.Click += async (_, _) => await AbrirCriacao();

            strip.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, btnNovo,
                new ToolStripSeparator(),
                new ToolStripLabel("Pesquisar:")
            });
            txtFiltro = new ToolStripTextBox { Width = 220 };
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
            lblTotal = new ToolStripStatusLabel("0 registro(s)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            pnlLoad = new Panel { BackColor = Color.FromArgb(140, Color.White), Visible = false, Dock = DockStyle.Fill };
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
            int w = Math.Min(1000, panelCentral.ClientSize.Width - 40);

            dgv.Bounds = new Rectangle(cx - w / 2, top, w, h < 300 ? 300 : h);
        }
        #endregion

        /* ================================================================== */
        #region Grid definition
        private void CriarColunas()
        {
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Imovel", HeaderText = "Imóvel", Width = 220 },
                new DataGridViewTextBoxColumn { DataPropertyName = "Locatario", HeaderText = "Locatário", Width = 180 },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "DataInicio",
                    HeaderText = "Início",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "d" }
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "DataTermino",
                    HeaderText = "Término",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "d" }
                },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ValorLocacao",
                    HeaderText = "Valor",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Format = "C2",
                        FormatProvider = new CultureInfo("pt-BR") // <-- aqui, R$ e vírgula decimal
                    }
                },
                new DataGridViewButtonColumn { Text = "Detalhes", UseColumnTextForButtonValue = true, Width = 70 },
                new DataGridViewButtonColumn { Text = "Editar", UseColumnTextForButtonValue = true, Width = 50 },
                new DataGridViewButtonColumn { Text = "Excluir", UseColumnTextForButtonValue = true, Width = 60 }
            );
            dgv.CellClick += Dgv_CellClick;
        }
        #endregion

        /* =====================  LOAD & FILTER ============================= */
        #region Carregar / Filtrar
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _alugueis.Clear();

                var lista = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<AluguelResponse>>(
                        "api/Aluguel/GetAll",
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lista != null)
                {
                    var dicImv = _imoveis.ToDictionary(i => i.id, i => i.Endereco);
                    var dicUsr = _locatarios.ToDictionary(u => u.id, u => u.Nome);

                    foreach (var a in lista)
                    {
                        _alugueis.Add(new AluguelGridRow
                        {
                            Id = a.Id,
                            ImovelId = a.ImovelId,
                            UsuarioId = a.UsuarioId,
                            ValorLocacao = a.ValorLocacao,
                            DataInicio = a.DataInicio,
                            DataTermino = a.DataTermino,
                            Imovel = dicImv.TryGetValue(a.ImovelId, out var end) ? end : $"#{a.ImovelId}",
                            Locatario = dicUsr.TryGetValue(a.UsuarioId, out var nm) ? nm : $"#{a.UsuarioId}"
                        });
                    }
                }

                dgv.DataSource = _alugueis;
                lblTotal.Text = $"{_alugueis.Count} registro(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar aluguéis: {ex.Message}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { ToggleLoading(false); }
        }

        private void Filtrar()
        {
            string t = txtFiltro.Text.Trim().ToLower();
            var dados = string.IsNullOrWhiteSpace(t)
                ? _alugueis
                : _alugueis.Where(a =>
                       (a.Imovel ?? "").ToLower().Contains(t) ||
                       (a.Locatario ?? "").ToLower().Contains(t));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} registro(s)";
        }
        #endregion

        /* =====================  GRID actions ============================== */
        private void Dgv_CellClick(object? _, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (dgv.Columns[e.ColumnIndex] is not DataGridViewButtonColumn b) return;

            var a = (AluguelGridRow)dgv.Rows[e.RowIndex].DataBoundItem;
            switch (b.Text)
            {
                case "Detalhes": MostrarDetalhes(a); break;
                case "Editar": _ = MostrarEditar(a); break;
                case "Excluir": ExcluirRegistro(a); break;
            }
        }

        /* =====================  CRUD: CREATE ============================== */
        #region Criar
        private async Task AbrirCriacao()
        {
            if (_imoveis.Count == 0) _imoveis = await ObterImoveis();
            if (_locatarios.Count == 0) _locatarios = await ObterLocatarios();

            using var f = NovaTela("Registrar aluguel");

            int y = 40;
            Control L(string t) => new Label { Text = t, Left = 20, Top = y - 18 };

            var cmbImovel = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _imoveis,
                DisplayMember = "Endereco",
                ValueMember = "id"
            };
            f.Controls.AddRange(new[] { L("Imóvel"), cmbImovel }); y += 55;

            var cmbLoc = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _locatarios,
                DisplayMember = "Nome",
                ValueMember = "id"
            };
            f.Controls.AddRange(new[] { L("Locatário"), cmbLoc }); y += 55;

            var dtIni = new DateTimePicker { Left = 20, Top = y, Width = 160, Format = DateTimePickerFormat.Short };
            f.Controls.AddRange(new[] { L("Data início"), dtIni }); y += 55;

            var dtFim = new DateTimePicker { Left = 20, Top = y, Width = 160, Format = DateTimePickerFormat.Short };
            f.Controls.AddRange(new[] { L("Data término"), dtFim }); y += 55;

            var txtValor = new TextBox { Left = 20, Top = y, Width = 120 };
            f.Controls.AddRange(new[] { L("Valor"), txtValor }); y += 75;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = y, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = y, Width = 75, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                if (!decimal.TryParse(txtValor.Text, out var valor) || valor <= 0)
                {
                    MessageBox.Show("Valor inválido.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dto = new AluguelDTO
                {
                    ImovelId = (int)cmbImovel.SelectedValue,
                    UsuarioId = (int)cmbLoc.SelectedValue,
                    DataInicio = dtIni.Value.Date,
                    DataTermino = dtFim.Value.Date,
                    ValorLocacao = valor
                };
                if (!Validar(dto)) return;

                try
                {
                    var rsp = await SessaoWinForms.Http.PostAsJsonAsync("api/Aluguel/Create", dto);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Aluguel registrado com sucesso!",
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

        /* ===============  CRUD: DETAILS & UPDATE  ========================= */
        #region Detalhes / Editar
        private static void MostrarDetalhes(AluguelGridRow a)
        {
            using var f = new Form
            {
                Text = $"Detalhes do aluguel {a.Id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(420, 280),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            string termino = a.DataTermino?.ToString("d") ?? "—";

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 10),
                Text =
                    $"ID:            {a.Id}\n\n" +
                    $"Imóvel:        {a.Imovel}\n\n" +
                    $"Locatário:     {a.Locatario}\n\n" +
                    $"Início:        {a.DataInicio:d}\n\n" +
                    $"Término:       {termino}\n\n" +
                    $"Valor locação: {a.ValorLocacao:C2}"
            };
            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 35 };
            f.Controls.AddRange(new Control[] { lbl, btnOk });
            f.AcceptButton = btnOk;
            f.ShowDialog();
        }

        private async Task MostrarEditar(AluguelGridRow a)
        {
            if (_imoveis.Count == 0) _imoveis = await ObterImoveis();
            if (_locatarios.Count == 0) _locatarios = await ObterLocatarios();

            using var f = NovaTela($"Editar aluguel {a.Id}");

            int y = 40;
            Control L(string t) => new Label { Text = t, Left = 20, Top = y - 18 };

            var cmbImovel = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _imoveis,
                DisplayMember = "Endereco",
                ValueMember = "id"
            };
            cmbImovel.SelectedValue = a.ImovelId;
            f.Controls.AddRange(new[] { L("Imóvel"), cmbImovel }); y += 55;

            var cmbLoc = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DataSource = _locatarios,
                DisplayMember = "Nome",
                ValueMember = "id"
            };
            cmbLoc.SelectedValue = a.UsuarioId;
            f.Controls.AddRange(new[] { L("Locatário"), cmbLoc }); y += 55;

            var dtIni = new DateTimePicker
            {
                Left = 20,
                Top = y,
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = a.DataInicio
            };
            f.Controls.AddRange(new[] { L("Data início"), dtIni }); y += 55;

            var dtFim = new DateTimePicker
            {
                Left = 20,
                Top = y,
                Width = 160,
                Format = DateTimePickerFormat.Short,
                Value = a.DataTermino ?? a.DataInicio
            };
            f.Controls.AddRange(new[] { L("Data término"), dtFim }); y += 55;

            var txtValor = new TextBox { Left = 20, Top = y, Width = 120, Text = a.ValorLocacao.ToString("F2") };
            f.Controls.AddRange(new[] { L("Valor"), txtValor }); y += 75;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = y, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = y, Width = 75, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                if (!decimal.TryParse(txtValor.Text, out var valor) || valor <= 0)
                {
                    MessageBox.Show("Valor inválido.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var dto = new AluguelResponse
                {
                    Id = a.Id,
                    ImovelId = (int)cmbImovel.SelectedValue,
                    UsuarioId = (int)cmbLoc.SelectedValue,
                    DataInicio = dtIni.Value.Date,
                    DataTermino = dtFim.Value.Date,
                    ValorLocacao = valor
                };
                if (!Validar(dto)) return;

                try
                {
                    var rsp = await SessaoWinForms.Http.PutAsJsonAsync("api/Aluguel/Update", dto);
                    if (rsp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Aluguel atualizado com sucesso!",
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

        /* ========================  DELETE  ================================ */
        #region Excluir
        private async void ExcluirRegistro(AluguelGridRow a)
        {
            if (MessageBox.Show($"Confirma excluir aluguel {a.Id}?",
                                "Confirmação", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var rsp = await SessaoWinForms.Http.DeleteAsync($"api/Aluguel/Delete/{a.Id}");
                if (rsp.IsSuccessStatusCode)
                {
                    _alugueis.Remove(a);
                    lblTotal.Text = $"{_alugueis.Count} registro(s)";
                    MessageBox.Show("Aluguel excluído com sucesso!",
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

        /* ===================== helpers & util ============================= */
        private static bool Validar(object o)
        {
            var ctx = new ValidationContext(o);
            var res = new List<ValidationResult>();
            if (Validator.TryValidateObject(o, ctx, res, true)) return true;

            MessageBox.Show(string.Join("\n", res.Select(r => r.ErrorMessage)),
                            "Erros de validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ToggleLoading(bool show)
        {
            pnlLoad.Visible = show;
            pnlLoad.BringToFront();
            dgv.Enabled = strip.Enabled = status.Enabled = !show;
        }

        private static async Task ExibirErrosDaApi(HttpResponseMessage rsp)
        {
            if (rsp.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var pb = await rsp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                MessageBox.Show(string.Join("\n", pb.Errors.SelectMany(kvp => kvp.Value)),
                                "Erros do servidor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                var txt = await rsp.Content.ReadAsStringAsync();
                MessageBox.Show($"Erro ({(int)rsp.StatusCode}):\n{txt}",
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Form NovaTela(string titulo) => new()
        {
            Text = titulo,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = new Size(460, 540),
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }

        /* ------------ APIs auxiliares ------------------------------------ */
        private static async Task<List<ImovelResponse>> ObterImoveis()
            => await SessaoWinForms.Http
                     .GetFromJsonAsync<List<ImovelResponse>>(
                         "api/Imovel/GetAll",
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<ImovelResponse>();

        private static async Task<List<UsuarioResponse>> ObterLocatarios()
            => await SessaoWinForms.Http
                     .GetFromJsonAsync<List<UsuarioResponse>>(
                         "api/Usuarios/GetAll",
                         new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? new List<UsuarioResponse>();
    }
}
