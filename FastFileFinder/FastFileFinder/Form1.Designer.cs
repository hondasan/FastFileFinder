namespace FastFileFinder
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.toolStripMain = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonExport = new System.Windows.Forms.ToolStripButton();
            this.panelConditions = new System.Windows.Forms.Panel();
            this.tableConditions = new System.Windows.Forms.TableLayoutPanel();
            this.labelRoot = new System.Windows.Forms.Label();
            this.flowRoot = new System.Windows.Forms.FlowLayoutPanel();
            this.txtRoot = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.comboRecent = new System.Windows.Forms.ComboBox();
            this.labelQuery = new System.Windows.Forms.Label();
            this.flowQuery = new System.Windows.Forms.FlowLayoutPanel();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.chkRegex = new System.Windows.Forms.CheckBox();
            this.labelExtensions = new System.Windows.Forms.Label();
            this.txtExtensions = new System.Windows.Forms.TextBox();
            this.labelExclude = new System.Windows.Forms.Label();
            this.txtExclude = new System.Windows.Forms.TextBox();
            this.labelParallel = new System.Windows.Forms.Label();
            this.flowParallel = new System.Windows.Forms.FlowLayoutPanel();
            this.numParallel = new System.Windows.Forms.NumericUpDown();
            this.lblParallelHint = new System.Windows.Forms.Label();
            this.labelOffice = new System.Windows.Forms.Label();
            this.flowOffice = new System.Windows.Forms.FlowLayoutPanel();
            this.chkWord = new System.Windows.Forms.CheckBox();
            this.chkExcel = new System.Windows.Forms.CheckBox();
            this.chkLegacy = new System.Windows.Forms.CheckBox();
            this.labelQuickFilter = new System.Windows.Forms.Label();
            this.txtQuickFilter = new System.Windows.Forms.TextBox();
            this.labelOptions = new System.Windows.Forms.Label();
            this.flowOptions = new System.Windows.Forms.FlowLayoutPanel();
            this.chkRecursive = new System.Windows.Forms.CheckBox();
            this.chkZip = new System.Windows.Forms.CheckBox();
            this.resultsGrid = new System.Windows.Forms.DataGridView();
            this.columnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnExt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnEntry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnLine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSnippet = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusElapsed = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusFiles = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusHits = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.uiTimer = new System.Windows.Forms.Timer(this.components);
            this.filterTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMain.SuspendLayout();
            this.panelConditions.SuspendLayout();
            this.tableConditions.SuspendLayout();
            this.flowRoot.SuspendLayout();
            this.flowQuery.SuspendLayout();
            this.flowParallel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numParallel)).BeginInit();
            this.flowOffice.SuspendLayout();
            this.flowOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMain
            // 
            this.toolStripMain.AutoSize = false;
            this.toolStripMain.BackColor = System.Drawing.Color.White;
            this.toolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonStart,
            this.toolStripButtonCancel,
            this.toolStripButtonExport});
            this.toolStripMain.Location = new System.Drawing.Point(0, 0);
            this.toolStripMain.Name = "toolStripMain";
            this.toolStripMain.Padding = new System.Windows.Forms.Padding(8, 6, 8, 6);
            this.toolStripMain.Size = new System.Drawing.Size(1180, 48);
            this.toolStripMain.TabIndex = 0;
            this.toolStripMain.Text = "toolStrip1";
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonStart.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.toolStripButtonStart.Size = new System.Drawing.Size(70, 36);
            this.toolStripButtonStart.Text = "検索開始";
            this.toolStripButtonStart.Click += new System.EventHandler(this.ToolStripButtonStart_Click);
            // 
            // toolStripButtonCancel
            // 
            this.toolStripButtonCancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonCancel.Enabled = false;
            this.toolStripButtonCancel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.toolStripButtonCancel.Name = "toolStripButtonCancel";
            this.toolStripButtonCancel.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.toolStripButtonCancel.Size = new System.Drawing.Size(70, 36);
            this.toolStripButtonCancel.Text = "キャンセル";
            this.toolStripButtonCancel.Click += new System.EventHandler(this.ToolStripButtonCancel_Click);
            // 
            // toolStripButtonExport
            // 
            this.toolStripButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonExport.Name = "toolStripButtonExport";
            this.toolStripButtonExport.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.toolStripButtonExport.Size = new System.Drawing.Size(73, 36);
            this.toolStripButtonExport.Text = "CSV 出力";
            this.toolStripButtonExport.Click += new System.EventHandler(this.ToolStripButtonExport_Click);
            // 
            // panelConditions
            // 
            this.panelConditions.AutoSize = true;
            this.panelConditions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panelConditions.BackColor = System.Drawing.Color.White;
            this.panelConditions.Controls.Add(this.tableConditions);
            this.panelConditions.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelConditions.Location = new System.Drawing.Point(0, 48);
            this.panelConditions.Name = "panelConditions";
            this.panelConditions.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.panelConditions.Size = new System.Drawing.Size(1180, 292);
            this.panelConditions.TabIndex = 1;
            // 
            // tableConditions
            // 
            this.tableConditions.AutoSize = true;
            this.tableConditions.ColumnCount = 2;
            this.tableConditions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableConditions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableConditions.Controls.Add(this.labelRoot, 0, 0);
            this.tableConditions.Controls.Add(this.flowRoot, 1, 0);
            this.tableConditions.Controls.Add(this.labelQuery, 0, 1);
            this.tableConditions.Controls.Add(this.flowQuery, 1, 1);
            this.tableConditions.Controls.Add(this.labelExtensions, 0, 2);
            this.tableConditions.Controls.Add(this.txtExtensions, 1, 2);
            this.tableConditions.Controls.Add(this.labelExclude, 0, 3);
            this.tableConditions.Controls.Add(this.txtExclude, 1, 3);
            this.tableConditions.Controls.Add(this.labelParallel, 0, 4);
            this.tableConditions.Controls.Add(this.flowParallel, 1, 4);
            this.tableConditions.Controls.Add(this.labelOffice, 0, 5);
            this.tableConditions.Controls.Add(this.flowOffice, 1, 5);
            this.tableConditions.Controls.Add(this.labelQuickFilter, 0, 6);
            this.tableConditions.Controls.Add(this.txtQuickFilter, 1, 6);
            this.tableConditions.Controls.Add(this.labelOptions, 0, 7);
            this.tableConditions.Controls.Add(this.flowOptions, 1, 7);
            this.tableConditions.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableConditions.Location = new System.Drawing.Point(12, 8);
            this.tableConditions.Name = "tableConditions";
            this.tableConditions.RowCount = 8;
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableConditions.Size = new System.Drawing.Size(1156, 276);
            this.tableConditions.TabIndex = 0;
            // 
            // labelRoot
            // 
            this.labelRoot.AutoSize = true;
            this.labelRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRoot.Location = new System.Drawing.Point(3, 0);
            this.labelRoot.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelRoot.Name = "labelRoot";
            this.labelRoot.Size = new System.Drawing.Size(144, 32);
            this.labelRoot.TabIndex = 0;
            this.labelRoot.Text = "起点フォルダ";
            this.labelRoot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowRoot
            // 
            this.flowRoot.AutoSize = true;
            this.flowRoot.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowRoot.Controls.Add(this.txtRoot);
            this.flowRoot.Controls.Add(this.btnBrowse);
            this.flowRoot.Controls.Add(this.comboRecent);
            this.flowRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowRoot.Location = new System.Drawing.Point(153, 3);
            this.flowRoot.Name = "flowRoot";
            this.flowRoot.Size = new System.Drawing.Size(1000, 26);
            this.flowRoot.TabIndex = 1;
            this.flowRoot.WrapContents = false;
            // 
            // txtRoot
            // 
            this.txtRoot.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtRoot.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            this.txtRoot.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.txtRoot.Name = "txtRoot";
            this.txtRoot.Size = new System.Drawing.Size(420, 23);
            this.txtRoot.TabIndex = 0;
            // 
            // btnBrowse
            // 
            this.btnBrowse.AutoSize = true;
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnBrowse.Location = new System.Drawing.Point(428, 0);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(52, 26);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "参照";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.BtnBrowse_Click);
            // 
            // comboRecent
            // 
            this.comboRecent.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboRecent.FormattingEnabled = true;
            this.comboRecent.Location = new System.Drawing.Point(488, 0);
            this.comboRecent.Margin = new System.Windows.Forms.Padding(0);
            this.comboRecent.Name = "comboRecent";
            this.comboRecent.Size = new System.Drawing.Size(260, 23);
            this.comboRecent.TabIndex = 2;
            this.comboRecent.SelectedIndexChanged += new System.EventHandler(this.ComboRecent_SelectedIndexChanged);
            // 
            // labelQuery
            // 
            this.labelQuery.AutoSize = true;
            this.labelQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelQuery.Location = new System.Drawing.Point(3, 32);
            this.labelQuery.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelQuery.Name = "labelQuery";
            this.labelQuery.Size = new System.Drawing.Size(144, 32);
            this.labelQuery.TabIndex = 2;
            this.labelQuery.Text = "内容フィルタ";
            this.labelQuery.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowQuery
            // 
            this.flowQuery.AutoSize = true;
            this.flowQuery.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowQuery.Controls.Add(this.txtQuery);
            this.flowQuery.Controls.Add(this.chkRegex);
            this.flowQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowQuery.Location = new System.Drawing.Point(153, 35);
            this.flowQuery.Name = "flowQuery";
            this.flowQuery.Size = new System.Drawing.Size(1000, 26);
            this.flowQuery.TabIndex = 3;
            this.flowQuery.WrapContents = false;
            // 
            // txtQuery
            // 
            this.txtQuery.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.Size = new System.Drawing.Size(420, 23);
            this.txtQuery.TabIndex = 0;
            // 
            // chkRegex
            // 
            this.chkRegex.AutoSize = true;
            this.chkRegex.Location = new System.Drawing.Point(428, 3);
            this.chkRegex.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.chkRegex.Name = "chkRegex";
            this.chkRegex.Size = new System.Drawing.Size(86, 19);
            this.chkRegex.TabIndex = 1;
            this.chkRegex.Text = "正規表現";
            this.chkRegex.UseVisualStyleBackColor = true;
            // 
            // labelExtensions
            // 
            this.labelExtensions.AutoSize = true;
            this.labelExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelExtensions.Location = new System.Drawing.Point(3, 64);
            this.labelExtensions.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelExtensions.Name = "labelExtensions";
            this.labelExtensions.Size = new System.Drawing.Size(144, 32);
            this.labelExtensions.TabIndex = 4;
            this.labelExtensions.Text = "対象拡張子";
            this.labelExtensions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtExtensions
            // 
            this.txtExtensions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExtensions.Location = new System.Drawing.Point(153, 67);
            this.txtExtensions.Name = "txtExtensions";
            this.txtExtensions.Size = new System.Drawing.Size(1000, 23);
            this.txtExtensions.TabIndex = 5;
            // 
            // labelExclude
            // 
            this.labelExclude.AutoSize = true;
            this.labelExclude.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelExclude.Location = new System.Drawing.Point(3, 96);
            this.labelExclude.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelExclude.Name = "labelExclude";
            this.labelExclude.Size = new System.Drawing.Size(144, 32);
            this.labelExclude.TabIndex = 6;
            this.labelExclude.Text = "除外フォルダ";
            this.labelExclude.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtExclude
            // 
            this.txtExclude.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtExclude.Location = new System.Drawing.Point(153, 99);
            this.txtExclude.Name = "txtExclude";
            this.txtExclude.Size = new System.Drawing.Size(1000, 23);
            this.txtExclude.TabIndex = 7;
            // 
            // labelParallel
            // 
            this.labelParallel.AutoSize = true;
            this.labelParallel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelParallel.Location = new System.Drawing.Point(3, 128);
            this.labelParallel.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelParallel.Name = "labelParallel";
            this.labelParallel.Size = new System.Drawing.Size(144, 32);
            this.labelParallel.TabIndex = 8;
            this.labelParallel.Text = "並列度";
            this.labelParallel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowParallel
            // 
            this.flowParallel.AutoSize = true;
            this.flowParallel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowParallel.Controls.Add(this.numParallel);
            this.flowParallel.Controls.Add(this.lblParallelHint);
            this.flowParallel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowParallel.Location = new System.Drawing.Point(153, 131);
            this.flowParallel.Name = "flowParallel";
            this.flowParallel.Size = new System.Drawing.Size(1000, 26);
            this.flowParallel.TabIndex = 9;
            this.flowParallel.WrapContents = false;
            // 
            // numParallel
            // 
            this.numParallel.Location = new System.Drawing.Point(0, 0);
            this.numParallel.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.numParallel.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numParallel.Name = "numParallel";
            this.numParallel.Size = new System.Drawing.Size(80, 23);
            this.numParallel.TabIndex = 0;
            // 
            // lblParallelHint
            // 
            this.lblParallelHint.AutoSize = true;
            this.lblParallelHint.Location = new System.Drawing.Point(88, 3);
            this.lblParallelHint.Name = "lblParallelHint";
            this.lblParallelHint.Size = new System.Drawing.Size(56, 15);
            this.lblParallelHint.TabIndex = 1;
            this.lblParallelHint.Text = "0 = 自動";
            // 
            // labelOffice
            // 
            this.labelOffice.AutoSize = true;
            this.labelOffice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOffice.Location = new System.Drawing.Point(3, 160);
            this.labelOffice.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelOffice.Name = "labelOffice";
            this.labelOffice.Size = new System.Drawing.Size(144, 32);
            this.labelOffice.TabIndex = 10;
            this.labelOffice.Text = "Office 形式";
            this.labelOffice.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowOffice
            // 
            this.flowOffice.AutoSize = true;
            this.flowOffice.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowOffice.Controls.Add(this.chkWord);
            this.flowOffice.Controls.Add(this.chkExcel);
            this.flowOffice.Controls.Add(this.chkLegacy);
            this.flowOffice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowOffice.Location = new System.Drawing.Point(153, 163);
            this.flowOffice.Name = "flowOffice";
            this.flowOffice.Size = new System.Drawing.Size(1000, 26);
            this.flowOffice.TabIndex = 11;
            this.flowOffice.WrapContents = false;
            // 
            // chkWord
            // 
            this.chkWord.AutoSize = true;
            this.chkWord.Checked = true;
            this.chkWord.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWord.Location = new System.Drawing.Point(0, 3);
            this.chkWord.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chkWord.Name = "chkWord";
            this.chkWord.Size = new System.Drawing.Size(100, 19);
            this.chkWord.TabIndex = 0;
            this.chkWord.Text = "Word (.docx)";
            this.chkWord.UseVisualStyleBackColor = true;
            // 
            // chkExcel
            // 
            this.chkExcel.AutoSize = true;
            this.chkExcel.Checked = true;
            this.chkExcel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExcel.Location = new System.Drawing.Point(112, 3);
            this.chkExcel.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chkExcel.Name = "chkExcel";
            this.chkExcel.Size = new System.Drawing.Size(104, 19);
            this.chkExcel.TabIndex = 1;
            this.chkExcel.Text = "Excel (.xlsx)";
            this.chkExcel.UseVisualStyleBackColor = true;
            // 
            // chkLegacy
            // 
            this.chkLegacy.AutoSize = true;
            this.chkLegacy.Location = new System.Drawing.Point(228, 3);
            this.chkLegacy.Margin = new System.Windows.Forms.Padding(0);
            this.chkLegacy.Name = "chkLegacy";
            this.chkLegacy.Size = new System.Drawing.Size(140, 19);
            this.chkLegacy.TabIndex = 2;
            this.chkLegacy.Text = "旧形式 (.doc/.xls)";
            this.chkLegacy.UseVisualStyleBackColor = true;
            // 
            // labelQuickFilter
            // 
            this.labelQuickFilter.AutoSize = true;
            this.labelQuickFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelQuickFilter.Location = new System.Drawing.Point(3, 192);
            this.labelQuickFilter.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelQuickFilter.Name = "labelQuickFilter";
            this.labelQuickFilter.Size = new System.Drawing.Size(144, 32);
            this.labelQuickFilter.TabIndex = 12;
            this.labelQuickFilter.Text = "結果フィルタ";
            this.labelQuickFilter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtQuickFilter
            // 
            this.txtQuickFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtQuickFilter.Location = new System.Drawing.Point(153, 195);
            this.txtQuickFilter.Name = "txtQuickFilter";
            this.txtQuickFilter.Size = new System.Drawing.Size(1000, 23);
            this.txtQuickFilter.TabIndex = 13;
            this.txtQuickFilter.TextChanged += new System.EventHandler(this.TxtQuickFilter_TextChanged);
            // 
            // labelOptions
            // 
            this.labelOptions.AutoSize = true;
            this.labelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelOptions.Location = new System.Drawing.Point(3, 224);
            this.labelOptions.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.labelOptions.Name = "labelOptions";
            this.labelOptions.Size = new System.Drawing.Size(144, 48);
            this.labelOptions.TabIndex = 14;
            this.labelOptions.Text = "その他";
            this.labelOptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowOptions
            // 
            this.flowOptions.AutoSize = true;
            this.flowOptions.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowOptions.Controls.Add(this.chkRecursive);
            this.flowOptions.Controls.Add(this.chkZip);
            this.flowOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowOptions.Location = new System.Drawing.Point(153, 227);
            this.flowOptions.Name = "flowOptions";
            this.flowOptions.Size = new System.Drawing.Size(1000, 42);
            this.flowOptions.TabIndex = 15;
            this.flowOptions.WrapContents = false;
            // 
            // chkRecursive
            // 
            this.chkRecursive.AutoSize = true;
            this.chkRecursive.Checked = true;
            this.chkRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRecursive.Location = new System.Drawing.Point(0, 3);
            this.chkRecursive.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chkRecursive.Name = "chkRecursive";
            this.chkRecursive.Size = new System.Drawing.Size(124, 19);
            this.chkRecursive.TabIndex = 0;
            this.chkRecursive.Text = "サブフォルダ検索";
            this.chkRecursive.UseVisualStyleBackColor = true;
            // 
            // chkZip
            // 
            this.chkZip.AutoSize = true;
            this.chkZip.Checked = true;
            this.chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkZip.Location = new System.Drawing.Point(136, 3);
            this.chkZip.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chkZip.Name = "chkZip";
            this.chkZip.Size = new System.Drawing.Size(118, 19);
            this.chkZip.TabIndex = 1;
            this.chkZip.Text = "ZIP 内も検索";
            this.chkZip.UseVisualStyleBackColor = true;
            // 
            // resultsGrid
            // 
            this.resultsGrid.AllowUserToAddRows = false;
            this.resultsGrid.AllowUserToDeleteRows = false;
            this.resultsGrid.AllowUserToResizeRows = false;
            this.resultsGrid.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.resultsGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.resultsGrid.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleHorizontal;
            this.resultsGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(241)))), ((int)(((byte)(251)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.resultsGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.resultsGrid.ColumnHeadersHeight = 36;
            this.resultsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnPath,
            this.columnExt,
            this.columnEntry,
            this.columnLine,
            this.columnSnippet});
            this.resultsGrid.ContextMenuStrip = this.contextMenu;
            this.resultsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultsGrid.EnableHeadersVisualStyles = false;
            this.resultsGrid.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(221)))), ((int)(((byte)(221)))), ((int)(((byte)(221)))));
            this.resultsGrid.Location = new System.Drawing.Point(0, 340);
            this.resultsGrid.MultiSelect = false;
            this.resultsGrid.Name = "resultsGrid";
            this.resultsGrid.ReadOnly = true;
            this.resultsGrid.RowHeadersVisible = false;
            this.resultsGrid.RowTemplate.Height = 28;
            this.resultsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.resultsGrid.ShowEditingIcon = false;
            this.resultsGrid.Size = new System.Drawing.Size(1180, 360);
            this.resultsGrid.TabIndex = 2;
            this.resultsGrid.VirtualMode = true;
            this.resultsGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsGrid_CellDoubleClick);
            this.resultsGrid.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ResultsGrid_CellMouseDown);
            this.resultsGrid.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.ResultsGrid_CellPainting);
            this.resultsGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.ResultsGrid_CellValueNeeded);
            this.resultsGrid.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.ResultsGrid_ColumnHeaderMouseClick);
            this.resultsGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ResultsGrid_KeyDown);
            this.resultsGrid.MouseLeave += new System.EventHandler(this.ResultsGrid_MouseLeave);
            this.resultsGrid.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.ResultsGrid_CellMouseEnter);
            // 
            // columnPath
            //
            this.columnPath.FillWeight = 40F;
            this.columnPath.HeaderText = "Path";
            this.columnPath.Name = "columnPath";
            this.columnPath.ReadOnly = true;
            //
            // columnExt
            //
            this.columnExt.FillWeight = 8F;
            this.columnExt.HeaderText = "Ext";
            this.columnExt.Name = "columnExt";
            this.columnExt.ReadOnly = true;
            //
            // columnEntry
            //
            this.columnEntry.FillWeight = 15F;
            this.columnEntry.HeaderText = "Entry";
            this.columnEntry.Name = "columnEntry";
            this.columnEntry.ReadOnly = true;
            // 
            // columnLine
            // 
            this.columnLine.FillWeight = 8F;
            this.columnLine.HeaderText = "Line";
            this.columnLine.Name = "columnLine";
            this.columnLine.ReadOnly = true;
            // 
            // columnSnippet
            // 
            this.columnSnippet.FillWeight = 37F;
            this.columnSnippet.HeaderText = "Snippet";
            this.columnSnippet.Name = "columnSnippet";
            this.columnSnippet.ReadOnly = true;
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.White;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusElapsed,
            this.statusFiles,
            this.statusHits,
            this.statusMessage});
            this.statusStrip.Location = new System.Drawing.Point(0, 700);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.statusStrip.Size = new System.Drawing.Size(1180, 26);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusElapsed
            // 
            this.statusElapsed.Margin = new System.Windows.Forms.Padding(0, 3, 12, 2);
            this.statusElapsed.Name = "statusElapsed";
            this.statusElapsed.Size = new System.Drawing.Size(103, 21);
            this.statusElapsed.Text = "経過: 00:00:00";
            // 
            // statusFiles
            // 
            this.statusFiles.Margin = new System.Windows.Forms.Padding(0, 3, 12, 2);
            this.statusFiles.Name = "statusFiles";
            this.statusFiles.Size = new System.Drawing.Size(98, 21);
            this.statusFiles.Text = "処理: 0 / 0 件";
            // 
            // statusHits
            // 
            this.statusHits.Margin = new System.Windows.Forms.Padding(0, 3, 12, 2);
            this.statusHits.Name = "statusHits";
            this.statusHits.Size = new System.Drawing.Size(77, 21);
            this.statusHits.Text = "ヒット: 0 件";
            // 
            // statusMessage
            // 
            this.statusMessage.Margin = new System.Windows.Forms.Padding(0, 3, 0, 2);
            this.statusMessage.Name = "statusMessage";
            this.statusMessage.Size = new System.Drawing.Size(35, 21);
            this.statusMessage.Spring = true;
            this.statusMessage.Text = "Ready";
            this.statusMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiTimer
            // 
            this.uiTimer.Interval = 75;
            this.uiTimer.Tick += new System.EventHandler(this.UiTimer_Tick);
            // 
            // filterTimer
            // 
            this.filterTimer.Interval = 250;
            this.filterTimer.Tick += new System.EventHandler(this.FilterTimer_Tick);
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuCopyPath,
            this.menuOpenExplorer,
            this.menuOpenFolder});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(199, 70);
            this.contextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenu_Opening);
            // 
            // menuCopyPath
            // 
            this.menuCopyPath.Name = "menuCopyPath";
            this.menuCopyPath.Size = new System.Drawing.Size(198, 22);
            this.menuCopyPath.Text = "フルパスをコピー";
            this.menuCopyPath.Click += new System.EventHandler(this.MenuCopyPath_Click);
            // 
            // menuOpenExplorer
            // 
            this.menuOpenExplorer.Name = "menuOpenExplorer";
            this.menuOpenExplorer.Size = new System.Drawing.Size(198, 22);
            this.menuOpenExplorer.Text = "エクスプローラーで開く";
            this.menuOpenExplorer.Click += new System.EventHandler(this.MenuOpenExplorer_Click);
            // 
            // menuOpenFolder
            // 
            this.menuOpenFolder.Name = "menuOpenFolder";
            this.menuOpenFolder.Size = new System.Drawing.Size(198, 22);
            this.menuOpenFolder.Text = "親フォルダを開く";
            this.menuOpenFolder.Click += new System.EventHandler(this.MenuOpenFolder_Click);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1180, 726);
            this.Controls.Add(this.resultsGrid);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.panelConditions);
            this.Controls.Add(this.toolStripMain);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(960, 600);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FastFileFinder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.toolStripMain.ResumeLayout(false);
            this.toolStripMain.PerformLayout();
            this.panelConditions.ResumeLayout(false);
            this.panelConditions.PerformLayout();
            this.tableConditions.ResumeLayout(false);
            this.tableConditions.PerformLayout();
            this.flowRoot.ResumeLayout(false);
            this.flowRoot.PerformLayout();
            this.flowQuery.ResumeLayout(false);
            this.flowQuery.PerformLayout();
            this.flowParallel.ResumeLayout(false);
            this.flowParallel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numParallel)).EndInit();
            this.flowOffice.ResumeLayout(false);
            this.flowOffice.PerformLayout();
            this.flowOptions.ResumeLayout(false);
            this.flowOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultsGrid)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMain;
        private System.Windows.Forms.ToolStripButton toolStripButtonStart;
        private System.Windows.Forms.ToolStripButton toolStripButtonCancel;
        private System.Windows.Forms.ToolStripButton toolStripButtonExport;
        private System.Windows.Forms.Panel panelConditions;
        private System.Windows.Forms.TableLayoutPanel tableConditions;
        private System.Windows.Forms.Label labelRoot;
        private System.Windows.Forms.FlowLayoutPanel flowRoot;
        private System.Windows.Forms.TextBox txtRoot;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ComboBox comboRecent;
        private System.Windows.Forms.Label labelQuery;
        private System.Windows.Forms.FlowLayoutPanel flowQuery;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.CheckBox chkRegex;
        private System.Windows.Forms.Label labelExtensions;
        private System.Windows.Forms.TextBox txtExtensions;
        private System.Windows.Forms.Label labelExclude;
        private System.Windows.Forms.TextBox txtExclude;
        private System.Windows.Forms.Label labelParallel;
        private System.Windows.Forms.FlowLayoutPanel flowParallel;
        private System.Windows.Forms.NumericUpDown numParallel;
        private System.Windows.Forms.Label lblParallelHint;
        private System.Windows.Forms.Label labelOffice;
        private System.Windows.Forms.FlowLayoutPanel flowOffice;
        private System.Windows.Forms.CheckBox chkWord;
        private System.Windows.Forms.CheckBox chkExcel;
        private System.Windows.Forms.CheckBox chkLegacy;
        private System.Windows.Forms.Label labelQuickFilter;
        private System.Windows.Forms.TextBox txtQuickFilter;
        private System.Windows.Forms.Label labelOptions;
        private System.Windows.Forms.FlowLayoutPanel flowOptions;
        private System.Windows.Forms.CheckBox chkRecursive;
        private System.Windows.Forms.CheckBox chkZip;
        private System.Windows.Forms.DataGridView resultsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnExt;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnEntry;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLine;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSnippet;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusElapsed;
        private System.Windows.Forms.ToolStripStatusLabel statusFiles;
        private System.Windows.Forms.ToolStripStatusLabel statusHits;
        private System.Windows.Forms.ToolStripStatusLabel statusMessage;
        private System.Windows.Forms.Timer uiTimer;
        private System.Windows.Forms.Timer filterTimer;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem menuCopyPath;
        private System.Windows.Forms.ToolStripMenuItem menuOpenExplorer;
        private System.Windows.Forms.ToolStripMenuItem menuOpenFolder;
    }
}
