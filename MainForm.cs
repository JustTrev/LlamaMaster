using System.Diagnostics;
using System.Text;

namespace LlamaMaster;

public partial class MainForm : Form
{
    private Process? _serverProcess;
    private readonly object _logLock = new();
    private CancellationTokenSource? _downloadCts;

    // Settings
    public bool CheckForUpdatesOnStartup { get; set; } = true;
    public bool DarkMode { get; set; } = true;   // default to dark

    public MainForm()
    {
        InitializeComponent();
        LoadDefaults();
        LoadUserSettings();
        ApplyTheme();                    

        if (CheckForUpdatesOnStartup)
            _ = CheckForUpdatesAsync(silent: true);
    }

    private void LoadDefaults()
    {
        cmbCuda.SelectedItem = "13.5";
        txtBuildTag.Text = "b12633";

        chkUseRpc.Checked = true;
        chkUseMoe.Checked = true;
        txtMoeArg.Text = "--cpu-moe";

        txtSpareIp.Text = "192.168.0.21";
        txtRpcPort.Text = "50052";
        numSplitMain.Value = 5;
        numSplitSpare.Value = 3;
        UpdateSplitPreview(null, EventArgs.Empty);

        txtPort.Text = "8080";
        chkEnableContext.Checked = true;
        numContext.Value = 65536;
        chkEnableBatch.Checked = true;
        numBatch.Value = 2048;
        numUBatch.Value = 512;
        chkEnableNgl.Checked = true;
        numNgl.Value = 99;
        chkEnableFlash.Checked = true;
        cmbFlash.SelectedItem = "off";

        chkEnableSampling.Checked = true;
        trackTemp.Value = 70;
        trackTopP.Value = 80;
        trackTopK.Value = 20;
        trackRepeat.Value = 105;

        lblTemp.Text = "0.70";
        lblTopP.Text = "0.80";
        lblTopK.Text = "20";
        lblRepeat.Text = "1.05";

        txtLlamaCache.Text = @$"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\.cache\huggingface\hub\";
        progressBar.Value = 0;
        lblProgress.Text = "Ready";
    }

