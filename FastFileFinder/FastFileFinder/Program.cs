// FastFileFinder (Python backend) - .NET Framework 4.8 / C# 7.3
// UIから python fastfilefinder_scan.py を起動し、標準出力(TSV)を逐次取り込み表示します。

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastFileFinderPy
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    public class SearchRow
    {
        public string Path { get; set; }
        public string Entry { get; set; } // zip内相対パス（通常ファイルは空）
        public int LineNo { get; set; }
        public string Snippet { get; set; }
    }

    public class MainForm : Form
    {
        TextBox txtRoot, txtQuery, txtExts, txtPython;
        CheckBox chkRegex, chkZip, chkRecursive;
        Button btnBrowse, btnSearch, btnCancel, btnCsv;
        Label lblStatus;

        DataGridView grid;
        BindingList<SearchRow> binding = new BindingList<SearchRow>();
        Process currentProc;
        CancellationTokenSource cts;

        public MainForm()
        {
            Text = "FastFileFinder (Python)";
            Width = 1100; Height = 720; StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(900, 520);
            KeyPreview = true;
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) CancelSearch(); };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 6,
                RowCount = 5,
                Padding = new Padding(8)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            for (int i = 0; i < 5; i++) panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

            // 1: 起点
            panel.Controls.Add(new Label { Text = "起点フォルダ", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            txtRoot = new TextBox { Dock = DockStyle.Fill };
            panel.Controls.Add(txtRoot, 1, 0);
            btnBrowse = new Button { Text = "参照" };
            btnBrowse.Click += (s, e) => Browse();
            panel.Controls.Add(btnBrowse, 2, 0);
            chkRecursive = new CheckBox { Text = "サブフォルダも検索", Checked = true, AutoSize = true };
            panel.Controls.Add(chkRecursive, 3, 0);

            // 2: 内容フィルタ
            panel.Controls.Add(new Label { Text = "内容フィルタ(文字列)", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 1);
            txtQuery = new TextBox { Dock = DockStyle.Fill };
            panel.Controls.Add(txtQuery, 1, 1);
            chkRegex = new CheckBox { Text = "正規表現", AutoSize = true };
            panel.Controls.Add(chkRegex, 2, 1);
            chkZip = new CheckBox { Text = "ZIP内も検索", Checked = true, AutoSize = true };
            panel.Controls.Add(chkZip, 3, 1);

            // 3: 拡張子
            panel.Controls.Add(new Label { Text = "対象拡張子(;区切り) 空=全て", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 2);
            txtExts = new TextBox { Dock = DockStyle.Fill, Text = "" };
            panel.Controls.Add(txtExts, 1, 2);

            // 4: Python 実行ファイル
            panel.Controls.Add(new Label { Text = "python.exe パス (空=python)", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 3);
            txtPython = new TextBox { Dock = DockStyle.Fill, Text = "" };
            panel.Controls.Add(txtPython, 1, 3);

            // 5: ボタン・ステータス
            btnSearch = new Button { Text = "検索開始", Dock = DockStyle.Fill };
            btnSearch.Click += async (s, e) => await StartSearchAsync();
            panel.Controls.Add(btnSearch, 4, 3);

            btnCancel = new Button { Text = "キャンセル(Esc)", Dock = DockStyle.Fill, Enabled = false };
            btnCancel.Click += (s, e) => CancelSearch();
            panel.Controls.Add(btnCancel, 5, 3);

            panel.Controls.Add(new Label { Text = "ステータス", TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 4);
            lblStatus = new Label { Text = "Ready", Dock = DockStyle.Fill, AutoEllipsis = true };
            panel.Controls.Add(lblStatus, 1, 4);

            btnCsv = new Button { Text = "CSV出力", Dock = DockStyle.Fill };
            btnCsv.Click += (s, e) => ExportCsv();
            panel.Controls.Add(btnCsv, 4, 4);

            Controls.Add(panel);

            // Grid
            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoGenerateColumns = false
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Path", DataPropertyName = "Path" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Entry", DataPropertyName = "Entry" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Line", DataPropertyName = "LineNo", Width = 70 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Snippet", DataPropertyName = "Snippet" });
            grid.DataSource = binding;

            // 右クリックメニュー
            var menu = new ContextMenuStrip();
            menu.Items.Add("フルパスをコピー", null, (s, e) => { var it = Current(); if (it != null) TryClipboard(it.Path); });
            menu.Items.Add("Entryパスをコピー", null, (s, e) => { var it = Current(); if (it != null) TryClipboard(it.Entry); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("エクスプローラーで開く", null, (s, e) =>
            {
                var it = Current();
                if (it != null && File.Exists(it.Path)) Process.Start("explorer.exe", "/select,\"" + it.Path + "\"");
            });
            menu.Items.Add("親フォルダを開く", null, (s, e) =>
            {
                var it = Current();
                if (it != null) Process.Start("explorer.exe", Path.GetDirectoryName(it.Path));
            });
            grid.ContextMenuStrip = menu;

            Controls.Add(grid);
        }

        private SearchRow Current() => grid.CurrentRow?.DataBoundItem as SearchRow;
        private void TryClipboard(string s) { try { if (!string.IsNullOrEmpty(s)) Clipboard.SetText(s); } catch { } }

        private void Browse()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (Directory.Exists(txtRoot.Text)) fbd.SelectedPath = txtRoot.Text;
                if (fbd.ShowDialog() == DialogResult.OK) txtRoot.Text = fbd.SelectedPath;
            }
        }

        private async Task StartSearchAsync()
        {
            var root = txtRoot.Text.Trim();
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                MessageBox.Show("起点フォルダが正しくありません"); return;
            }
            var query = txtQuery.Text;
            if (string.IsNullOrEmpty(query))
            {
                if (MessageBox.Show("内容フィルタが空です。全件走査しますか？", "確認", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            // Python スクリプトの場所（EXEと同じフォルダ）
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string pyPath = Path.Combine(exeDir, "fastfilefinder_scan.py");
            if (!File.Exists(pyPath))
            {
                MessageBox.Show("fastfilefinder_scan.py が見つかりません。EXEと同じフォルダに配置してください。");
                return;
            }

            // 引数
            var args = new List<string> { Quote(pyPath), "--folder", Quote(root), "--query", Quote(query) };
            if (chkRegex.Checked) args.Add("--regex");
            if (chkZip.Checked) args.Add("--zip");
            if (chkRecursive.Checked) args.Add("--recursive");
            var exts = txtExts.Text.Trim();
            if (!string.IsNullOrEmpty(exts)) { args.Add("--exts"); args.Add(Quote(exts)); }

            string pythonExe = string.IsNullOrWhiteSpace(txtPython.Text) ? "python" : txtPython.Text.Trim();

            // 実行
            binding.Clear(); lblStatus.Text = "起動中..."; btnSearch.Enabled = false; btnCancel.Enabled = true;
            cts = new CancellationTokenSource();
            currentProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = string.Join(" ", args.ToArray()),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                },
                EnableRaisingEvents = true
            };
            currentProc.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";


            currentProc.OutputDataReceived += (s, ev) =>
            {
                if (ev.Data == null) return;
                if (ev.Data.StartsWith("#")) { BeginInvoke((Action)(() => lblStatus.Text = ev.Data.TrimStart('#'))); return; }
                AddRowFromTsv(ev.Data);
            };
            currentProc.ErrorDataReceived += (s, ev) =>
            {
                if (string.IsNullOrEmpty(ev.Data)) return;
                BeginInvoke((Action)(() => lblStatus.Text = "ERR: " + ev.Data));
            };
            currentProc.Exited += (s, ev) =>
            {
                BeginInvoke((Action)(() =>
                {
                    lblStatus.Text = string.Format("完了。{0:N0} 件", binding.Count);
                    btnSearch.Enabled = true; btnCancel.Enabled = false;
                }));
            };

            try
            {
                currentProc.Start();
                currentProc.BeginOutputReadLine();
                currentProc.BeginErrorReadLine();
                await Task.Run(() =>
                {
                    while (!currentProc.HasExited)
                    {
                        if (cts.IsCancellationRequested) { try { currentProc.Kill(); } catch { } break; }
                        Thread.Sleep(100);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("python 実行エラー: " + ex.Message);
                btnSearch.Enabled = true; btnCancel.Enabled = false; lblStatus.Text = "エラー";
            }
        }

        private void CancelSearch() { try { cts?.Cancel(); } catch { } }

        private void AddRowFromTsv(string line)
        {
            // TSV: path\tentry\tlineno\tsnippet
            var cols = line.Split(new[] { '\t' }, 4);
            if (cols.Length < 4) return;
            int lineno = 0; int.TryParse(cols[2], out lineno);
            var row = new SearchRow { Path = cols[0], Entry = cols[1], LineNo = lineno, Snippet = cols[3] };
            BeginInvoke((Action)(() => binding.Add(row)));
        }

        private void ExportCsv()
        {
            if (binding.Count == 0) { MessageBox.Show("出力する行がありません"); return; }
            using (var sfd = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv|All Files (*.*)|*.*", FileName = "FastFileFinder.csv" })
            {
                if (sfd.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true)))
                    {
                        sw.WriteLine("Path,Entry,Line,Snippet");
                        foreach (var r in binding)
                        {
                            sw.WriteLine(string.Join(",", Csv(r.Path), Csv(r.Entry), r.LineNo.ToString(), Csv(r.Snippet)));
                        }
                    }
                    lblStatus.Text = "CSVに出力しました: " + sfd.FileName;
                }
                catch (Exception ex) { MessageBox.Show("CSV出力に失敗: " + ex.Message); }
            }
        }
        private string Csv(string s) { if (string.IsNullOrEmpty(s)) return ""; return "\"" + s.Replace("\"", "\"\"") + "\""; }

        private static string Quote(string s)
        {
            if (string.IsNullOrEmpty(s)) return "\"\"";
            if (s.IndexOf('\"') >= 0) s = s.Replace("\"", "\\\"");
            if (s.IndexOf(' ') >= 0 || s.IndexOf('\\') >= 0 || s.IndexOf(';') >= 0) return "\"" + s + "\"";
            return s;
        }
    }
}
