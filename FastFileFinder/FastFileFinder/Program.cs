// FastFileFinder - Windows front-end for fastfilefinder_scan.py
// .NET Framework 4.8 / C# 7.3

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastFileFinder
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class SearchRow
    {
        public string Path { get; set; }
        public string Entry { get; set; }
        public int LineNo { get; set; }
        public string Snippet { get; set; }
    }

    public class MainForm : Form
    {
        private const int BatchSize = 1000;
        private const int MaxErrorLines = 100;

        private readonly TextBox txtRoot;
        private readonly TextBox txtQuery;
        private readonly TextBox txtExts;
        private readonly TextBox txtPython;
        private readonly TextBox txtPerFile;
        private readonly TextBox txtQuickFilter;
        private readonly CheckBox chkRegex;
        private readonly CheckBox chkZip;
        private readonly CheckBox chkRecursive;
        private readonly CheckBox chkWord;
        private readonly CheckBox chkExcel;
        private readonly CheckBox chkLegacy;
        private readonly NumericUpDown numMaxWorkers;
        private readonly Button btnSearch;
        private readonly Button btnCancel;
        private readonly Button btnCsv;
        private readonly Label lblElapsed;
        private readonly Label lblFiles;
        private readonly Label lblHits;
        private readonly Label lblCurrent;
        private readonly DataGridView grid;
        private readonly Timer uiTimer;
        private readonly Timer filterTimer;
        private readonly Timer batchTimer;
        private readonly ToolTip statusToolTip;

        private readonly List<SearchRow> allRows = new List<SearchRow>();
        private readonly List<SearchRow> viewRows = new List<SearchRow>();
        private readonly object rowsLock = new object();
        private readonly Queue<string> errorBuffer = new Queue<string>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private readonly Color rowBack = Color.White;
        private readonly Color rowAlt = Color.FromArgb(0xF2, 0xF4, 0xF8);
        private readonly Color rowHover = Color.FromArgb(0xF5, 0xF9, 0xFF);
        private readonly Color rowSelected = Color.FromArgb(0xE5, 0xF1, 0xFB);
        private readonly Color highlightBack = Color.FromArgb(0xFF, 0xF2, 0xAB);

        private ConcurrentQueue<SearchRow> pendingRows = new ConcurrentQueue<SearchRow>();
        private Process currentProc;
        private bool searchRunning;
        private bool cancelRequested;
        private string filterText = string.Empty;
        private int hoverRow = -1;
        private Regex highlightRegex;
        private string highlightText = string.Empty;
        private bool highlightIsRegex;
        private int processedFiles;
        private int totalHitsReported;
        private string currentFileDisplay = string.Empty;
        private string statusMessage = string.Empty;
        private DateTime statusMessageExpire;
        private bool errorTooltipDirty;

        public MainForm()
        {
            Text = "FastFileFinder";
            Width = 1180;
            Height = 780;
            MinimumSize = new Size(960, 600);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;
            BackColor = Color.FromArgb(0xFA, 0xFA, 0xFA);
            Font = new Font("Meiryo UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);

            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    CancelSearch();
                    e.Handled = true;
                }
            };

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(0),
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(12, 10, 12, 4),
                BackColor = Color.White,
                AutoSize = true,
            };

            btnSearch = CreateToolbarButton("検索開始", Color.FromArgb(0x00, 0x78, 0xD4));
            btnSearch.Click += async (s, e) => await StartSearchAsync();
            toolbar.Controls.Add(btnSearch);

            btnCancel = CreateToolbarButton("キャンセル (Esc)", Color.FromArgb(0xA0, 0xA0, 0xA0));
            btnCancel.Enabled = false;
            btnCancel.Click += (s, e) => CancelSearch();
            toolbar.Controls.Add(btnCancel);

            btnCsv = CreateToolbarButton("CSV 出力", Color.FromArgb(0x4C, 0x4C, 0x4C));
            btnCsv.Click += (s, e) => ExportCsv();
            toolbar.Controls.Add(btnCsv);

            mainLayout.Controls.Add(toolbar, 0, 0);

            var group = new GroupBox
            {
                Text = "検索条件",
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(12, 8, 12, 12),
                BackColor = Color.White,
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 6,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            for (int i = 0; i < 6; i++)
            {
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            }

            layout.Controls.Add(CreateLabel("起点フォルダ"), 0, 0);
            txtRoot = CreateTextBox();
            layout.Controls.Add(txtRoot, 1, 0);
            layout.SetColumnSpan(txtRoot, 2);

            var btnBrowse = CreateFlatButton("参照");
            btnBrowse.Click += (s, e) => Browse();
            layout.Controls.Add(btnBrowse, 3, 0);

            chkRecursive = CreateCheckBox("サブフォルダも検索", true);
            layout.Controls.Add(chkRecursive, 4, 0);
            layout.SetColumnSpan(chkRecursive, 2);

            layout.Controls.Add(CreateLabel("内容フィルタ"), 0, 1);
            txtQuery = CreateTextBox();
            layout.Controls.Add(txtQuery, 1, 1);
            layout.SetColumnSpan(txtQuery, 2);

            chkRegex = CreateCheckBox("正規表現", false);
            layout.Controls.Add(chkRegex, 3, 1);

            chkZip = CreateCheckBox("ZIP 内も検索", true);
            layout.Controls.Add(chkZip, 4, 1);

            layout.Controls.Add(CreateLabel("対象拡張子 (;区切り)"), 0, 2);
            txtExts = CreateTextBox();
            layout.Controls.Add(txtExts, 1, 2);
            layout.SetColumnSpan(txtExts, 2);

            layout.Controls.Add(CreateLabel("1 ファイル上限"), 3, 2);
            txtPerFile = CreateTextBox();
            txtPerFile.Width = 80;
            layout.Controls.Add(txtPerFile, 4, 2);

            layout.Controls.Add(CreateLabel("並列度 (0=自動)"), 0, 3);
            numMaxWorkers = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 128,
                Value = 0,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4),
            };
            layout.Controls.Add(numMaxWorkers, 1, 3);

            layout.Controls.Add(CreateLabel("python.exe パス"), 3, 3);
            txtPython = CreateTextBox();
            layout.Controls.Add(txtPython, 4, 3);
            layout.SetColumnSpan(txtPython, 2);

            layout.Controls.Add(CreateLabel("Office 形式"), 0, 4);
            var flowOffice = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 4),
            };
            chkWord = CreateCheckBox("Word (.docx)", true);
            chkExcel = CreateCheckBox("Excel (.xlsx)", true);
            chkLegacy = CreateCheckBox("旧形式 (.doc/.xls)", false);
            flowOffice.Controls.Add(chkWord);
            flowOffice.Controls.Add(chkExcel);
            flowOffice.Controls.Add(chkLegacy);
            layout.Controls.Add(flowOffice, 1, 4);
            layout.SetColumnSpan(flowOffice, 5);

            layout.Controls.Add(CreateLabel("結果フィルタ"), 0, 5);
            txtQuickFilter = CreateTextBox();
            layout.Controls.Add(txtQuickFilter, 1, 5);
            layout.SetColumnSpan(txtQuickFilter, 5);

            group.Controls.Add(layout);
            mainLayout.Controls.Add(group, 0, 1);

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = true,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                VirtualMode = true,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
            };

            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(0x20, 0x20, 0x20);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.DefaultCellStyle.SelectionBackColor = rowSelected;
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.RowsDefaultCellStyle.BackColor = rowBack;
            grid.AlternatingRowsDefaultCellStyle.BackColor = rowAlt;
            grid.GridColor = Color.FromArgb(0xDD, 0xDD, 0xDD);
            grid.RowTemplate.Height = 26;

            EnableDoubleBuffer(grid);

            var colPath = new DataGridViewTextBoxColumn { HeaderText = "Path", Name = "colPath", FillWeight = 40f };
            var colEntry = new DataGridViewTextBoxColumn { HeaderText = "Entry", Name = "colEntry", FillWeight = 20f };
            var colLine = new DataGridViewTextBoxColumn { HeaderText = "Line", Name = "colLine", FillWeight = 10f };
            var colSnippet = new DataGridViewTextBoxColumn { HeaderText = "Snippet", Name = "colSnippet", FillWeight = 60f };
            grid.Columns.AddRange(colPath, colEntry, colLine, colSnippet);

            grid.CellValueNeeded += Grid_CellValueNeeded;
            grid.CellPainting += Grid_CellPainting;
            grid.CellMouseEnter += (s, e) => SetHoverRow(e.RowIndex);
            grid.CellMouseLeave += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    SetHoverRow(-1);
                }
            };
            grid.MouseLeave += (s, e) => SetHoverRow(-1);
            grid.CellDoubleClick += (s, e) => OpenSelection();
            grid.ColumnHeaderMouseClick += Grid_ColumnHeaderMouseClick;
            grid.KeyDown += Grid_KeyDown;
            grid.CellMouseDown += Grid_CellMouseDown;

            var menu = new ContextMenuStrip();
            menu.Items.Add("パスをコピー", null, (s, e) => CopyToClipboard(r => r.Path));
            menu.Items.Add("Entry をコピー", null, (s, e) => CopyToClipboard(r => r.Entry));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("エクスプローラーで開く", null, (s, e) => OpenSelection());
            menu.Items.Add("親フォルダを開く", null, (s, e) => OpenParent());
            grid.ContextMenuStrip = menu;

            mainLayout.Controls.Add(grid, 0, 2);

            var statusPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                AutoSize = true,
                Padding = new Padding(12, 4, 12, 12),
                BackColor = Color.White,
            };
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            lblElapsed = CreateStatusLabel("経過: 00:00:00");
            lblFiles = CreateStatusLabel("処理済み: 0 件");
            lblHits = CreateStatusLabel("ヒット: 0 件");
            lblCurrent = CreateStatusLabel("Ready");
            lblCurrent.AutoEllipsis = true;

            statusPanel.Controls.Add(lblElapsed, 0, 0);
            statusPanel.Controls.Add(lblFiles, 1, 0);
            statusPanel.Controls.Add(lblHits, 2, 0);
            statusPanel.Controls.Add(lblCurrent, 3, 0);

            statusToolTip = new ToolTip
            {
                AutomaticDelay = 150,
                AutoPopDelay = 15000,
                InitialDelay = 500,
                ReshowDelay = 150,
            };
            statusToolTip.SetToolTip(lblCurrent, string.Empty);

            var statusMenu = new ContextMenuStrip();
            statusMenu.Items.Add("エラー履歴をコピー", null, (s, e) => CopyErrors());
            lblCurrent.ContextMenuStrip = statusMenu;

            mainLayout.Controls.Add(statusPanel, 0, 3);

            Controls.Add(mainLayout);

            uiTimer = new Timer { Interval = 500 };
            uiTimer.Tick += (s, e) => UpdateStatusLabels();

            filterTimer = new Timer { Interval = 350 };
            filterTimer.Tick += (s, e) =>
            {
                filterTimer.Stop();
                ApplyFilter();
            };

            batchTimer = new Timer { Interval = 75 };
            batchTimer.Tick += (s, e) => DrainPendingRows(BatchSize);

            txtQuickFilter.TextChanged += (s, e) => filterTimer.Start();

            FormClosing += (s, e) =>
            {
                if (searchRunning)
                {
                    CancelSearch();
                    var proc = currentProc;
                    if (proc != null)
                    {
                        try
                        {
                            proc.WaitForExit(2000);
                        }
                        catch
                        {
                        }
                    }
                }
            };
        }

        private Button CreateToolbarButton(string text, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(0, 0, 8, 0),
                Padding = new Padding(16, 6, 16, 6),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(Font, FontStyle.Bold),
                TabStop = false,
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backColor);
            return btn;
        }

        private Button CreateFlatButton(string text)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0xEE, 0xEE, 0xEE),
                ForeColor = Color.FromArgb(0x20, 0x20, 0x20),
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(0xCC, 0xCC, 0xCC);
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        private Label CreateLabel(string text)
        {
            return new Label
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4),
            };
        }

        private CheckBox CreateCheckBox(string text, bool isChecked)
        {
            return new CheckBox
            {
                Text = text,
                Checked = isChecked,
                AutoSize = true,
                Margin = new Padding(6, 4, 6, 4),
                FlatStyle = FlatStyle.Flat,
            };
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4),
            };
        }

        private Label CreateStatusLabel(string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3, 0, 3, 0),
            };
        }

        private void EnableDoubleBuffer(DataGridView dgv)
        {
            var prop = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            prop?.SetValue(dgv, true, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                _ = StartSearchAsync();
                return true;
            }

            if (keyData == Keys.Escape)
            {
                CancelSearch();
                return true;
            }

            if (keyData == Keys.F5)
            {
                _ = StartSearchAsync();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async Task StartSearchAsync()
        {
            if (searchRunning)
            {
                return;
            }

            var root = txtRoot.Text.Trim();
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                MessageBox.Show("起点フォルダが正しくありません", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var query = txtQuery.Text;
            if (string.IsNullOrEmpty(query))
            {
                if (MessageBox.Show("内容フィルタが空です。全件走査しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            if (!int.TryParse(string.IsNullOrWhiteSpace(txtPerFile.Text) ? "0" : txtPerFile.Text.Trim(), out var perFile) || perFile < 0)
            {
                MessageBox.Show("1ファイル上限には 0 以上の整数を入力してください", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string pyPath = Path.Combine(exeDir, "fastfilefinder_scan.py");
            if (!File.Exists(pyPath))
            {
                MessageBox.Show("fastfilefinder_scan.py が見つかりません。実行ファイルと同じフォルダに配置してください。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pythonExe = string.IsNullOrWhiteSpace(txtPython.Text) ? "python" : txtPython.Text.Trim();

            var args = new List<string>
            {
                Quote(pyPath),
                "--folder",
                Quote(root),
                "--query",
                Quote(query),
            };

            if (chkRegex.Checked)
            {
                args.Add("--regex");
            }

            if (chkZip.Checked)
            {
                args.Add("--zip");
            }

            if (chkRecursive.Checked)
            {
                args.Add("--recursive");
            }

            if (chkWord.Checked)
            {
                args.Add("--word");
            }

            if (chkExcel.Checked)
            {
                args.Add("--excel");
            }

            if (chkLegacy.Checked)
            {
                args.Add("--legacy");
            }

            var exts = txtExts.Text.Trim();
            if (!string.IsNullOrEmpty(exts))
            {
                args.Add("--exts");
                args.Add(Quote(exts));
            }

            if (perFile > 0)
            {
                args.Add("--perfile");
                args.Add(perFile.ToString());
            }

            if (numMaxWorkers.Value > 0)
            {
                args.Add("--max-workers");
                args.Add(((int)numMaxWorkers.Value).ToString());
            }

            PrepareHighlight(query, chkRegex.Checked);

            lock (rowsLock)
            {
                allRows.Clear();
                viewRows.Clear();
                grid.RowCount = 0;
            }

            pendingRows = new ConcurrentQueue<SearchRow>();
            processedFiles = 0;
            totalHitsReported = 0;
            currentFileDisplay = string.Empty;
            statusMessage = string.Empty;
            hoverRow = -1;
            cancelRequested = false;
            filterText = txtQuickFilter.Text.Trim();
            errorBuffer.Clear();
            errorTooltipDirty = true;
            statusToolTip.SetToolTip(lblCurrent, string.Empty);

            btnSearch.Enabled = false;
            btnCancel.Enabled = true;
            searchRunning = true;
            stopwatch.Reset();
            stopwatch.Start();
            uiTimer.Start();
            batchTimer.Start();
            UpdateStatusLabels();

            currentProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = exeDir,
                },
                EnableRaisingEvents = true,
            };
            currentProc.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            currentProc.OutputDataReceived += CurrentProc_OutputDataReceived;
            currentProc.ErrorDataReceived += CurrentProc_ErrorDataReceived;
            currentProc.Exited += (s, e) => BeginInvoke((Action)SearchCompleted);

            try
            {
                currentProc.Start();
                currentProc.BeginOutputReadLine();
                currentProc.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                searchRunning = false;
                btnSearch.Enabled = true;
                btnCancel.Enabled = false;
                uiTimer.Stop();
                batchTimer.Stop();
                stopwatch.Reset();
                MessageBox.Show("python 実行エラー: " + ex.Message, "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                try
                {
                    currentProc?.Dispose();
                }
                catch
                {
                }
                currentProc = null;
                return;
            }

            await Task.CompletedTask;
        }

        private void CurrentProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            if (e.Data.StartsWith("#"))
            {
                BeginInvoke((Action)(() => HandleStatusLine(e.Data.Substring(1))));
                return;
            }

            var parts = e.Data.Split(new[] { '\t' }, 4);
            if (parts.Length < 4)
            {
                return;
            }

            int.TryParse(parts[2], out var lineNo);
            var row = new SearchRow
            {
                Path = parts[0],
                Entry = parts[1],
                LineNo = lineNo,
                Snippet = parts[3],
            };

            pendingRows.Enqueue(row);
        }

        private void CurrentProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            BeginInvoke((Action)(() =>
            {
                AppendError(e.Data);
                statusMessage = "⚠ " + e.Data;
                statusMessageExpire = DateTime.Now.AddSeconds(10);
                UpdateStatusLabels();
            }));
        }

        private void AppendError(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            errorBuffer.Enqueue(line);
            while (errorBuffer.Count > MaxErrorLines)
            {
                errorBuffer.Dequeue();
            }

            errorTooltipDirty = true;
        }

        private void HandleStatusLine(string line)
        {
            var parts = line.Split('\t');
            if (parts.Length == 0)
            {
                return;
            }

            switch (parts[0])
            {
                case "queued":
                    statusMessage = $"対象 {parts.ElementAtOrDefault(1) ?? "0"} ファイル";
                    statusMessageExpire = DateTime.Now.AddSeconds(10);
                    break;
                case "current":
                    currentFileDisplay = parts.ElementAtOrDefault(1) ?? string.Empty;
                    break;
                case "progress":
                    if (parts.Length > 1 && int.TryParse(parts[1], out var files))
                    {
                        processedFiles = files;
                    }
                    if (parts.Length > 2 && int.TryParse(parts[2], out var hits))
                    {
                        totalHitsReported = hits;
                    }
                    if (parts.Length > 3)
                    {
                        currentFileDisplay = parts[3];
                    }
                    break;
                case "done":
                    if (parts.Length > 1 && int.TryParse(parts[1], out var doneFiles))
                    {
                        processedFiles = doneFiles;
                    }
                    if (parts.Length > 2 && int.TryParse(parts[2], out var doneHits))
                    {
                        totalHitsReported = doneHits;
                    }
                    if (parts.Length > 3)
                    {
                        statusMessage = $"完了: {parts[3]} 秒";
                        statusMessageExpire = DateTime.Now.AddSeconds(30);
                    }
                    break;
                default:
                    statusMessage = parts[0];
                    statusMessageExpire = DateTime.Now.AddSeconds(10);
                    break;
            }

            UpdateStatusLabels();
        }

        private void DrainPendingRows(int limit)
        {
            if (pendingRows == null)
            {
                return;
            }

            var batch = new List<SearchRow>(Math.Max(16, Math.Min(limit <= 0 ? BatchSize : limit, BatchSize)));
            while ((limit <= 0 || batch.Count < limit) && pendingRows.TryDequeue(out var row))
            {
                batch.Add(row);
            }

            if (batch.Count == 0)
            {
                return;
            }

            int addedToView = 0;
            int startIndex = 0;
            int newCount = 0;
            int totalRows = 0;

            lock (rowsLock)
            {
                foreach (var row in batch)
                {
                    allRows.Add(row);
                    if (IsVisibleByFilter(row))
                    {
                        viewRows.Add(row);
                        addedToView++;
                    }
                }

                totalRows = allRows.Count;
                newCount = viewRows.Count;
                if (addedToView > 0)
                {
                    startIndex = newCount - addedToView;
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                    }
                }
            }

            if (addedToView > 0)
            {
                grid.RowCount = newCount;
                for (int i = startIndex; i < newCount; i++)
                {
                    if (i >= 0 && i < grid.RowCount)
                    {
                        grid.InvalidateRow(i);
                    }
                }
            }

            totalHitsReported = Math.Max(totalHitsReported, totalRows);
            UpdateStatusLabels();
        }

        private void ApplyFilter()
        {
            filterText = txtQuickFilter.Text.Trim();
            lock (rowsLock)
            {
                viewRows.Clear();
                foreach (var row in allRows)
                {
                    if (IsVisibleByFilter(row))
                    {
                        viewRows.Add(row);
                    }
                }
                grid.RowCount = viewRows.Count;
            }
            UpdateStatusLabels();
            grid.Invalidate();
        }

        private bool IsVisibleByFilter(SearchRow row)
        {
            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            return (row.Path?.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (row.Entry?.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0
                || (row.Snippet?.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        private void PrepareHighlight(string query, bool isRegex)
        {
            highlightIsRegex = isRegex;
            highlightRegex = null;
            highlightText = string.Empty;
            if (isRegex)
            {
                try
                {
                    highlightRegex = new Regex(query, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch
                {
                    highlightRegex = null;
                }
            }
            else
            {
                highlightText = query ?? string.Empty;
            }
        }

        private void CancelSearch()
        {
            if (!searchRunning)
            {
                return;
            }

            cancelRequested = true;
            btnCancel.Enabled = false;
            statusMessage = "キャンセルしています...";
            statusMessageExpire = DateTime.Now.AddSeconds(5);
            UpdateStatusLabels();

            var proc = currentProc;
            if (proc == null)
            {
                return;
            }

            try
            {
                proc.CancelOutputRead();
            }
            catch
            {
            }

            try
            {
                proc.CancelErrorRead();
            }
            catch
            {
            }

            Task.Run(() =>
            {
                try
                {
                    if (!proc.HasExited)
                    {
                        proc.CloseMainWindow();
                        if (!proc.WaitForExit(1500))
                        {
                            proc.Kill();
                        }
                    }
                }
                catch
                {
                }
            });
        }

        private void SearchCompleted()
        {
            batchTimer.Stop();
            DrainPendingRows(0);
            uiTimer.Stop();
            stopwatch.Stop();
            searchRunning = false;
            btnSearch.Enabled = true;
            btnCancel.Enabled = false;

            var proc = currentProc;
            if (proc != null)
            {
                proc.OutputDataReceived -= CurrentProc_OutputDataReceived;
                proc.ErrorDataReceived -= CurrentProc_ErrorDataReceived;
                try
                {
                    proc.Dispose();
                }
                catch
                {
                }
                currentProc = null;
            }

            if (cancelRequested)
            {
                statusMessage = "中断しました";
                statusMessageExpire = DateTime.Now.AddSeconds(10);
            }
            else
            {
                statusMessage = "完了";
                statusMessageExpire = DateTime.Now.AddSeconds(10);
            }

            lock (rowsLock)
            {
                totalHitsReported = Math.Max(totalHitsReported, allRows.Count);
            }

            UpdateStatusLabels();
        }

        private void UpdateStatusLabels()
        {
            lblElapsed.Text = "経過: " + stopwatch.Elapsed.ToString("hh\\:mm\\:ss");
            lblFiles.Text = $"処理済み: {processedFiles:N0} 件";

            int visibleCount;
            int totalCount;
            lock (rowsLock)
            {
                visibleCount = viewRows.Count;
                totalCount = allRows.Count;
            }

            lblHits.Text = $"ヒット: {Math.Max(totalHitsReported, totalCount):N0} 件 (表示: {visibleCount:N0})";

            string display = string.Empty;
            if (!string.IsNullOrEmpty(currentFileDisplay))
            {
                display = TruncatePath(currentFileDisplay, 80);
            }

            if (!string.IsNullOrEmpty(statusMessage) && statusMessageExpire > DateTime.Now)
            {
                if (string.IsNullOrEmpty(display))
                {
                    display = statusMessage;
                }
                else
                {
                    display += " | " + statusMessage;
                }
            }
            else if (statusMessageExpire <= DateTime.Now)
            {
                statusMessage = string.Empty;
            }

            lblCurrent.Text = string.IsNullOrEmpty(display) ? (searchRunning ? "処理中..." : "Ready") : display;

            if (errorTooltipDirty)
            {
                if (errorBuffer.Count > 0)
                {
                    var arr = errorBuffer.ToArray();
                    var recent = arr.Skip(Math.Max(0, arr.Length - 10)).ToArray();
                    statusToolTip.SetToolTip(lblCurrent, string.Join(Environment.NewLine, recent));
                }
                else
                {
                    statusToolTip.SetToolTip(lblCurrent, string.Empty);
                }
                errorTooltipDirty = false;
            }
        }

        private string TruncatePath(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }
            int keep = max - 3;
            if (keep <= 0)
            {
                return value.Substring(0, max);
            }
            int head = keep / 2;
            int tail = keep - head;
            return value.Substring(0, head) + "..." + value.Substring(value.Length - tail);
        }

        private void Grid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            SearchRow row;
            lock (rowsLock)
            {
                if (e.RowIndex >= viewRows.Count)
                {
                    return;
                }

                row = viewRows[e.RowIndex];
            }

            switch (grid.Columns[e.ColumnIndex].Name)
            {
                case "colPath":
                    e.Value = row.Path;
                    break;
                case "colEntry":
                    e.Value = row.Entry;
                    break;
                case "colLine":
                    e.Value = row.LineNo;
                    break;
                case "colSnippet":
                    e.Value = row.Snippet;
                    break;
            }
        }

        private void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                e.CellStyle.BackColor = Color.White;
                e.CellStyle.ForeColor = Color.FromArgb(0x20, 0x20, 0x20);
                return;
            }

            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            bool hover = !selected && e.RowIndex == hoverRow;
            var background = selected ? rowSelected : hover ? rowHover : (e.RowIndex % 2 == 0 ? rowBack : rowAlt);

            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }

            e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);

            if (grid.Columns[e.ColumnIndex].Name != "colSnippet")
            {
                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }

            var text = Convert.ToString(e.FormattedValue) ?? string.Empty;
            DrawHighlightedText(e.Graphics, text, e.CellBounds, e.CellStyle.Font, e.CellStyle.ForeColor);
            e.Handled = true;
        }

        private void DrawHighlightedText(Graphics g, string text, Rectangle bounds, Font font, Color color)
        {
            var rect = new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8);
            int x = rect.Left;
            int right = rect.Right;
            var flags = TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping;

            if (string.IsNullOrEmpty(text) || (highlightIsRegex && (highlightRegex == null || !highlightRegex.IsMatch(text))) || (!highlightIsRegex && string.IsNullOrEmpty(highlightText)))
            {
                TextRenderer.DrawText(g, text, font, rect, color, flags | TextFormatFlags.EndEllipsis);
                return;
            }

            if (highlightIsRegex)
            {
                var match = highlightRegex?.Match(text);
                if (match == null || !match.Success)
                {
                    TextRenderer.DrawText(g, text, font, rect, color, flags | TextFormatFlags.EndEllipsis);
                    return;
                }
                DrawSegment(g, text.Substring(0, match.Index), font, color, rect, ref x, right, flags);
                DrawSegment(g, match.Value, font, Color.Black, rect, ref x, right, flags, true);
                DrawSegment(g, text.Substring(match.Index + match.Length), font, color, rect, ref x, right, flags);
                return;
            }

            int pos = 0;
            var search = highlightText;
            while (pos < text.Length)
            {
                int idx = text.IndexOf(search, pos, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                {
                    DrawSegment(g, text.Substring(pos), font, color, rect, ref x, right, flags);
                    break;
                }
                if (idx > pos)
                {
                    DrawSegment(g, text.Substring(pos, idx - pos), font, color, rect, ref x, right, flags);
                }
                DrawSegment(g, text.Substring(idx, search.Length), font, Color.Black, rect, ref x, right, flags, true);
                pos = idx + search.Length;
                if (x >= right)
                {
                    break;
                }
            }
        }

        private void DrawSegment(Graphics g, string text, Font font, Color color, Rectangle rect, ref int x, int right, TextFormatFlags flags, bool highlight = false)
        {
            if (string.IsNullOrEmpty(text) || x >= right)
            {
                return;
            }
            var size = TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, rect.Height), flags);
            int width = Math.Min(size.Width, right - x);
            var segmentRect = new Rectangle(x, rect.Top, width, rect.Height);
            if (highlight)
            {
                using (var b = new SolidBrush(highlightBack))
                {
                    g.FillRectangle(b, segmentRect);
                }
            }
            TextRenderer.DrawText(g, text, font, segmentRect, color, flags);
            x += width;
        }

        private void SetHoverRow(int rowIndex)
        {
            if (hoverRow == rowIndex)
            {
                return;
            }
            int previous = hoverRow;
            hoverRow = rowIndex;
            if (previous >= 0 && previous < grid.RowCount)
            {
                grid.InvalidateRow(previous);
            }
            if (hoverRow >= 0 && hoverRow < grid.RowCount)
            {
                grid.InvalidateRow(hoverRow);
            }
        }

        private void Grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var column = grid.Columns[e.ColumnIndex];
            Sort(column.Name, Control.ModifierKeys.HasFlag(Keys.Shift));
        }

        private void Sort(string columnName, bool descending)
        {
            lock (rowsLock)
            {
                Comparison<SearchRow> comparison = null;
                switch (columnName)
                {
                    case "colPath":
                        comparison = (a, b) => string.Compare(a.Path, b.Path, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "colEntry":
                        comparison = (a, b) => string.Compare(a.Entry, b.Entry, StringComparison.OrdinalIgnoreCase);
                        break;
                    case "colLine":
                        comparison = (a, b) => a.LineNo.CompareTo(b.LineNo);
                        break;
                    case "colSnippet":
                        comparison = (a, b) => string.Compare(a.Snippet, b.Snippet, StringComparison.OrdinalIgnoreCase);
                        break;
                }

                if (comparison == null)
                {
                    return;
                }

                viewRows.Sort((a, b) => descending ? -comparison(a, b) : comparison(a, b));
            }
            grid.Invalidate();
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopyRows();
                e.Handled = true;
            }
        }

        private void CopyRows()
        {
            if (grid.SelectedRows.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (DataGridViewRow row in grid.SelectedRows)
            {
                var data = GetRow(row.Index);
                if (data == null)
                {
                    continue;
                }
                sb.AppendLine(string.Join("\t", data.Path, data.Entry, data.LineNo.ToString(), data.Snippet));
            }
            if (sb.Length > 0)
            {
                try
                {
                    Clipboard.SetText(sb.ToString());
                    statusMessage = "クリップボードにコピーしました";
                    statusMessageExpire = DateTime.Now.AddSeconds(5);
                    UpdateStatusLabels();
                }
                catch
                {
                }
            }
        }

        private void CopyToClipboard(Func<SearchRow, string> selector)
        {
            var row = GetRow(grid.CurrentRow?.Index ?? -1);
            if (row == null)
            {
                return;
            }
            var value = selector(row);
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            try
            {
                Clipboard.SetText(value);
                statusMessage = "コピーしました";
                statusMessageExpire = DateTime.Now.AddSeconds(5);
                UpdateStatusLabels();
            }
            catch
            {
            }
        }

        private void CopyErrors()
        {
            if (errorBuffer.Count == 0)
            {
                MessageBox.Show("エラー履歴はありません", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, errorBuffer.ToArray()));
                statusMessage = "エラー履歴をコピーしました";
                statusMessageExpire = DateTime.Now.AddSeconds(5);
                UpdateStatusLabels();
            }
            catch
            {
            }
        }

        private void Grid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                grid.ClearSelection();
                grid.Rows[e.RowIndex].Selected = true;
                grid.CurrentCell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex >= 0 ? e.ColumnIndex : 0];
            }
        }

        private SearchRow GetRow(int index)
        {
            if (index < 0)
            {
                return null;
            }
            lock (rowsLock)
            {
                if (index >= viewRows.Count)
                {
                    return null;
                }
                return viewRows[index];
            }
        }

        private void OpenSelection()
        {
            var row = GetRow(grid.CurrentRow?.Index ?? -1);
            if (row == null)
            {
                return;
            }
            try
            {
                if (!string.IsNullOrEmpty(row.Path) && File.Exists(row.Path))
                {
                    Process.Start("explorer.exe", $"/select,\"{row.Path}\"");
                }
            }
            catch
            {
            }
        }

        private void OpenParent()
        {
            var row = GetRow(grid.CurrentRow?.Index ?? -1);
            if (row == null)
            {
                return;
            }
            try
            {
                var dir = Path.GetDirectoryName(row.Path);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    Process.Start(dir);
                }
            }
            catch
            {
            }
        }

        private void ExportCsv()
        {
            List<SearchRow> rows;
            lock (rowsLock)
            {
                rows = new List<SearchRow>(viewRows);
            }

            if (rows.Count == 0)
            {
                MessageBox.Show("出力する行がありません", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var sfd = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv|TSV (*.tsv)|*.tsv|All Files (*.*)|*.*",
                FileName = "FastFileFinder.csv",
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                try
                {
                    var isTsv = string.Equals(Path.GetExtension(sfd.FileName), ".tsv", StringComparison.OrdinalIgnoreCase);
                    using (var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                    {
                        if (isTsv)
                        {
                            foreach (var r in rows)
                            {
                                sw.WriteLine(string.Join("\t", r.Path, r.Entry, r.LineNo.ToString(), r.Snippet));
                            }
                        }
                        else
                        {
                            sw.WriteLine("Path,Entry,Line,Snippet");
                            foreach (var r in rows)
                            {
                                sw.WriteLine(string.Join(",", Csv(r.Path), Csv(r.Entry), r.LineNo.ToString(), Csv(r.Snippet)));
                            }
                        }
                    }
                    statusMessage = "CSV/TSV を出力しました";
                    statusMessageExpire = DateTime.Now.AddSeconds(10);
                    UpdateStatusLabels();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("CSV 出力に失敗しました: " + ex.Message, "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string Csv(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        private void Browse()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (Directory.Exists(txtRoot.Text))
                {
                    dialog.SelectedPath = txtRoot.Text;
                }
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtRoot.Text = dialog.SelectedPath;
                }
            }
        }

        private static string Quote(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "\"\"";
            }
            if (s.IndexOf('"') >= 0)
            {
                s = s.Replace("\"", "\\\"");
            }
            if (s.IndexOf(' ') >= 0 || s.IndexOf('\t') >= 0 || s.IndexOf(';') >= 0)
            {
                return "\"" + s + "\"";
            }
            return s;
        }
    }
}