    private void LoadUserSettings()
    {
        try
        {
            string settingsFile = Path.Combine(Application.StartupPath, "master-settings.txt");
            if (!File.Exists(settingsFile)) return;

            foreach (var line in File.ReadAllLines(settingsFile))
            {
                if (line.StartsWith("CheckForUpdates="))
                    CheckForUpdatesOnStartup = line.EndsWith("True", StringComparison.OrdinalIgnoreCase);

                if (line.StartsWith("DarkMode="))
                    DarkMode = line.EndsWith("True", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch { }
    }

    private void SaveUserSettings()
    {
        try
        {
            string settingsFile = Path.Combine(Application.StartupPath, "master-settings.txt");
            File.WriteAllText(settingsFile,
                $"CheckForUpdates={CheckForUpdatesOnStartup}\n" +
                $"DarkMode={DarkMode}");
        }
        catch { }
    }
    public void ApplyTheme()
    {
        Color back = DarkMode ? Color.FromArgb(32, 32, 32) : SystemColors.Control;
        Color fore = DarkMode ? Color.WhiteSmoke : SystemColors.ControlText;
        Color panel = DarkMode ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
        Color group = DarkMode ? Color.FromArgb(37, 37, 38) : SystemColors.Control;
        Color inputBack = DarkMode ? Color.FromArgb(60, 60, 60) : Color.White;
        Color inputFore = DarkMode ? Color.White : Color.Black;

        this.BackColor = back;
        this.ForeColor = fore;

        ApplyThemeToControl(this, back, fore, panel, group, inputBack, inputFore);

        // Special treatment for the log window
        if (txtPreview != null)
        {
            txtPreview.BackColor = DarkMode ? Color.FromArgb(25, 25, 25) : Color.White;
            txtPreview.ForeColor = DarkMode ? Color.LightGreen : Color.Black;
        }
    }

    private void ApplyThemeToControl(Control parent, Color back, Color fore, Color panel, Color group, Color inputBack, Color inputFore)
    {
        foreach (Control c in parent.Controls)
        {
            if (c is MenuStrip ms)
            {
                ms.BackColor = panel;
                ms.ForeColor = fore;
                foreach (ToolStripItem item in ms.Items)
                    item.ForeColor = fore;
            }
            else if (c is GroupBox)
            {
                c.BackColor = group;
                c.ForeColor = fore;
            }
            else if (c is Panel || c is SplitContainer || c is FlowLayoutPanel || c is TableLayoutPanel)
            {
                c.BackColor = panel;
                c.ForeColor = fore;
            }
            else if (c is TextBox or RichTextBox or ComboBox or NumericUpDown)
            {
                c.BackColor = inputBack;
                c.ForeColor = inputFore;
            }
            else if (c is Button btn)
            {
                btn.FlatStyle = DarkMode ? FlatStyle.Flat : FlatStyle.Standard;
                btn.BackColor = DarkMode ? Color.FromArgb(70, 70, 70) : SystemColors.Control;
                btn.ForeColor = fore;
            }
            else if (c is Label or CheckBox)
            {
                c.ForeColor = fore;
                c.BackColor = Color.Transparent;
            }
            else if (c is TrackBar or ProgressBar)
            {
                // leave default
            }
            else
            {
                c.BackColor = back;
                c.ForeColor = fore;
            }

            // recurse
            if (c.HasChildren)
                ApplyThemeToControl(c, back, fore, panel, group, inputBack, inputFore);
        }
    }

    // ==================== MENU HANDLERS ====================

    private void menuSaveBat_Click(object sender, EventArgs e) => btnSaveBat_Click(sender, e);
    private void menuLaunch_Click(object sender, EventArgs e) => btnLaunch_Click(sender, e);

    private void menuExit_Click(object sender, EventArgs e)
    {
        StopServer();
        Close();
    }

    private void menuSettings_Click(object sender, EventArgs e)
    {
        using var frm = new SettingsForm(this);
        frm.ShowDialog(this);
        SaveUserSettings();
    }

    private void menuAbout_Click(object sender, EventArgs e)
    {
        MessageBox.Show(
            "Llama.cpp Master\n" +
            "Version 1.0\n\n" +
            "A modern GUI frontend for llama.cpp server + RPC.\n" +
            "Built for easy multi-GPU and multi-PC (RPC) setups.\n\n" +
            "Created by: xSoMuchPotx + Grok (xAI) 20261124\n" +
            "Developed with love for the local AI community.\n\n" +
            "Features:\n" +
            "• Automatic CUDA runtime downloader\n" +
            "• RPC tensor splitting\n" +
            "• Live server output with clickable links\n" +
            "• Dark mode & persistent settings\n\n" +
            "Enjoy!",
            "About Llama.cpp Master",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void menuHelp_Click(object sender, EventArgs e)
    {
        MessageBox.Show(
            "Quick Help\n\n" +
            "1. Choose CUDA version and click Download / Ensure Runtime\n" +
            "2. Configure RPC / sampling / server settings on the left\n" +
            "3. Click Save & Launch (or use File menu)\n" +
            "4. Output appears on the right – links are clickable\n\n" +
            "For the spare PC use the LlamaSlave application.",
            "Help",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private async void menuCheckUpdate_Click(object sender, EventArgs e)
    {
        await CheckForUpdatesAsync(silent: false);
    }

    private async Task CheckForUpdatesAsync(bool silent)
    {
        try
        {
            lblProgress.Text = "Checking for updates...";
            // Placeholder – replace with real version check later
            await Task.Delay(800);

            if (!silent)
            {
                MessageBox.Show("You are running the latest version (1.0).",
                    "Update Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            lblProgress.Text = "Ready";
        }
        catch (Exception ex)
        {
            if (!silent)
                MessageBox.Show("Update check failed:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ==================== EXISTING LOGIC ====================

    private async void btnDownload_Click(object sender, EventArgs e)
    {
        if (_downloadCts != null)
        {
            _downloadCts.Cancel();
            return;
        }

        btnDownload.Enabled = false;
        btnLaunch.Enabled = false;
        btnDownload.Text = "Cancel Download";
        _downloadCts = new CancellationTokenSource();

        try
        {
            var progress = new Progress<(string message, int percent)>(p =>
            {
                if (InvokeRequired)
                {
                    BeginInvoke(() => UpdateProgress(p.message, p.percent));
                    return;
                }
                UpdateProgress(p.message, p.percent);
            });

            var result = await RuntimeManager.EnsureRuntimeAsync(
                Application.StartupPath,
                cmbCuda.SelectedItem!.ToString()!,
                txtBuildTag.Text.Trim(),
                progress,
                _downloadCts.Token);

            txtWorkingDir.Text = result.RuntimeDir;
            lblRuntimeStatus.Text = $"Ready → {Path.GetFileName(result.RuntimeDir)}";
            lblRuntimeStatus.ForeColor = Color.Green;
            UpdateProgress("Runtime ready", 100);
        }
        catch (OperationCanceledException)
        {
            UpdateProgress("Download cancelled", 0);
            lblRuntimeStatus.Text = "Cancelled";
            lblRuntimeStatus.ForeColor = Color.DarkOrange;
        }
        catch (Exception ex)
        {
            UpdateProgress("Failed", 0);
            lblRuntimeStatus.Text = "Download failed";
            lblRuntimeStatus.ForeColor = Color.Red;
            MessageBox.Show(ex.Message, "Runtime Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _downloadCts = null;
            btnDownload.Text = "Download / Ensure Runtime";
            btnDownload.Enabled = true;
            btnLaunch.Enabled = true;
        }
    }

    private void UpdateProgress(string message, int percent)
    {
        progressBar.Value = Math.Clamp(percent, 0, 100);
        lblProgress.Text = $"{message}  ({percent}%)";
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        txtPreview.Text = BuildBatScript();
    }

    private void btnSaveBat_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Batch files (*.bat)|*.bat",
            FileName = "start-main-server.bat",
            InitialDirectory = string.IsNullOrWhiteSpace(txtWorkingDir.Text)
                ? Application.StartupPath
                : txtWorkingDir.Text
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        File.WriteAllText(sfd.FileName, BuildBatScript(), Encoding.ASCII);
        MessageBox.Show("Saved:\n" + sfd.FileName, "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void btnLaunch_Click(object sender, EventArgs e)
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            StopServer();
            return;
        }

        string workDir = txtWorkingDir.Text.Trim();
        if (string.IsNullOrWhiteSpace(workDir) || !Directory.Exists(workDir))
        {
            MessageBox.Show("Please download a runtime first.", "No runtime",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string exePath = Path.Combine(workDir, "llama-server.exe");
        if (!File.Exists(exePath))
        {
            MessageBox.Show("llama-server.exe not found in:\n" + workDir,
                "Missing executable", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var args = BuildServerArguments();

        txtPreview.Clear();
        AppendLog("Starting llama-server ...");
        AppendLog($"Executable : {exePath}");
        AppendLog($"Arguments  : {args}");
        AppendLog(new string('-', 70));

        UpdateProgress("Starting server...", 10);

        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                WorkingDirectory = workDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };

        _serverProcess.OutputDataReceived += (_, ev) => { if (ev.Data != null) AppendLog(ev.Data); };
        _serverProcess.ErrorDataReceived += (_, ev) => { if (ev.Data != null) AppendLog(ev.Data); };

        _serverProcess.Exited += (_, _) =>
        {
            BeginInvoke(() =>
            {
                AppendLog($"[EXIT] Exit code: {_serverProcess?.ExitCode}");
                btnLaunch.Text = "Save & Launch";
                UpdateProgress("Server stopped", 0);
            });
        };

        try
        {
            await Task.Run(() =>
            {
                _serverProcess.Start();
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();
            });

            btnLaunch.Text = "Stop Server";
            UpdateProgress("Server running", 100);
        }
        catch (Exception ex)
        {
            AppendLog("[ERROR] " + ex.Message);
            MessageBox.Show(ex.Message, "Failed to start", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateProgress("Start failed", 0);
        }
    }

    private string BuildServerArguments()
    {
        var args = new StringBuilder();
        args.Append("--host 127.0.0.1 ");
        args.Append($"--port {txtPort.Text.Trim()} ");

        if (chkUseRpc.Checked)
        {
            args.Append($"--rpc {txtSpareIp.Text.Trim()}:{txtRpcPort.Text.Trim()} ");
            args.Append("--device CUDA0,RPC0 --split-mode layer ");
            args.Append($"-ts {numSplitMain.Value},{numSplitSpare.Value} ");
        }

        if (chkEnableNgl.Checked)
            args.Append($"-ngl {numNgl.Value} ");

        if (chkUseMoe.Checked && !string.IsNullOrWhiteSpace(txtMoeArg.Text))
            args.Append($"{txtMoeArg.Text.Trim()} ");

        if (chkEnableContext.Checked)
            args.Append($"-c {numContext.Value} ");

        args.Append("--no-mmproj --models-max 1 ");

        if (chkEnableBatch.Checked)
        {
            args.Append($"--batch-size {numBatch.Value} ");
            args.Append($"--ubatch-size {numUBatch.Value} ");
        }

        if (chkEnableFlash.Checked)
            args.Append($"--flash-attn {cmbFlash.SelectedItem} ");

        if (chkEnableSampling.Checked)
        {
            string temp = (trackTemp.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string topp = (trackTopP.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string topk = trackTopK.Value.ToString();
            string repeat = (trackRepeat.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

            args.Append($"--temp {temp} --top-p {topp} --top-k {topk} --repeat-penalty {repeat} ");
        }

        return args.ToString();
    }

    private void StopServer()
    {
        if (_serverProcess == null || _serverProcess.HasExited) return;

        try
        {
            _serverProcess.Kill(entireProcessTree: true);
            AppendLog("[STOP] Kill signal sent.");
            UpdateProgress("Stopping...", 50);
        }
        catch (Exception ex)
        {
            AppendLog("[ERROR] " + ex.Message);
        }
    }

    private void AppendLog(string line)
    {
        if (txtPreview.InvokeRequired)
        {
            txtPreview.BeginInvoke(() => AppendLog(line));
            return;
        }

        lock (_logLock)
        {
            txtPreview.SelectionStart = txtPreview.TextLength;
            txtPreview.SelectionLength = 0;

            if (line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("failed", StringComparison.OrdinalIgnoreCase))
                txtPreview.SelectionColor = Color.OrangeRed;
            else if (line.Contains("listening", StringComparison.OrdinalIgnoreCase) ||
                     line.Contains("ready", StringComparison.OrdinalIgnoreCase))
                txtPreview.SelectionColor = Color.LightGreen;
            else if (line.StartsWith("[") || line.Contains("srv "))
                txtPreview.SelectionColor = Color.LightSkyBlue;
            else
                txtPreview.SelectionColor = Color.Gainsboro;

            txtPreview.AppendText(line + Environment.NewLine);

            if (txtPreview.TextLength > 400_000)
            {
                txtPreview.Select(0, 150_000);
                txtPreview.SelectedText = "";
            }

            txtPreview.SelectionStart = txtPreview.TextLength;
            txtPreview.ScrollToCaret();
        }
    }

    private void BrowseFolder(System.Windows.Forms.TextBox target)
    {
        using var dlg = new FolderBrowserDialog { SelectedPath = target.Text };
        if (dlg.ShowDialog() == DialogResult.OK)
            target.Text = dlg.SelectedPath;
    }

    private void UpdateSplitPreview(object? sender, EventArgs e)
    {
        lblSplitPreview.Text = $"→  -ts {numSplitMain.Value},{numSplitSpare.Value}";
    }

    private void TxtPreview_LinkClicked(object sender, LinkClickedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.LinkText,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not open link:\n" + ex.Message,
                "Link Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private string BuildBatScript()
    {
        var sb = new StringBuilder();
        sb.AppendLine("@echo off");
        sb.AppendLine("setlocal");
        sb.AppendLine("title llama.cpp Main Server");
        sb.AppendLine("cd /d \"%~dp0\"");
        sb.AppendLine($"set \"USE_RPC={(chkUseRpc.Checked ? "1" : "0")}\"");
        sb.AppendLine($"set \"MOE_ARG={(chkUseMoe.Checked ? txtMoeArg.Text.Trim() : "")}\"");
        sb.AppendLine($"set \"SPARE_IP={txtSpareIp.Text.Trim()}\"");
        sb.AppendLine($"set \"RPC_PORT={txtRpcPort.Text.Trim()}\"");
        sb.AppendLine($"set \"PORT={txtPort.Text.Trim()}\"");
        sb.AppendLine($"set \"CONTEXT={(chkEnableContext.Checked ? numContext.Value.ToString() : "")}\"");
        sb.AppendLine($"set \"BATCH={(chkEnableBatch.Checked ? numBatch.Value.ToString() : "")}\"");
        sb.AppendLine($"set \"UBATCH={(chkEnableBatch.Checked ? numUBatch.Value.ToString() : "")}\"");
        sb.AppendLine($"set \"NGL_ARG={(chkEnableNgl.Checked ? $"-ngl {numNgl.Value}" : "")}\"");
        sb.AppendLine($"set \"FLASH={(chkEnableFlash.Checked ? cmbFlash.SelectedItem : "")}\"");
        sb.AppendLine("set \"CACHE_ARGS=\"");

        if (chkEnableSampling.Checked)
        {
            string temp = (trackTemp.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            string topp = (trackTopP.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            sb.AppendLine($"set \"SAMPLER_ARGS=--temp {temp} --top-p {topp} --top-k {trackTopK.Value} --repeat-penalty {(trackRepeat.Value / 100.0).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}\"");
        }
        else sb.AppendLine("set \"SAMPLER_ARGS=\"");

        sb.AppendLine($"set \"LLAMA_CACHE={txtLlamaCache.Text.Trim()}\"");
        sb.AppendLine("set \"RPC_ARGS=\"");
        sb.AppendLine($"if \"%USE_RPC%\"==\"1\" set \"RPC_ARGS=--rpc %SPARE_IP%:%RPC_PORT% --device CUDA0,RPC0 --split-mode layer -ts {numSplitMain.Value},{numSplitSpare.Value}\"");
        sb.AppendLine("llama-server.exe --host 127.0.0.1 --port %PORT% --model-dir %LLAMA_CACHE% %RPC_ARGS% %NGL_ARG% %MOE_ARG% -c %CONTEXT% --no-mmproj --models-max 1 --batch-size %BATCH% --ubatch-size %UBATCH% --flash-attn %FLASH% %SAMPLER_ARGS% -ub 512");
        return sb.ToString();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _downloadCts?.Cancel();
        StopServer();
        SaveUserSettings();
        base.OnFormClosing(e);
    }
}