using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FastFileFinder
{
    public partial class Form1 : Form
    {
        private const int BatchSize = 1000;
        private const int MaxRecentFolders = 10;

        private static readonly Color RowEvenColor = Color.White;
        private static readonly Color RowOddColor = Color.FromArgb(247, 247, 247);
        private static readonly Color HoverColor = Color.FromArgb(229, 238, 248);
        private static readonly Color SelectionColor = Color.FromArgb(229, 241, 251);
        private static readonly Color HighlightColor = Color.FromArgb(255, 236, 179);

        private static readonly Color ButtonNormalColor = Color.FromArgb(245, 246, 248);
        private static readonly Color ButtonHoverColor = Color.FromArgb(233, 237, 245);
        private static readonly Color ButtonPressedColor = Color.FromArgb(220, 227, 239);
        private static readonly Color ButtonBorderColor = Color.FromArgb(197, 202, 215);
        private static readonly Color ButtonTextColor = Color.FromArgb(34, 34, 34);
        private static readonly Color ButtonDisabledBackColor = Color.FromArgb(237, 239, 243);
        private static readonly Color ButtonDisabledTextColor = Color.FromArgb(136, 136, 136);

        private static readonly Color PrimaryButtonNormalColor = Color.FromArgb(0, 120, 212);
        private static readonly Color PrimaryButtonHoverColor = Color.FromArgb(10, 132, 255);
        private static readonly Color PrimaryButtonPressedColor = Color.FromArgb(6, 111, 214);
        private static readonly Color PrimaryButtonBorderColor = Color.FromArgb(0, 98, 168);

        private ConcurrentQueue<SearchResult> _pendingResults = new ConcurrentQueue<SearchResult>();
        private readonly List<SearchResult> _allResults = new List<SearchResult>();
        private readonly List<int> _visibleIndices = new List<int>();
        private readonly List<string> _recentFolders = new List<string>();
        private readonly Stopwatch _stopwatch = new Stopwatch();

        private Process _process;
        private System.Threading.CancellationTokenSource _cancellation;
        private bool _isSearching;
        private bool _cancelRequested;

        private bool _searchIsRegex;
        private Regex _searchRegex;
        private string _searchLower = string.Empty;

        private string[] _quickFilterTokens = Array.Empty<string>();
        private bool _sortDescending;
        private string _sortColumn = string.Empty;
        private int _hoverRow = -1;

        private int _queuedFiles;
        private int _processedFiles;
        private int _totalHits;
        private string _currentFile = string.Empty;

        private string _statusMessage = string.Empty;
        private DateTime _statusMessageUntil = DateTime.MinValue;
        private int _tsvDebugLinesLogged;

        public Form1()
        {
            InitializeComponent();
            InitializeRuntime();
        }

        private void InitializeRuntime()
        {
            resultsGrid.AutoGenerateColumns = false;
            resultsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            resultsGrid.RowsDefaultCellStyle.BackColor = RowEvenColor;
            resultsGrid.AlternatingRowsDefaultCellStyle.BackColor = RowOddColor;
            resultsGrid.DefaultCellStyle.SelectionBackColor = SelectionColor;
            resultsGrid.DefaultCellStyle.SelectionForeColor = Color.Black;
            resultsGrid.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            resultsGrid.RowTemplate.Height = 28;
            resultsGrid.GridColor = Color.FromArgb(221, 221, 221);
            resultsGrid.ColumnHeadersDefaultCellStyle.Font = new Font(resultsGrid.Font, FontStyle.Bold);
            EnableDoubleBuffer(resultsGrid);

            txtExclude.Text = ".git;node_modules;bin;obj;.vs";

            ApplyButtonStyles();

            uiTimer.Start();
        }

        private static void EnableDoubleBuffer(DataGridView grid)
        {
            var property = typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            property?.SetValue(grid, true, null);
        }

        private void ApplyButtonStyles()
        {
            ApplyButtonStyle(btnBrowse, isPrimary: false);
            ApplyButtonStyle(btnBrowsePython, isPrimary: false);

            toolStripButtonStart.Tag = ModernToolStripRenderer.ButtonStyle.Primary;
            toolStripButtonCancel.Tag = ModernToolStripRenderer.ButtonStyle.Primary;
            toolStripButtonExport.Tag = ModernToolStripRenderer.ButtonStyle.Primary;

            toolStripMain.Renderer = new ModernToolStripRenderer(
                ButtonNormalColor,
                ButtonHoverColor,
                ButtonPressedColor,
                ButtonBorderColor,
                ButtonTextColor,
                ButtonDisabledBackColor,
                ButtonDisabledTextColor,
                PrimaryButtonNormalColor,
                PrimaryButtonHoverColor,
                PrimaryButtonPressedColor,
                PrimaryButtonBorderColor,
                Color.White);

            toolStripMain.BackColor = Color.White;
        }

        private static void ApplyButtonStyle(Button button, bool isPrimary)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = isPrimary ? PrimaryButtonBorderColor : ButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = isPrimary ? PrimaryButtonHoverColor : ButtonHoverColor;
            button.FlatAppearance.MouseDownBackColor = isPrimary ? PrimaryButtonPressedColor : ButtonPressedColor;
            button.BackColor = isPrimary ? PrimaryButtonNormalColor : ButtonNormalColor;
            button.ForeColor = isPrimary ? Color.White : ButtonTextColor;
            button.Padding = new Padding(12, 8, 12, 8);
            button.UseVisualStyleBackColor = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtPythonPath.Text = Properties.Settings.Default.PythonExecutablePath ?? string.Empty;
            LoadRecentFolders();
            UpdateStatusDisplay();
        }

        private void LoadRecentFolders()
        {
            _recentFolders.Clear();
            var stored = Properties.Settings.Default.RecentFolders;
            if (stored != null)
            {
                foreach (string entry in stored)
                {
                    if (string.IsNullOrWhiteSpace(entry))
                    {
                        continue;
                    }

                    if (!_recentFolders.Contains(entry, StringComparer.OrdinalIgnoreCase))
                    {
                        _recentFolders.Add(entry);
                    }
                }
            }

            RefreshRecentCombo();
        }

        private void RefreshRecentCombo()
        {
            comboRecent.BeginUpdate();
            comboRecent.Items.Clear();
            foreach (string path in _recentFolders)
            {
                comboRecent.Items.Add(path);
            }
            comboRecent.EndUpdate();
            comboRecent.SelectedIndex = -1;
        }

        private void SaveRecentFolders()
        {
            var collection = new StringCollection();
            foreach (string path in _recentFolders.Take(MaxRecentFolders))
            {
                collection.Add(path);
            }

            Properties.Settings.Default.RecentFolders = collection;
        }

        private void AddRecentFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            path = path.Trim();
            int existingIndex = _recentFolders.FindIndex(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                _recentFolders.RemoveAt(existingIndex);
            }

            _recentFolders.Insert(0, path);
            while (_recentFolders.Count > MaxRecentFolders)
            {
                _recentFolders.RemoveAt(_recentFolders.Count - 1);
            }

            RefreshRecentCombo();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isSearching)
            {
                CancelSearch();
                TryWaitForProcessExit();
            }

            Properties.Settings.Default.PythonExecutablePath = txtPythonPath.Text.Trim();
            SaveRecentFolders();
            Properties.Settings.Default.Save();
        }

        private void TryWaitForProcessExit()
        {
            var proc = _process;
            if (proc == null)
            {
                return;
            }

            try
            {
                proc.WaitForExit(2000);
            }
            catch
            {
                // Ignore
            }
        }

        private void ToolStripButtonStart_Click(object sender, EventArgs e) => StartSearch();

        private void ToolStripButtonCancel_Click(object sender, EventArgs e) => CancelSearch();

        private void ToolStripButtonExport_Click(object sender, EventArgs e) => ExportResults();

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (Directory.Exists(txtRoot.Text))
                {
                    dialog.SelectedPath = txtRoot.Text;
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtRoot.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBrowsePython_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Python 実行ファイル (python.exe)|python.exe|実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*";
                dialog.Title = "Python 実行ファイルを選択";
                dialog.CheckFileExists = true;

                string current = txtPythonPath.Text.Trim();
                if (!string.IsNullOrEmpty(current))
                {
                    try
                    {
                        if (File.Exists(current))
                        {
                            dialog.InitialDirectory = Path.GetDirectoryName(Path.GetFullPath(current));
                            dialog.FileName = Path.GetFileName(current);
                        }
                        else
                        {
                            string directory = Path.GetDirectoryName(current);
                            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                            {
                                dialog.InitialDirectory = directory;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths
                    }
                }

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtPythonPath.Text = dialog.FileName;
                }
            }
        }

        private void ComboRecent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboRecent.SelectedIndex >= 0 && comboRecent.SelectedIndex < _recentFolders.Count)
            {
                txtRoot.Text = _recentFolders[comboRecent.SelectedIndex];
            }
        }

        private void TxtQuickFilter_TextChanged(object sender, EventArgs e)
        {
            filterTimer.Stop();
            filterTimer.Start();
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            filterTimer.Stop();
            _quickFilterTokens = ParseFilterTokens(txtQuickFilter.Text);
            RebuildFilteredResults();
        }

        private static string[] ParseFilterTokens(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<string>();
            }

            return text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(t => t.Trim())
                       .Where(t => t.Length > 0)
                       .ToArray();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            DrainPendingResults(BatchSize);
            UpdateStatusDisplay();
        }

        private void DrainPendingResults(int limit)
        {
            int added = 0;
            while (added < limit && _pendingResults.TryDequeue(out var result))
            {
                int index = _allResults.Count;
                _allResults.Add(result);
                if (MatchesQuickFilter(result))
                {
                    _visibleIndices.Add(index);
                }

                added++;
            }

            if (added > 0)
            {
                ApplySortIfNeeded();
                resultsGrid.RowCount = _visibleIndices.Count;
                resultsGrid.Invalidate();
            }
        }

        private void RebuildFilteredResults()
        {
            _visibleIndices.Clear();
            for (int i = 0; i < _allResults.Count; i++)
            {
                if (MatchesQuickFilter(_allResults[i]))
                {
                    _visibleIndices.Add(i);
                }
            }

            ApplySortIfNeeded();
            resultsGrid.RowCount = _visibleIndices.Count;
            resultsGrid.Invalidate();
        }

        private bool MatchesQuickFilter(SearchResult result)
        {
            if (_quickFilterTokens.Length == 0)
            {
                return true;
            }

            foreach (var token in _quickFilterTokens)
            {
                if (result.DisplayPath?.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(result.Entry) && result.Entry.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(result.Snippet) && result.Snippet.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private void ApplySortIfNeeded()
        {
            if (string.IsNullOrEmpty(_sortColumn))
            {
                return;
            }

            Comparison<int> comparison = null;
            switch (_sortColumn)
            {
                case "columnPath":
                    comparison = (a, b) => string.Compare(_allResults[a].DisplayPath, _allResults[b].DisplayPath, StringComparison.OrdinalIgnoreCase);
                    break;
                case "columnExt":
                    comparison = (a, b) => string.Compare(_allResults[a].Extension, _allResults[b].Extension, StringComparison.OrdinalIgnoreCase);
                    break;
                case "columnEntry":
                    comparison = (a, b) => string.Compare(_allResults[a].Entry, _allResults[b].Entry, StringComparison.OrdinalIgnoreCase);
                    break;
                case "columnLine":
                    comparison = (a, b) => _allResults[a].LineNumber.CompareTo(_allResults[b].LineNumber);
                    break;
                case "columnSnippet":
                    comparison = (a, b) => string.Compare(_allResults[a].Snippet, _allResults[b].Snippet, StringComparison.OrdinalIgnoreCase);
                    break;
            }

            if (comparison == null)
            {
                return;
            }

            _visibleIndices.Sort((a, b) => _sortDescending ? -comparison(a, b) : comparison(a, b));
        }
        private void ResultsGrid_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            var result = GetResult(e.RowIndex);
            if (result == null)
            {
                return;
            }

            if (e.ColumnIndex == columnPath.Index)
            {
                e.Value = result.DisplayPath;
            }
            else if (e.ColumnIndex == columnExt.Index)
            {
                e.Value = result.Extension;
            }
            else if (e.ColumnIndex == columnEntry.Index)
            {
                e.Value = result.Entry;
            }
            else if (e.ColumnIndex == columnLine.Index)
            {
                e.Value = result.LineNumber;
            }
            else if (e.ColumnIndex == columnSnippet.Index)
            {
                e.Value = result.Snippet;
            }
        }

        private SearchResult GetResult(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _visibleIndices.Count)
            {
                return null;
            }

            int actual = _visibleIndices[rowIndex];
            if (actual < 0 || actual >= _allResults.Count)
            {
                return null;
            }

            return _allResults[actual];
        }

        private void ResultsGrid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            var result = GetResult(e.RowIndex);
            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            bool hover = !selected && e.RowIndex == _hoverRow;
            Color baseColor = (e.RowIndex % 2 == 0) ? RowEvenColor : RowOddColor;
            Color background = selected ? SelectionColor : hover ? HoverColor : baseColor;

            using (var brush = new SolidBrush(background))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }

            e.Paint(e.ClipBounds, DataGridViewPaintParts.Border);

            if (e.ColumnIndex != columnSnippet.Index || result == null)
            {
                e.PaintContent(e.CellBounds);
                e.Handled = true;
                return;
            }

            DrawSnippetCell(e, result);
        }

        private void DrawSnippetCell(DataGridViewCellPaintingEventArgs e, SearchResult result)
        {
            string text = result.Snippet ?? string.Empty;
            var highlights = result.Highlights;
            Rectangle bounds = new Rectangle(e.CellBounds.X + 4, e.CellBounds.Y + 2, e.CellBounds.Width - 8, e.CellBounds.Height - 4);
            var flags = TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

            if (highlights == null || highlights.Count == 0)
            {
                TextRenderer.DrawText(e.Graphics, text, e.CellStyle.Font, bounds, e.CellStyle.ForeColor, flags);
                e.Handled = true;
                return;
            }

            int current = 0;
            int x = bounds.Left;
            foreach (var span in highlights)
            {
                if (span.Start > text.Length)
                {
                    break;
                }

                if (span.Start > current)
                {
                    string before = text.Substring(current, span.Start - current);
                    DrawSegment(e.Graphics, before, e.CellStyle.Font, e.CellStyle.ForeColor, ref x, bounds, flags, false);
                }

                int length = Math.Min(span.Length, Math.Max(0, text.Length - span.Start));
                if (length > 0)
                {
                    string highlightText = text.Substring(span.Start, length);
                    DrawSegment(e.Graphics, highlightText, e.CellStyle.Font, Color.Black, ref x, bounds, flags, true);
                }

                current = span.Start + span.Length;
                if (x >= bounds.Right)
                {
                    break;
                }
            }

            if (current < text.Length && x < bounds.Right)
            {
                string tail = text.Substring(current);
                DrawSegment(e.Graphics, tail, e.CellStyle.Font, e.CellStyle.ForeColor, ref x, bounds, flags, false);
            }

            e.Handled = true;
        }

        private void DrawSegment(Graphics g, string text, Font font, Color color, ref int x, Rectangle bounds, TextFormatFlags flags, bool highlight)
        {
            if (string.IsNullOrEmpty(text) || x >= bounds.Right)
            {
                return;
            }

            Size size = TextRenderer.MeasureText(g, text, font, new Size(int.MaxValue, bounds.Height), flags);
            int width = Math.Min(size.Width, bounds.Right - x);
            var rect = new Rectangle(x, bounds.Y, width, bounds.Height);
            if (highlight)
            {
                using (var brush = new SolidBrush(HighlightColor))
                {
                    g.FillRectangle(brush, rect);
                }
            }

            TextRenderer.DrawText(g, text, font, rect, color, flags);
            x += width;
        }

        private void ResultsGrid_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                SetHoverRow(e.RowIndex);
            }
        }

        private void ResultsGrid_MouseLeave(object sender, EventArgs e)
        {
            SetHoverRow(-1);
        }

        private void SetHoverRow(int index)
        {
            if (_hoverRow == index)
            {
                return;
            }

            int previous = _hoverRow;
            _hoverRow = index;
            if (previous >= 0 && previous < resultsGrid.RowCount)
            {
                resultsGrid.InvalidateRow(previous);
            }

            if (_hoverRow >= 0 && _hoverRow < resultsGrid.RowCount)
            {
                resultsGrid.InvalidateRow(_hoverRow);
            }
        }

        private void ResultsGrid_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                resultsGrid.ClearSelection();
                resultsGrid.Rows[e.RowIndex].Selected = true;
                int col = e.ColumnIndex >= 0 ? e.ColumnIndex : 0;
                resultsGrid.CurrentCell = resultsGrid.Rows[e.RowIndex].Cells[col];
            }
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var row = GetResult(resultsGrid.CurrentCell?.RowIndex ?? -1);
            bool hasRow = row != null;
            menuCopyPath.Enabled = hasRow;
            menuOpenExplorer.Enabled = hasRow;
            menuOpenFolder.Enabled = hasRow;
            if (!hasRow)
            {
                e.Cancel = true;
            }
        }

        private void MenuCopyPath_Click(object sender, EventArgs e)
        {
            var row = GetResult(resultsGrid.CurrentCell?.RowIndex ?? -1);
            if (row == null)
            {
                return;
            }

            CopyText(row.DisplayPath);
        }

        private void MenuOpenExplorer_Click(object sender, EventArgs e)
        {
            var row = GetResult(resultsGrid.CurrentCell?.RowIndex ?? -1);
            if (row != null)
            {
                OpenInExplorer(row.FullPath);
            }
        }

        private void MenuOpenFolder_Click(object sender, EventArgs e)
        {
            var row = GetResult(resultsGrid.CurrentCell?.RowIndex ?? -1);
            if (row != null)
            {
                OpenParentFolder(row.FullPath);
            }
        }

        private void ResultsGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = GetResult(e.RowIndex);
            if (row != null)
            {
                OpenInExplorer(row.FullPath);
            }
        }

        private void ResultsGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedRows();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void CopySelectedRows()
        {
            if (resultsGrid.SelectedRows.Count == 0)
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (DataGridViewRow row in resultsGrid.SelectedRows)
            {
                var result = GetResult(row.Index);
                if (result == null)
                {
                    continue;
                }

                sb.Append(result.DisplayPath);
                sb.Append('\t');
                sb.Append(result.Entry);
                sb.Append('\t');
                sb.Append(result.LineNumber.ToString(CultureInfo.InvariantCulture));
                sb.Append('\t');
                sb.Append(result.Snippet);
                sb.AppendLine();
            }

            CopyText(sb.ToString());
        }

        private void CopyText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            try
            {
                Clipboard.SetText(text);
                SetStatusMessage("コピーしました", TimeSpan.FromSeconds(4));
            }
            catch
            {
                // Clipboard unavailable
            }
        }

        private void OpenInExplorer(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string display = ToDisplayPath(path);
            if (!File.Exists(display) && !Directory.Exists(display))
            {
                return;
            }

            string argument = $"/select,\"{display}\"";
            try
            {
                Process.Start("explorer.exe", argument);
            }
            catch (Exception ex)
            {
                SetStatusMessage("Explorer 起動に失敗: " + ex.Message, TimeSpan.FromSeconds(6));
            }
        }

        private void OpenParentFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string display = ToDisplayPath(path);
            string directory = Directory.Exists(display) ? display : Path.GetDirectoryName(display);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return;
            }

            try
            {
                Process.Start(directory);
            }
            catch (Exception ex)
            {
                SetStatusMessage("フォルダを開けません: " + ex.Message, TimeSpan.FromSeconds(6));
            }
        }

        private void ResultsGrid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string columnName = resultsGrid.Columns[e.ColumnIndex].Name;
            if (_sortColumn == columnName)
            {
                _sortDescending = !_sortDescending;
            }
            else
            {
                _sortColumn = columnName;
                _sortDescending = false;
            }

            ApplySortIfNeeded();
            resultsGrid.Invalidate();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Control && !e.Alt)
            {
                StartSearch();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                CancelSearch();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                StartSearch();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var items = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (items != null && items.Any(Directory.Exists))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }

            e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items == null)
            {
                return;
            }

            string folder = items.FirstOrDefault(Directory.Exists);
            if (!string.IsNullOrEmpty(folder))
            {
                txtRoot.Text = folder;
            }
        }
        private void StartSearch()
        {
            if (_isSearching)
            {
                return;
            }

            string folderInput = txtRoot.Text.Trim();
            if (string.IsNullOrEmpty(folderInput) || !Directory.Exists(folderInput))
            {
                MessageBox.Show(this, "起点フォルダを指定してください。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = txtQuery.Text;
            if (string.IsNullOrEmpty(query))
            {
                MessageBox.Show(this, "検索語を入力してください。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fastfilefinder_scan.py");
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show(this, "fastfilefinder_scan.py が見つかりません。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string pythonExecutable = txtPythonPath.Text.Trim();
            if (!string.IsNullOrEmpty(pythonExecutable) && !File.Exists(pythonExecutable))
            {
                MessageBox.Show(this, "指定された Python 実行ファイルが見つかりません。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string displayFolder;
            try
            {
                displayFolder = Path.GetFullPath(folderInput);
            }
            catch
            {
                displayFolder = folderInput;
            }

            string folderArgument = ToExtendedPath(displayFolder);

            PrepareHighlight(query, chkRegex.Checked);

            _pendingResults = new ConcurrentQueue<SearchResult>();
            _allResults.Clear();
            _visibleIndices.Clear();
            resultsGrid.RowCount = 0;
            resultsGrid.Invalidate();

            _queuedFiles = 0;
            _processedFiles = 0;
            _totalHits = 0;
            _currentFile = string.Empty;
            _statusMessage = string.Empty;
            _statusMessageUntil = DateTime.MinValue;
            _tsvDebugLinesLogged = 0;

            _stopwatch.Reset();
            _stopwatch.Start();

            _searchIsRegex = chkRegex.Checked;

            AddRecentFolder(displayFolder);

            string arguments = BuildArguments(scriptPath, folderArgument, query);

            var psi = new ProcessStartInfo
            {
                FileName = string.IsNullOrEmpty(pythonExecutable) ? "python" : pythonExecutable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = new UTF8Encoding(false),
                StandardErrorEncoding = new UTF8Encoding(false),
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
            };
            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            try
            {
                _process = new Process { StartInfo = psi, EnableRaisingEvents = true };
                _process.OutputDataReceived += Process_OutputDataReceived;
                _process.ErrorDataReceived += Process_ErrorDataReceived;
                _process.Exited += Process_Exited;
                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _cancellation = new System.Threading.CancellationTokenSource();
                _isSearching = true;
                _cancelRequested = false;
                SetInputsEnabled(false);
                SetStatusMessage("検索を開始しました", TimeSpan.FromSeconds(4));
            }
            catch (Exception ex)
            {
                _process = null;
                MessageBox.Show(this, "Python を起動できません: " + ex.Message, "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _stopwatch.Reset();
                SetInputsEnabled(true);
            }
        }

        private string BuildArguments(string scriptPath, string folder, string query)
        {
            var args = new List<string>
            {
                scriptPath,
                "--folder", folder,
                "--query", query,
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

            string exts = txtExtensions.Text.Trim();
            if (!string.IsNullOrEmpty(exts))
            {
                args.Add("--exts");
                args.Add(exts);
            }

            string exclude = txtExclude.Text.Trim();
            if (!string.IsNullOrEmpty(exclude))
            {
                args.Add("--exclude-folders");
                args.Add(exclude);
            }

            args.Add("--max-workers");
            args.Add(((int)numParallel.Value).ToString(CultureInfo.InvariantCulture));

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
                if (chkWord.Checked)
                {
                    args.Add("--legacy-doc");
                    args.Add("com");
                }
            }

            return string.Join(" ", args.Select(QuoteArgument));
        }

        private static string QuoteArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return "\"\"";
            }

            bool needQuotes = arg.Any(c => char.IsWhiteSpace(c) || c == '\"');
            if (!needQuotes)
            {
                return arg;
            }

            var sb = new StringBuilder();
            sb.Append('\"');
            int backslashes = 0;
            foreach (char c in arg)
            {
                if (c == '\\')
                {
                    backslashes++;
                }
                else if (c == '\"')
                {
                    sb.Append('\\', backslashes * 2 + 1);
                    sb.Append(c);
                    backslashes = 0;
                }
                else
                {
                    if (backslashes > 0)
                    {
                        sb.Append('\\', backslashes);
                        backslashes = 0;
                    }
                    sb.Append(c);
                }
            }

            if (backslashes > 0)
            {
                sb.Append('\\', backslashes * 2);
            }

            sb.Append('\"');
            return sb.ToString();
        }

        private void PrepareHighlight(string query, bool isRegex)
        {
            _searchRegex = null;
            _searchLower = string.Empty;
            if (isRegex)
            {
                try
                {
                    _searchRegex = new Regex(query, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch
                {
                    _searchRegex = null;
                }
            }
            else
            {
                _searchLower = query?.ToLowerInvariant() ?? string.Empty;
            }
        }

        private IReadOnlyList<HighlightSpan> BuildHighlights(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Array.Empty<HighlightSpan>();
            }

            if (_searchIsRegex)
            {
                if (_searchRegex == null)
                {
                    return Array.Empty<HighlightSpan>();
                }

                var match = _searchRegex.Match(text);
                if (!match.Success)
                {
                    return Array.Empty<HighlightSpan>();
                }

                return new[] { new HighlightSpan(match.Index, match.Length) };
            }

            if (string.IsNullOrEmpty(_searchLower))
            {
                return Array.Empty<HighlightSpan>();
            }

            var spans = new List<HighlightSpan>();
            int index = 0;
            while (index < text.Length)
            {
                int found = text.IndexOf(_searchLower, index, StringComparison.OrdinalIgnoreCase);
                if (found < 0)
                {
                    break;
                }

                spans.Add(new HighlightSpan(found, _searchLower.Length));
                index = found + _searchLower.Length;
                if (spans.Count > 64)
                {
                    break;
                }
            }

            return spans;
        }

        private void CancelSearch()
        {
            if (!_isSearching || _cancelRequested)
            {
                return;
            }

            _cancelRequested = true;
            toolStripButtonCancel.Enabled = false;
            SetStatusMessage("キャンセルしています...", TimeSpan.FromSeconds(5));

            var proc = _process;
            if (proc != null)
            {
                Task.Run(() => CancelProcessAsync(proc));
            }
        }

        private async Task CancelProcessAsync(Process process)
        {
            try
            {
                process.CancelOutputRead();
            }
            catch
            {
            }

            try
            {
                process.CancelErrorRead();
            }
            catch
            {
            }

            try
            {
                process.StandardInput?.Close();
            }
            catch
            {
            }

            try
            {
                process.StandardOutput?.Close();
            }
            catch
            {
            }

            try
            {
                process.StandardError?.Close();
            }
            catch
            {
            }

            await Task.Delay(250).ConfigureAwait(false);

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            {
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }

            if (e.Data.StartsWith("#", StringComparison.Ordinal))
            {
                BeginInvoke((Action)(() => HandleStatusLine(e.Data.Substring(1))));
                return;
            }

            var parts = e.Data.Split('\t');
            if (parts.Length < 4)
            {
                return;
            }

            if (_tsvDebugLinesLogged < 10)
            {
                int index = Interlocked.Increment(ref _tsvDebugLinesLogged);
                if (index <= 10)
                {
                    Debug.WriteLine($"[FastFileFinder TSV {index}] {e.Data}");
                    if (index == 10)
                    {
                        BeginInvoke((Action)(() =>
                            SetStatusMessage("TSVデバッグ: 最初の10行をデバッグ出力しました", TimeSpan.FromSeconds(6))));
                    }
                }
            }

            string path = parts[0];
            string entry = parts[1];
            int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int line);
            string snippet = parts[3];

            string extension = Path.GetExtension(path);
            string extDisplay = string.IsNullOrEmpty(extension) ? string.Empty : extension.TrimStart('.');

            var result = new SearchResult(path, ToDisplayPath(path), extDisplay, entry, line, snippet, BuildHighlights(snippet));
            _pendingResults.Enqueue(result);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            BeginInvoke((Action)(() =>
            {
                SetStatusMessage("⚠ " + e.Data, TimeSpan.FromSeconds(8));
            }));
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            BeginInvoke((Action)OnProcessExited);
        }

        private void OnProcessExited()
        {
            bool wasCancelled = _cancelRequested;
            _stopwatch.Stop();
            _isSearching = false;
            _cancelRequested = false;

            if (_process != null)
            {
                _process.OutputDataReceived -= Process_OutputDataReceived;
                _process.ErrorDataReceived -= Process_ErrorDataReceived;
                _process.Exited -= Process_Exited;
                _process.Dispose();
                _process = null;
            }

            _cancellation?.Dispose();
            _cancellation = null;

            SetInputsEnabled(true);
            if (wasCancelled)
            {
                SetStatusMessage("キャンセルしました", TimeSpan.FromSeconds(6));
            }
            UpdateStatusDisplay();
        }

        private void HandleStatusLine(string payload)
        {
            var parts = payload.Split('\t');
            if (parts.Length == 0)
            {
                return;
            }

            switch (parts[0])
            {
                case "queued":
                    if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int queued))
                    {
                        _queuedFiles = queued;
                    }

                    SetStatusMessage($"対象 { _queuedFiles } ファイル", TimeSpan.FromSeconds(4));
                    break;
                case "current":
                    if (parts.Length > 1)
                    {
                        _currentFile = parts[1];
                    }

                    break;
                case "progress":
                    if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int processed))
                    {
                        _processedFiles = processed;
                    }

                    if (parts.Length > 2 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int hits))
                    {
                        _totalHits = hits;
                    }

                    if (parts.Length > 3)
                    {
                        _currentFile = parts[3];
                    }

                    break;
                case "done":
                    if (parts.Length > 1 && int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int done))
                    {
                        _processedFiles = done;
                    }

                    if (parts.Length > 2 && int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int total))
                    {
                        _totalHits = total;
                    }

                    if (parts.Length > 3 && double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double elapsed))
                    {
                        SetStatusMessage($"完了: {elapsed:F2} 秒", TimeSpan.FromSeconds(10));
                    }
                    else
                    {
                        SetStatusMessage("完了しました", TimeSpan.FromSeconds(8));
                    }

                    break;
                default:
                    SetStatusMessage(parts[0], TimeSpan.FromSeconds(6));
                    break;
            }

            UpdateStatusDisplay();
        }

        private void UpdateStatusDisplay()
        {
            TimeSpan elapsed = _stopwatch.IsRunning ? _stopwatch.Elapsed : TimeSpan.Zero;
            statusElapsed.Text = $"経過: {elapsed:hh\\:mm\\:ss}";
            statusFiles.Text = $"処理: {_processedFiles} / {_queuedFiles} 件";
            statusHits.Text = $"ヒット: {_totalHits} 件";

            string message;
            if (!string.IsNullOrEmpty(_statusMessage) && DateTime.Now <= _statusMessageUntil)
            {
                message = _statusMessage;
            }
            else if (!string.IsNullOrEmpty(_currentFile))
            {
                message = Shorten(ToDisplayPath(_currentFile));
            }
            else
            {
                message = "Ready";
            }

            statusMessage.Text = message;
            statusMessage.ToolTipText = !string.IsNullOrEmpty(_currentFile) ? ToDisplayPath(_currentFile) : string.Empty;
            toolStripButtonExport.Enabled = !_isSearching && _visibleIndices.Count > 0;
        }

        private static string Shorten(string value, int max = 80)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }

            int keep = Math.Max(1, max - 3);
            int head = keep / 2;
            int tail = keep - head;
            return value.Substring(0, head) + "..." + value.Substring(value.Length - tail);
        }

        private void SetStatusMessage(string message, TimeSpan duration)
        {
            _statusMessage = message;
            _statusMessageUntil = DateTime.Now.Add(duration);
            UpdateStatusDisplay();
        }

        private void SetInputsEnabled(bool enabled)
        {
            txtRoot.ReadOnly = !enabled;
            btnBrowse.Enabled = enabled;
            comboRecent.Enabled = enabled;
            txtPythonPath.ReadOnly = !enabled;
            btnBrowsePython.Enabled = enabled;
            txtQuery.ReadOnly = !enabled;
            chkRegex.Enabled = enabled;
            txtExtensions.ReadOnly = !enabled;
            txtExclude.ReadOnly = !enabled;
            numParallel.Enabled = enabled;
            chkWord.Enabled = enabled;
            chkExcel.Enabled = enabled;
            chkLegacy.Enabled = enabled;
            chkRecursive.Enabled = enabled;
            chkZip.Enabled = enabled;

            toolStripButtonStart.Enabled = enabled;
            toolStripButtonCancel.Enabled = !enabled;
            toolStripButtonExport.Enabled = enabled && _visibleIndices.Count > 0;

            Cursor = enabled ? Cursors.Default : Cursors.AppStarting;
        }

        private static string ToExtendedPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = path.Replace('/', '\\');
            if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
            {
                return path;
            }

            if (path.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return @"\\?\UNC\" + path.Substring(2);
            }

            if (path.Length >= 2 && path[1] == ':')
            {
                return @"\\?\" + path;
            }

            return path;
        }

        private static string ToDisplayPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            path = path.Replace('/', '\\');
            if (path.StartsWith(@"\\?\UNC\", StringComparison.Ordinal))
            {
                return @"\\" + path.Substring(@"\\?\UNC\".Length);
            }

            if (path.StartsWith(@"\\?\", StringComparison.Ordinal))
            {
                return path.Substring(4);
            }

            return path;
        }

        private void ExportResults()
        {
            if (_visibleIndices.Count == 0)
            {
                MessageBox.Show(this, "出力対象の結果がありません。", "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv|TSV (*.tsv)|*.tsv|All Files (*.*)|*.*",
                FileName = "FastFileFinder.csv",
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                bool isTsv = string.Equals(Path.GetExtension(dialog.FileName), ".tsv", StringComparison.OrdinalIgnoreCase);
                try
                {
                    using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                    {
                        if (isTsv)
                        {
                            foreach (int index in _visibleIndices)
                            {
                                var result = _allResults[index];
                                writer.WriteLine(string.Join("\t", result.DisplayPath, result.Entry, result.LineNumber.ToString(CultureInfo.InvariantCulture), result.Snippet));
                            }
                        }
                        else
                        {
                            writer.WriteLine("Path,Entry,Line,Snippet");
                            foreach (int index in _visibleIndices)
                            {
                                var result = _allResults[index];
                                writer.WriteLine(string.Join(",", EscapeCsv(result.DisplayPath), EscapeCsv(result.Entry), result.LineNumber.ToString(CultureInfo.InvariantCulture), EscapeCsv(result.Snippet)));
                            }
                        }
                    }

                    SetStatusMessage("ファイルに出力しました", TimeSpan.FromSeconds(6));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "出力に失敗しました: " + ex.Message, "FastFileFinder", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private sealed class ModernToolStripRenderer : ToolStripProfessionalRenderer
        {
            public enum ButtonStyle
            {
                Secondary,
                Primary,
            }

            private readonly Color _secondaryNormal;
            private readonly Color _secondaryHover;
            private readonly Color _secondaryPressed;
            private readonly Color _secondaryBorder;
            private readonly Color _secondaryText;
            private readonly Color _disabledBack;
            private readonly Color _disabledText;
            private readonly Color _primaryNormal;
            private readonly Color _primaryHover;
            private readonly Color _primaryPressed;
            private readonly Color _primaryBorder;
            private readonly Color _primaryText;

            public ModernToolStripRenderer(
                Color secondaryNormal,
                Color secondaryHover,
                Color secondaryPressed,
                Color secondaryBorder,
                Color secondaryText,
                Color disabledBack,
                Color disabledText,
                Color primaryNormal,
                Color primaryHover,
                Color primaryPressed,
                Color primaryBorder,
                Color primaryText)
                : base(new ProfessionalColorTable())
            {
                _secondaryNormal = secondaryNormal;
                _secondaryHover = secondaryHover;
                _secondaryPressed = secondaryPressed;
                _secondaryBorder = secondaryBorder;
                _secondaryText = secondaryText;
                _disabledBack = disabledBack;
                _disabledText = disabledText;
                _primaryNormal = primaryNormal;
                _primaryHover = primaryHover;
                _primaryPressed = primaryPressed;
                _primaryBorder = primaryBorder;
                _primaryText = primaryText;
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                // Suppress default border rendering for a flatter appearance
            }

            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                var style = GetStyle(e.Item);
                var bounds = new Rectangle(Point.Empty, e.Item.Size);

                Color backColor;
                Color borderColor;

                if (!e.Item.Enabled)
                {
                    if (style == ButtonStyle.Primary)
                    {
                        backColor = ControlPaint.Light(_primaryNormal, 0.6f);
                        borderColor = ControlPaint.Light(_primaryBorder, 0.6f);
                    }
                    else
                    {
                        backColor = _disabledBack;
                        borderColor = _secondaryBorder;
                    }
                }
                else if (e.Item.Pressed)
                {
                    backColor = style == ButtonStyle.Primary ? _primaryPressed : _secondaryPressed;
                    borderColor = style == ButtonStyle.Primary ? _primaryBorder : _secondaryBorder;
                }
                else if (e.Item.Selected)
                {
                    backColor = style == ButtonStyle.Primary ? _primaryHover : _secondaryHover;
                    borderColor = style == ButtonStyle.Primary ? _primaryBorder : _secondaryBorder;
                }
                else
                {
                    backColor = style == ButtonStyle.Primary ? _primaryNormal : _secondaryNormal;
                    borderColor = style == ButtonStyle.Primary ? _primaryBorder : _secondaryBorder;
                }

                using (var brush = new SolidBrush(backColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }

                using (var pen = new Pen(borderColor))
                {
                    var rect = new Rectangle(bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                var style = GetStyle(e.Item);
                if (!e.Item.Enabled)
                {
                    e.TextColor = _disabledText;
                }
                else if (style == ButtonStyle.Primary)
                {
                    e.TextColor = _primaryText;
                }
                else
                {
                    e.TextColor = _secondaryText;
                }

                base.OnRenderItemText(e);
            }

            private static ButtonStyle GetStyle(ToolStripItem item)
            {
                return item.Tag is ButtonStyle style ? style : ButtonStyle.Secondary;
            }
        }

        private sealed class SearchResult
        {
            public SearchResult(string fullPath, string displayPath, string extension, string entry, int lineNumber, string snippet, IReadOnlyList<HighlightSpan> highlights)
            {
                FullPath = fullPath;
                DisplayPath = displayPath;
                Extension = extension ?? string.Empty;
                Entry = entry;
                LineNumber = lineNumber;
                Snippet = snippet;
                Highlights = highlights ?? Array.Empty<HighlightSpan>();
            }

            public string FullPath { get; }
            public string DisplayPath { get; }
            public string Extension { get; }
            public string Entry { get; }
            public int LineNumber { get; }
            public string Snippet { get; }
            public IReadOnlyList<HighlightSpan> Highlights { get; }
        }

        private readonly struct HighlightSpan
        {
            public HighlightSpan(int start, int length)
            {
                Start = start;
                Length = length;
            }

            public int Start { get; }
            public int Length { get; }
        }
    }
}
