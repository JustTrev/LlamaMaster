namespace LlamaMaster;

public class SettingsForm : Form
{
    private readonly MainForm _owner;
    private CheckBox chkUpdate;
    private CheckBox chkDarkMode;

    public SettingsForm(MainForm owner)
    {
        _owner = owner;

        Text = "Settings";
        Size = new Size(440, 260);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9.5F);

        this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        // Check for updates
        chkUpdate = new CheckBox
        {
            Text = "Check for updates on startup",
            AutoSize = true,
            Location = new Point(30, 35),
            Checked = owner.CheckForUpdatesOnStartup
        };

        // Dark Mode
        chkDarkMode = new CheckBox
        {
            Text = "Enable Dark Mode",
            AutoSize = true,
            Location = new Point(30, 75),
            Checked = owner.DarkMode
        };

        var btnOk = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(230, 160),
            Size = new Size(85, 34)
        };

        var btnCancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(325, 160),
            Size = new Size(85, 34)
        };

        btnOk.Click += (s, e) =>
        {
            _owner.CheckForUpdatesOnStartup = chkUpdate.Checked;
            _owner.DarkMode = chkDarkMode.Checked;
            _owner.ApplyTheme();          // apply immediately
        };

        Controls.Add(chkUpdate);
        Controls.Add(chkDarkMode);
        Controls.Add(btnOk);
        Controls.Add(btnCancel);

        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }
}