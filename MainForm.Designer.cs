namespace LlamaMaster;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Text = "Llama.cpp Master – Runtime + Script Designer";
        this.MinimumSize = new Size(1100, 820);
        this.Size = new Size(1220, 900);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 9F);
        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        // ==================== MENU STRIP ====================
        menuStrip = new MenuStrip();

        // File
        var menuFile = new ToolStripMenuItem("&File");
        var menuSaveBat = new ToolStripMenuItem("Save .bat", null, menuSaveBat_Click);
        var menuLaunch = new ToolStripMenuItem("Save & Launch", null, menuLaunch_Click);
        var menuExit = new ToolStripMenuItem("E&xit", null, menuExit_Click);
        menuFile.DropDownItems.AddRange(new ToolStripItem[] { menuSaveBat, menuLaunch, new ToolStripSeparator(), menuExit });

        // Settings
        var menuSettings = new ToolStripMenuItem("&Settings", null, menuSettings_Click);

        // Help
        var menuHelp = new ToolStripMenuItem("&Help");
        var menuHelpItem = new ToolStripMenuItem("View Help", null, menuHelp_Click);
        var menuCheckUpdate = new ToolStripMenuItem("Check for Updates", null, menuCheckUpdate_Click);
        var menuAbout = new ToolStripMenuItem("About Me", null, menuAbout_Click);
        menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuHelpItem, menuCheckUpdate, new ToolStripSeparator(), menuAbout });

        menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuSettings, menuHelp });
        this.MainMenuStrip = menuStrip;
        this.Controls.Add(menuStrip);

        // ==================== SPLIT CONTAINER ====================
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BorderStyle = BorderStyle.FixedSingle,
            SplitterWidth = 6
        };

        this.Shown += (s, e) =>
        {
            split.Panel1MinSize = 460;
            split.Panel2MinSize = 420;
            if (split.Width > 900)
                split.SplitterDistance = 490;
        };

        // ---------- LEFT PANEL (same as before) ----------
        var leftScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(8) };
        var leftContent = new Panel { Dock = DockStyle.Top, AutoSize = true, Width = 460 };

        // (… all the groups: Sampling, Server, Network, Profile, Runtime …)
        // For brevity I’m reusing the exact same left-side construction
        // from the previous full Designer. Just keep your existing left side code here.

        // ----- Sampling -----
        var grpSample = MakeGroup("Sampling");
        chkEnableSampling = new CheckBox { Text = "Override sampling parameters", Checked = true, AutoSize = true };
        trackTemp = CreateTrackBar(0, 200, 70);
        lblTemp = new Label { Text = "0.70", Width = 40, TextAlign = ContentAlignment.MiddleRight };
        trackTopP = CreateTrackBar(0, 100, 80);
        lblTopP = new Label { Text = "0.80", Width = 40, TextAlign = ContentAlignment.MiddleRight };
        trackTopK = CreateTrackBar(0, 200, 20);
        lblTopK = new Label { Text = "20", Width = 40, TextAlign = ContentAlignment.MiddleRight };
        trackRepeat = CreateTrackBar(100, 200, 105);
        lblRepeat = new Label { Text = "1.05", Width = 40, TextAlign = ContentAlignment.MiddleRight };

        var sampleTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3, Padding = new Padding(6) };
        sampleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        sampleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        sampleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 45));
        sampleTable.Controls.Add(chkEnableSampling, 0, 0);
        sampleTable.SetColumnSpan(chkEnableSampling, 3);
        AddSliderRow(sampleTable, "Temperature:", trackTemp, lblTemp, 1);
        AddSliderRow(sampleTable, "Top-P:", trackTopP, lblTopP, 2);
        AddSliderRow(sampleTable, "Top-K:", trackTopK, lblTopK, 3);
        AddSliderRow(sampleTable, "Repeat penalty:", trackRepeat, lblRepeat, 4);
        grpSample.Controls.Add(sampleTable);
        leftContent.Controls.Add(grpSample);

        // ----- Server -----
        var grpServer = MakeGroup("Server Settings");
        chkEnableContext = new CheckBox { Text = "Context size", Checked = true, AutoSize = true };
        chkEnableBatch = new CheckBox { Text = "Batch / uBatch", Checked = true, AutoSize = true };
        chkEnableNgl = new CheckBox { Text = "GPU layers (-ngl)", Checked = true, AutoSize = true };
        chkEnableFlash = new CheckBox { Text = "Flash Attention", Checked = true, AutoSize = true };
        txtPort = new TextBox { Width = 90, Text = "8081" };
        numContext = new NumericUpDown { Minimum = 512, Maximum = 131072, Value = 65536, Increment = 512, Width = 110 };
        numBatch = new NumericUpDown { Minimum = 128, Maximum = 8192, Value = 2048, Increment = 128, Width = 80 };
        numUBatch = new NumericUpDown { Minimum = 32, Maximum = 2048, Value = 512, Increment = 32, Width = 80 };
        numNgl = new NumericUpDown { Minimum = 0, Maximum = 999, Value = 99, Width = 80 };
        cmbFlash = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        cmbFlash.Items.AddRange(new[] { "off", "on", "auto" });
        cmbFlash.SelectedItem = "off";

        var serverTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(6) };
        serverTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        serverTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(serverTable, "HTTP Port:", txtPort);
        AddRow(serverTable, chkEnableContext, numContext);
        AddRow(serverTable, chkEnableBatch, CreateBatchPanel());
        AddRow(serverTable, chkEnableNgl, numNgl);
        AddRow(serverTable, chkEnableFlash, cmbFlash);
        grpServer.Controls.Add(serverTable);
        leftContent.Controls.Add(grpServer);

        // ----- Network -----
        var grpNet = MakeGroup("Network / RPC");
        txtSpareIp = new TextBox { Width = 160, Text = "192.168.0.21" };
        txtRpcPort = new TextBox { Width = 80, Text = "50052" };
        lblSplit = new Label { Text = "Tensor split ratio (Main : Spare)", AutoSize = true };
        numSplitMain = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 5, Width = 55 };
        numSplitSpare = new NumericUpDown { Minimum = 1, Maximum = 20, Value = 3, Width = 55 };
        lblSplitPreview = new Label { Text = "→  -ts 5,3", AutoSize = true, ForeColor = Color.DarkBlue };

        var netTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(6) };
        netTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        netTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(netTable, "Spare PC IP:", txtSpareIp);
        AddRow(netTable, "RPC Port:", txtRpcPort);
        var splitPanel = new FlowLayoutPanel { AutoSize = true };
        splitPanel.Controls.Add(numSplitMain);
        splitPanel.Controls.Add(new Label { Text = " : ", AutoSize = true, Margin = new Padding(3, 5, 3, 0) });
        splitPanel.Controls.Add(numSplitSpare);
        splitPanel.Controls.Add(lblSplitPreview);
        netTable.Controls.Add(lblSplit, 0, 2);
        netTable.Controls.Add(splitPanel, 1, 2);
        grpNet.Controls.Add(netTable);
        leftContent.Controls.Add(grpNet);

        // ----- Profile -----
        var grpProfile = MakeGroup("2. Profile");
        chkUseRpc = new CheckBox { Text = "Use RPC (split to spare PC)", AutoSize = true, Checked = true };
        chkUseMoe = new CheckBox { Text = "Enable MoE on CPU", AutoSize = true, Checked = true };
        txtMoeArg = new TextBox { Width = 300, Text = "--cpu-moe" };
        var profilePanel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(6) };
        profilePanel.Controls.Add(chkUseRpc);
        profilePanel.Controls.Add(chkUseMoe);
        profilePanel.Controls.Add(new Label { Text = "MoE argument:", AutoSize = true, Margin = new Padding(0, 6, 0, 0) });
        profilePanel.Controls.Add(txtMoeArg);
        grpProfile.Controls.Add(profilePanel);
        leftContent.Controls.Add(grpProfile);

        // ----- Runtime -----
        var grpRuntime = MakeGroup("1. Runtime (CUDA version)");
        cmbCuda = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
        cmbCuda.Items.AddRange(new[] { "12.4", "13.5" });
        cmbCuda.SelectedItem = "13.5";
        txtBuildTag = new TextBox { Width = 110, Text = "b12633" };
        btnDownload = new Button { Text = "Download / Ensure Runtime", Size = new Size(200, 30) };
        lblRuntimeStatus = new Label { Text = "No runtime loaded", AutoSize = true, ForeColor = Color.DarkOrange };

        var rtTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(6) };
        rtTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        rtTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rtTable.Controls.Add(new Label { Text = "CUDA version:", AutoSize = true }, 0, 0);
        rtTable.Controls.Add(cmbCuda, 1, 0);
        rtTable.Controls.Add(new Label { Text = "Build tag:", AutoSize = true }, 0, 1);
        rtTable.Controls.Add(txtBuildTag, 1, 1);
        rtTable.Controls.Add(btnDownload, 1, 2);
        rtTable.SetColumnSpan(lblRuntimeStatus, 2);
        rtTable.Controls.Add(lblRuntimeStatus, 0, 3);
        grpRuntime.Controls.Add(rtTable);
        leftContent.Controls.Add(grpRuntime);

        foreach (Control c in leftContent.Controls) c.Dock = DockStyle.Top;
        leftScroll.Controls.Add(leftContent);
        split.Panel1.Controls.Add(leftScroll);

        // ---------- RIGHT PANEL ----------
        var rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

        var grpPaths = MakeGroup("Paths");
        txtLlamaCache = new TextBox { Text = @"C:\AI_Models", Dock = DockStyle.Fill };
        btnBrowseCache = new Button { Text = "...", Width = 40, Height = 27 };
        txtWorkingDir = new TextBox { Dock = DockStyle.Fill };
        btnBrowseWork = new Button { Text = "...", Width = 40, Height = 27 };

        var pathsTable = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Padding = new Padding(8) };
        pathsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        pathsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
        pathsTable.Controls.Add(new Label { Text = "LLAMA_CACHE (models):", AutoSize = true }, 0, 0);
        pathsTable.SetColumnSpan(pathsTable.GetControlFromPosition(0, 0)!, 2);
        pathsTable.Controls.Add(txtLlamaCache, 0, 1);
        pathsTable.Controls.Add(btnBrowseCache, 1, 1);
        pathsTable.Controls.Add(new Label { Text = "Working dir (runtime):", AutoSize = true, Margin = new Padding(0, 12, 0, 0) }, 0, 2);
        pathsTable.SetColumnSpan(pathsTable.GetControlFromPosition(0, 2)!, 2);
        pathsTable.Controls.Add(txtWorkingDir, 0, 3);
        pathsTable.Controls.Add(btnBrowseWork, 1, 3);
        grpPaths.Controls.Add(pathsTable);
        grpPaths.Dock = DockStyle.Top;
        grpPaths.Height = 150;

        var progressPanel = new Panel { Dock = DockStyle.Bottom, Height = 48, Padding = new Padding(0, 6, 0, 0) };
        progressBar = new ProgressBar { Dock = DockStyle.Top, Height = 18 };
        lblProgress = new Label { Dock = DockStyle.Bottom, Height = 20, Text = "Ready" };
        progressPanel.Controls.Add(progressBar);
        progressPanel.Controls.Add(lblProgress);

        var buttonBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 6, 0, 0)
        };
        btnLaunch = new Button { Text = "Save & Launch", Size = new Size(140, 34) };
        btnSaveBat = new Button { Text = "Save .bat", Size = new Size(110, 34) };
        btnGenerate = new Button { Text = "Generate Preview", Size = new Size(140, 34) };
        buttonBar.Controls.AddRange(new Control[] { btnLaunch, btnSaveBat, btnGenerate });

        txtPreview = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9.5F),
            ReadOnly = true,
            BackColor = Color.FromArgb(30, 30, 30),
            ForeColor = Color.LightGreen,
            DetectUrls = true,
            BorderStyle = BorderStyle.None
        };
        txtPreview.LinkClicked += TxtPreview_LinkClicked;

        rightPanel.Controls.Add(txtPreview);
        rightPanel.Controls.Add(progressPanel);
        rightPanel.Controls.Add(buttonBar);
        rightPanel.Controls.Add(grpPaths);
        split.Panel2.Controls.Add(rightPanel);

        // Events
        btnDownload.Click += btnDownload_Click;
        btnGenerate.Click += btnGenerate_Click;
        btnSaveBat.Click += btnSaveBat_Click;
        btnLaunch.Click += btnLaunch_Click;
        btnBrowseCache.Click += (s, e) => BrowseFolder(txtLlamaCache);
        btnBrowseWork.Click += (s, e) => BrowseFolder(txtWorkingDir);

        trackTemp.Scroll += (s, e) => lblTemp.Text = (trackTemp.Value / 100.0).ToString("0.00");
        trackTopP.Scroll += (s, e) => lblTopP.Text = (trackTopP.Value / 100.0).ToString("0.00");
        trackTopK.Scroll += (s, e) => lblTopK.Text = trackTopK.Value.ToString();
        trackRepeat.Scroll += (s, e) => lblRepeat.Text = (trackRepeat.Value / 100.0).ToString("0.00");

        numSplitMain.ValueChanged += UpdateSplitPreview;
        numSplitSpare.ValueChanged += UpdateSplitPreview;
        chkUseRpc.CheckedChanged += (s, e) =>
        {
            bool on = chkUseRpc.Checked;
            numSplitMain.Enabled = numSplitSpare.Enabled = on;
            lblSplit.Enabled = lblSplitPreview.Enabled = on;
        };

        this.Controls.Add(split);
        // MenuStrip must be added last so it stays on top
        this.Controls.Add(menuStrip);
        this.MainMenuStrip = menuStrip;
    }

    // helpers (same as before)
    private GroupBox MakeGroup(string title) => new()
    {
        Text = title,
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        Padding = new Padding(8),
        Dock = DockStyle.Top,
        Margin = new Padding(0, 0, 0, 10)
    };

    private TrackBar CreateTrackBar(int min, int max, int value) => new()
    {
        Minimum = min,
        Maximum = max,
        Value = value,
        TickFrequency = Math.Max(1, (max - min) / 10),
        SmallChange = 1,
        LargeChange = 5,
        Width = 180,
        Height = 30
    };

    private void AddRow(TableLayoutPanel t, string label, Control c)
    {
        int r = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, r);
        c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        t.Controls.Add(c, 1, r);
    }

    private void AddRow(TableLayoutPanel t, CheckBox chk, Control c)
    {
        int r = t.RowCount++;
        t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        t.Controls.Add(chk, 0, r);
        c.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        t.Controls.Add(c, 1, r);
    }

    private void AddSliderRow(TableLayoutPanel t, string label, TrackBar track, Label valueLabel, int row)
    {
        t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        t.Controls.Add(track, 1, row);
        t.Controls.Add(valueLabel, 2, row);
    }

    private Control CreateBatchPanel()
    {
        var p = new FlowLayoutPanel { AutoSize = true };
        p.Controls.Add(numBatch);
        p.Controls.Add(new Label { Text = " / ", AutoSize = true, Margin = new Padding(2, 6, 2, 0) });
        p.Controls.Add(numUBatch);
        return p;
    }

    // Controls
    private MenuStrip menuStrip;

    private ComboBox cmbCuda;
    private TextBox txtBuildTag;
    private Button btnDownload;
    private Label lblRuntimeStatus;

    private CheckBox chkUseRpc, chkUseMoe, chkEnableSampling;
    private CheckBox chkEnableContext, chkEnableBatch, chkEnableNgl, chkEnableFlash;
    private TextBox txtMoeArg, txtSpareIp, txtRpcPort, txtPort;
    private NumericUpDown numSplitMain, numSplitSpare, numContext, numBatch, numUBatch, numNgl;
    private Label lblSplit, lblSplitPreview, lblTemp, lblTopP, lblTopK, lblRepeat;
    private ComboBox cmbFlash;
    private TrackBar trackTemp, trackTopP, trackTopK, trackRepeat;

    private TextBox txtLlamaCache, txtWorkingDir;
    private Button btnBrowseCache, btnBrowseWork;
    private RichTextBox txtPreview;
    private Button btnGenerate, btnSaveBat, btnLaunch;

    private ProgressBar progressBar;
    private Label lblProgress;
}