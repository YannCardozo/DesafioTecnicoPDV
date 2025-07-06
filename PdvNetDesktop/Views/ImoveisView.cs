using General.Response.Imovel;
using PdvNetDesktop.Sessao;
using PdvNetDesktop.Utilitarios;
using System.Collections;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace PdvNetDesktop.Views
{
    public partial class ImoveisView : UserControl
    {
        /* ───── Fonte de dados ───── */
        private readonly BindingList<ImovelResponse> _imoveis = new();

        /* ───── Construtor ───────── */
        public ImoveisView()
        {
            InitializeComponent();
            _ = CarregarAsync();
        }

        /* ───── Designer & Layout ── */
        #region Designer
        private ToolStrip tool;
        private ToolStripButton btnRefresh, btnNovo;
        private ToolStripTextBox txtFiltro;
        private ToolStripLabel lblFiltro;

        private Panel panelCentral;
        private DataGridView dgv;

        private StatusStrip status;
        private ToolStripStatusLabel lblTotal;

        private Panel pnlLoad;
        private PictureBox picLoader;

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;

            /* ToolStrip */
            tool = new() { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };
            btnRefresh = new ToolStripButton("🔄") { ToolTipText = "Atualizar" };
            btnNovo = new ToolStripButton("➕") { ToolTipText = "Novo imóvel" };
            btnRefresh.Click += async (_, _) => await CarregarAsync();
            btnNovo.Click += (_, _) => MessageBox.Show("TODO: tela de cadastro");

            lblFiltro = new ToolStripLabel("Pesquisar:");
            txtFiltro = new ToolStripTextBox { Width = 200 };
            txtFiltro.TextChanged += (_, _) => Filtrar();

            tool.Items.AddRange(new ToolStripItem[]
                { btnRefresh, btnNovo, new ToolStripSeparator(), lblFiltro, txtFiltro });
            Controls.Add(tool);

            /* Painel central */
            panelCentral = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            Controls.Add(panelCentral);

            /* DataGridView */
            dgv = new DoubleBufferedGrid
            {
                Size = new Size(700, 380),
                ReadOnly = true,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                Anchor = AnchorStyles.None
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

            panelCentral.Resize += (_, _) =>
            {
                dgv.Left = (panelCentral.Width - dgv.Width) / 2;
                dgv.Top = (panelCentral.Height - dgv.Height) / 2;
            };

            /* StatusStrip */
            status = new StatusStrip { BackColor = Color.Gainsboro };
            lblTotal = new ToolStripStatusLabel("0 registro(s)");
            status.Items.Add(lblTotal);
            Controls.Add(status);

            /* Overlay loading */
            pnlLoad = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(140, Color.White),
                Visible = false
            };
            picLoader = new PictureBox
            {
                Image = Image.FromFile(Utilitarios.DiretorioPastas.ObterLoading()),
                SizeMode = PictureBoxSizeMode.CenterImage,
                Dock = DockStyle.Fill
            };
            pnlLoad.Controls.Add(picLoader);
            panelCentral.Controls.Add(pnlLoad);
        }

        private void CriarColunas()
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "id",
                HeaderText = "ID",
                Width = 60
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "tipo",
                HeaderText = "Tipo",
                Width = 120
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "endereco",
                HeaderText = "Endereço",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "status",
                HeaderText = "Status",
                Width = 90
            });

            dgv.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Detalhes",
                UseColumnTextForButtonValue = true,
                Width = 70,
                HeaderText = ""
            });
            dgv.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Editar",
                UseColumnTextForButtonValue = true,
                Width = 50,
                HeaderText = ""
            });
            dgv.Columns.Add(new DataGridViewButtonColumn
            {
                Text = "Excluir",
                UseColumnTextForButtonValue = true,
                Width = 60,
                HeaderText = ""
            });

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

        /* ───── Dados ───── */
        private async Task CarregarAsync()
        {
            ToggleLoading(true);
            try
            {
                _imoveis.Clear();
                var lista = await SessaoWinForms.Http.GetFromJsonAsync<List<ImovelResponse>>(
                                "api/Imovel/GetAll",
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (lista != null)
                    foreach (var i in lista) _imoveis.Add(i);

                dgv.DataSource = _imoveis;
                txtFiltro.Clear();
                lblTotal.Text = $"{_imoveis.Count} registro(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar imóveis: {ex.Message}");
            }
            finally
            {
                ToggleLoading(false);
            }
        }

        private void Filtrar()
        {
            string termo = txtFiltro.Text.Trim().ToLower();

            var dados = string.IsNullOrEmpty(termo)
                ? _imoveis
                : _imoveis.Where(i =>
                      (i.Endereco ?? "").ToLower().Contains(termo) ||
                      (i.Tipo ?? "").ToLower().Contains(termo) ||
                      (i.Status ?? "").ToLower().Contains(termo));

            dgv.DataSource = dados.ToList();
            lblTotal.Text = $"{((ICollection)dgv.DataSource).Count} registro(s)";
        }

        /* ───── Click das ações ───── */
        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var imovel = (ImovelResponse)dgv.Rows[e.RowIndex].DataBoundItem;
            string coluna = dgv.Columns[e.ColumnIndex].HeaderText;

            switch (coluna)
            {
                case "":
                    string botao = dgv.Columns[e.ColumnIndex].HeaderText ;

                    if (botao == "Detalhes") MostrarDetalhes(imovel);
                    else if (botao == "Editar") MostrarEditar(imovel);
                    else if (botao == "Excluir") ExcluirRegistro(imovel);
                    break;
            }
        }

        /* ───── Modais ───── */
        private static void MostrarDetalhes(ImovelResponse imv)
        {
            using var f = new Form
            {
                Text = $"Detalhes do imóvel {imv.id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(400, 260),
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
            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Dock = DockStyle.Bottom,
                Height = 35
            };
            f.Controls.Add(lbl);
            f.Controls.Add(btnOk);
            f.AcceptButton = btnOk;

            f.ShowDialog();
        }

        private async void MostrarEditar(ImovelResponse imv)
        {
            using var f = new Form
            {
                Text = $"Editar imóvel {imv.id}",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(400, 300),
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
                { new Label{Text="Tipo:",Left=20,Top=20}, txtTipo,
                  new Label{Text="Endereço:",Left=20,Top=80}, txtEnd,
                  new Label{Text="Status:",Left=20,Top=180}, cmbStat,
                  btnSave, btnCancel });

            btnSave.Click += async (_, _) =>
            {
                imv.Tipo = txtTipo.Text.Trim();
                imv.Endereco = txtEnd.Text.Trim();
                imv.Status = cmbStat.SelectedItem?.ToString() ?? imv.Status;

                try
                {
                    var r = await SessaoWinForms.Http.PutAsJsonAsync($"api/Imovel/{imv.id}", imv);
                    if (r.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Atualizado com sucesso!");
                        await CarregarAsync();
                        f.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Falha ao atualizar: {(int)r.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}");
                }
            };

            f.ShowDialog();
        }

        private async void ExcluirRegistro(ImovelResponse imv)
        {
            if (MessageBox.Show($"Confirma excluir imóvel {imv.id}?",
                                "Confirmação", MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    var resp = await SessaoWinForms.Http.DeleteAsync($"api/Imovel/{imv.id}");
                    if (resp.IsSuccessStatusCode)
                    {
                        _imoveis.Remove(imv);
                        lblTotal.Text = $"{_imoveis.Count} registro(s)";
                    }
                    else
                    {
                        MessageBox.Show($"Falha ao excluir: {(int)resp.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}");
                }
            }
        }

        /* ───── Utils ───── */
        private void ToggleLoading(bool on)
        {
            pnlLoad.Visible = on;
            pnlLoad.BringToFront();
            dgv.Enabled = tool.Enabled = !on;
        }

        private class DoubleBufferedGrid : DataGridView
        {
            public DoubleBufferedGrid() => DoubleBuffered = true;
        }
    }
}
