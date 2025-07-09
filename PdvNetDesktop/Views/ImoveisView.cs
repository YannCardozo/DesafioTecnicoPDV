/*  ImoveisView.cs  ───────────────────────────────────────────────────────── */
/*  - CRUD de imóveis + upload/preview de foto (Base-64)                     */
using General.DTO.Imovel;
using General.Response.Imovel;
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
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdvNetDesktop.Views
{
    public partial class ImoveisView : UserControl
    {
        /* ───────────────────────────────────────────────────────────────── */
        private readonly BindingList<ImovelResponse> _imoveis = new();

        public ImoveisView()
        {
            InitializeComponent();
            Load += async (_, _) => await CarregarAsync();
        }

        /* ───────── Campos de instância para imagens ───────── */
        private string? _imagemBase64Novo;   // usado em “Criar”
        private string? _imagemBase64Edit;   // usado em “Editar”

        /* ───────── Designer fields ───────── */
        #region Designer
        private ToolStrip strip;
        private ToolStripButton btnRefresh, btnNovo, btnFiltrar;
        private ToolStripTextBox txtFiltro;

        private Panel panelCentral;
        private DataGridView dgv;
        private Label lblTitulo;

        private StatusStrip status;
        private ToolStripStatusLabel lblTotal;

        private Panel pnlLoad;
        private PictureBox picLoader;
        #endregion

        /* ================================================================= */
        #region Init/UI
        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            panelCentral = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(panelCentral);

            lblTitulo = new Label
            {
                Text = "Lista de Imóveis",
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

            btnNovo = new ToolStripButton("➕  Novo") { ToolTipText = "Cadastrar novo imóvel" };
            btnNovo.Click += (_, _) => AbrirCriacao();

            strip.Items.AddRange(new ToolStripItem[]
            {
                btnRefresh, btnNovo, new ToolStripSeparator(),
                new ToolStripLabel("Pesquisar:")
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

            pnlLoad = new Panel
            {
                BackColor = Color.FromArgb(140, Color.White),
                Visible = false,
                Dock = DockStyle.Fill
            };
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
            int w = Math.Min(900, panelCentral.ClientSize.Width - 40);

            dgv.Bounds = new Rectangle(cx - w / 2, top, w, h < 300 ? 300 : h);
        }
        #endregion

        /* ================================================================= */
        #region Grid
        private void CriarColunas()
        {
            dgv.Columns.AddRange(
                new DataGridViewTextBoxColumn { DataPropertyName = "id", HeaderText = "ID", Width = 60 },
                new DataGridViewTextBoxColumn { DataPropertyName = "tipo", HeaderText = "Tipo", Width = 120 },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "endereco",
                    HeaderText = "Endereço",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                },
                new DataGridViewTextBoxColumn { DataPropertyName = "status", HeaderText = "Status", Width = 90 },
                new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "ValorLocacao",
                    HeaderText = "Locação",
                    Width = 90,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "C2" }
                },
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
        #endregion

        /* ================================================================= */
        #region Load & Filter
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _imoveis.Clear();
                var lista = await SessaoWinForms.Http
                    .GetFromJsonAsync<List<ImovelResponse>>(
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
        #endregion

        /* ================================================================= */
        #region Grid actions
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
        #endregion

        /* ================================================================= */
        #region CREATE
        private void AbrirCriacao()
        {
            _imagemBase64Novo = null;                   // zera

            using var f = NovaTela("Cadastrar imóvel");

            int y = 40;
            Control L(string t) => new Label { Text = t, Left = 20, Top = y - 18 };

            var txtTipo = new TextBox { Left = 20, Top = y, Width = 340 };
            f.Controls.AddRange(new Control[] { L("Tipo"), txtTipo });
            y += 55;

            var txtEnd = new TextBox { Left = 20, Top = y, Width = 340, Height = 60, Multiline = true };
            f.Controls.AddRange(new Control[] { L("Endereço"), txtEnd });
            y += 75;

            var cmbStat = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStat.Items.AddRange(new[] { "Disponível", "Alugado" });
            cmbStat.SelectedIndex = 0;
            f.Controls.AddRange(new Control[] { L("Status"), cmbStat });
            y += 55;

            /* Pré-view da foto */
            var picPrev = new PictureBox
            {
                Left = 20,
                Top = y,
                Size = new Size(120, 90),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            var btnImg = new Button
            {
                Text = "Selecionar foto…",
                Left = 150,
                Top = y + 30,
                Width = 110
            };
            btnImg.Click += (_, _) =>
            {
                using var dlg = new OpenFileDialog
                {
                    Title = "Escolher imagem",
                    Filter = "Arquivos de imagem|*.jpg;*.jpeg;*.png;*.bmp"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    picPrev.Image = Image.FromFile(dlg.FileName);
                    _imagemBase64Novo = Convert.ToBase64String(File.ReadAllBytes(dlg.FileName));
                    /* inclui cabeçalho mime simples */
                    _imagemBase64Novo = $"data:image/{Path.GetExtension(dlg.FileName)[1..]};base64,{_imagemBase64Novo}";
                }
            };
            f.Controls.AddRange(new Control[] { L("Foto"), picPrev, btnImg });
            y += 115;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = y, Width = 75 };
            var btnCancel = new Button { Text = "Cancelar", Left = 285, Top = y, Width = 75, DialogResult = DialogResult.Cancel };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                var dto = new ImovelDTO
                {
                    Tipo = txtTipo.Text.Trim(),
                    Endereco = txtEnd.Text.Trim(),
                    Status = cmbStat.SelectedItem?.ToString() ?? "",
                    ValorLocacao = 0m,
                    ImagemBase64 = _imagemBase64Novo
                };

                if (!Validar(dto)) return;

                try
                {
                    var resp = await SessaoWinForms.Http.PostAsJsonAsync("api/Imovel/Create", dto);
                    if (resp.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Imóvel criado com sucesso!",
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

        /* ================================================================= */
        #region DETAILS / UPDATE
        private static void MostrarDetalhes(ImovelResponse imv)
        {
            using var f = new Form
            {
                Text = $"Detalhes do imóvel {imv.id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ClientSize = new Size(420, 400),
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label
            {
                Dock = DockStyle.Top,
                Padding = new Padding(20),
                Font = new Font("Segoe UI", 10),
                Height = 180,
                Text =
                    $"ID:        {imv.id}\n\n" +
                    $"Tipo:      {imv.Tipo}\n\n" +
                    $"Endereço:  {imv.Endereco}\n\n" +
                    $"Status:    {imv.Status}\n\n" +
                    $"Locação:   {imv.ValorLocacao:C2}"
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrWhiteSpace(imv.ImagemBase64))
                pic.Image = Base64ToImage(imv.ImagemBase64);

            var btn = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 35 };
            f.Controls.AddRange(new Control[] { pic, lbl, btn });
            f.AcceptButton = btn;
            f.ShowDialog();
        }

        private void MostrarEditar(ImovelResponse imv)
        {
            _imagemBase64Edit = imv.ImagemBase64;  // valor inicial

            using var f = NovaTela($"Editar imóvel {imv.id}");

            int y = 40;
            Control L(string t) => new Label { Text = t, Left = 20, Top = y - 18 };

            var txtTipo = new TextBox { Text = imv.Tipo, Left = 20, Top = y, Width = 340 };
            f.Controls.AddRange(new Control[] { L("Tipo"), txtTipo });
            y += 55;

            var txtEnd = new TextBox
            {
                Text = imv.Endereco,
                Left = 20,
                Top = y,
                Width = 340,
                Height = 60,
                Multiline = true
            };
            f.Controls.AddRange(new Control[] { L("Endereço"), txtEnd });
            y += 75;

            var cmbStat = new ComboBox
            {
                Left = 20,
                Top = y,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbStat.Items.AddRange(new[] { "Disponível", "Alugado" });
            cmbStat.SelectedItem = imv.Status;
            f.Controls.AddRange(new Control[] { L("Status"), cmbStat });
            y += 55;

            var picPrev = new PictureBox
            {
                Left = 20,
                Top = y,
                Size = new Size(120, 90),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            if (!string.IsNullOrWhiteSpace(imv.ImagemBase64))
                picPrev.Image = Base64ToImage(imv.ImagemBase64);

            var btnImg = new Button
            {
                Text = "Trocar foto…",
                Left = 150,
                Top = y + 30,
                Width = 110
            };
            btnImg.Click += (_, _) =>
            {
                using var dlg = new OpenFileDialog
                {
                    Title = "Escolher imagem",
                    Filter = "Arquivos de imagem|*.jpg;*.jpeg;*.png;*.bmp"
                };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    picPrev.Image = Image.FromFile(dlg.FileName);
                    _imagemBase64Edit = Convert.ToBase64String(File.ReadAllBytes(dlg.FileName));
                    _imagemBase64Edit = $"data:image/{Path.GetExtension(dlg.FileName)[1..]};base64,{_imagemBase64Edit}";
                }
            };
            f.Controls.AddRange(new Control[] { L("Foto"), picPrev, btnImg });
            y += 115;

            var btnSave = new Button { Text = "Salvar", Left = 200, Top = y, Width = 75 };
            var btnCancel = new Button
            {
                Text = "Cancelar",
                Left = 285,
                Top = y,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };
            f.Controls.AddRange(new Control[] { btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                var dto = new ImovelResponse
                {
                    id = imv.id,
                    Tipo = txtTipo.Text.Trim(),
                    Endereco = txtEnd.Text.Trim(),
                    Status = cmbStat.SelectedItem?.ToString() ?? "",
                    ValorLocacao = imv.ValorLocacao,
                    ImagemBase64 = _imagemBase64Edit
                };

                if (!Validar(dto)) return;

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

        /* ================================================================= */
        #region DELETE
        private async void ExcluirRegistro(ImovelResponse imv)
        {
            if (MessageBox.Show($"Confirma excluir imóvel {imv.id}?",
                                "Confirmação", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                var resp = await SessaoWinForms.Http.DeleteAsync($"api/Imovel/Delete/{imv.id}");
                if (resp.IsSuccessStatusCode)
                {
                    _imoveis.Remove(imv);
                    lblTotal.Text = $"{_imoveis.Count} registro(s)";
                    MessageBox.Show("Imóvel excluído com sucesso!",
                                    "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show($"Falha ao excluir: {(int)resp.StatusCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }
        #endregion

        /* ================================================================= */
        #region Helpers
        private static bool Validar(object o)
        {
            var ctx = new ValidationContext(o);
            var res = new List<ValidationResult>();
            if (Validator.TryValidateObject(o, ctx, res, true)) return true;

            MessageBox.Show(string.Join("\n", res.Select(r => r.ErrorMessage)),
                            "Erros de validação",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private static Image Base64ToImage(string b64)
        {
            var data = b64.Contains(',') ? b64.Split(',')[1] : b64;
            byte[] bin = Convert.FromBase64String(data);
            using var ms = new MemoryStream(bin);
            return Image.FromStream(ms);
        }

        private void ToggleLoading(bool show)
        {
            // garante que roda sempre na UI thread
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(ToggleLoading), show);
                return;
            }

            pnlLoad.Visible = show;
            pnlLoad.BringToFront();
            panelCentral.Refresh();        // força um paint imediato

            dgv.Enabled = !show;
            strip.Enabled = !show;
            status.Enabled = !show;
        }

        private static async Task ExibirErrosDaApi(HttpResponseMessage resp)
        {
            if (resp.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                var pb = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
                MessageBox.Show(string.Join("\n", pb.Errors.SelectMany(kvp => kvp.Value)),
                                "Erros do servidor",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            ClientSize = new Size(420, 550),
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        private sealed class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }
        #endregion
    }
}
