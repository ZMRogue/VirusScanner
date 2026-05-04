using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

partial class VirusScannerApp : Form
{
    Button btnScan, btnClean, btnReport;
    RichTextBox txtResults;
    ProgressBar progressBar;
    Label lblStatus;
    int threatCount = 0;

    string[] badProcesses = {
        "xmrig", "minerd", "remcos", "njrat",
        "darkcomet", "quasar", "asyncrat", "nanocore"
    };

    string[] suspiciousKeywords = {
        "temp", ".vbs", ".bat", ".cmd", ".ps1"
    };

    string[] dangerousExtensions = {
        ".exe", ".bat", ".vbs", ".cmd", ".ps1", ".scr", ".hta"
    };

    public VirusScannerApp()
    {
        this.Text = "🛡️ Virus Scanner";
        this.Size = new Size(700, 550);
        this.BackColor = Color.FromArgb(15, 15, 25);
        this.ForeColor = Color.White;
        this.StartPosition = FormStartPosition.CenterScreen;

        Label lblTitle = new Label();
        lblTitle.Text = "🛡️ VIRUS SCANNER & CLEANER";
        lblTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        lblTitle.ForeColor = Color.Cyan;
        lblTitle.Location = new Point(20, 15);
        lblTitle.Size = new Size(500, 35);

        btnScan = new Button();
        btnScan.Text = "▶ SCAN NOW";
        btnScan.Location = new Point(20, 60);
        btnScan.Size = new Size(150, 40);
        btnScan.BackColor = Color.FromArgb(0, 120, 215);
        btnScan.ForeColor = Color.White;
        btnScan.FlatStyle = FlatStyle.Flat;
        btnScan.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btnScan.Click += (s, e) => BtnScan_Click();

        btnClean = new Button();
        btnClean.Text = "🗑️ CLEAN";
        btnClean.Location = new Point(190, 60);
        btnClean.Size = new Size(150, 40);
        btnClean.BackColor = Color.FromArgb(180, 0, 0);
        btnClean.ForeColor = Color.White;
        btnClean.FlatStyle = FlatStyle.Flat;
        btnClean.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btnClean.Click += (s, e) => BtnClean_Click();

        btnReport = new Button();
        btnReport.Text = "💾 SAVE REPORT";
        btnReport.Location = new Point(360, 60);
        btnReport.Size = new Size(150, 40);
        btnReport.BackColor = Color.FromArgb(0, 150, 80);
        btnReport.ForeColor = Color.White;
        btnReport.FlatStyle = FlatStyle.Flat;
        btnReport.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btnReport.Click += (s, e) => BtnReport_Click();

        progressBar = new ProgressBar();
        progressBar.Location = new Point(20, 115);
        progressBar.Size = new Size(640, 20);
        progressBar.Maximum = 100;

        lblStatus = new Label();
        lblStatus.Text = "Ready to scan...";
        lblStatus.Location = new Point(20, 140);
        lblStatus.Size = new Size(640, 20);
        lblStatus.ForeColor = Color.LightGray;

        txtResults = new RichTextBox();
        txtResults.Location = new Point(20, 165);
        txtResults.Size = new Size(640, 320);
        txtResults.BackColor = Color.FromArgb(10, 10, 20);
        txtResults.ForeColor = Color.LimeGreen;
        txtResults.Font = new Font("Consolas", 9f);
        txtResults.ReadOnly = true;

        this.Controls.AddRange(new Control[] {
            lblTitle, btnScan, btnClean, btnReport,
            progressBar, lblStatus, txtResults
        });
    }

    void Log(string msg, Color? color = null)
    {
        txtResults.SelectionColor = color ?? Color.LimeGreen;
        txtResults.AppendText(msg + "\n");
        txtResults.ScrollToCaret();
    }

    void BtnScan_Click()
    {
        txtResults.Clear();
        threatCount = 0;
        progressBar.Value = 0;
        Log("========================================");
        Log("   VIRUS SCAN STARTED", Color.Cyan);
        Log("========================================\n");

        lblStatus.Text = "Scanning processes...";
        progressBar.Value = 20;
        Log("[ SCANNING PROCESSES ]", Color.Yellow);
        foreach (var proc in Process.GetProcesses())
            foreach (var bad in badProcesses)
                if (proc.ProcessName.ToLower().Contains(bad))
                {
                    Log($"  [THREAT] {proc.ProcessName} (PID {proc.Id})", Color.Red);
                    threatCount++;
                }
        Log("  Process scan complete!\n", Color.LimeGreen);

        lblStatus.Text = "Scanning startup programs...";
        progressBar.Value = 50;
        Log("[ SCANNING STARTUP PROGRAMS ]", Color.Yellow);
        using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run"))
        {
            if (key != null)
                foreach (var name in key.GetValueNames())
                {
                    string val = key.GetValue(name)?.ToString() ?? "";
                    bool suspicious = false;
                    foreach (var kw in suspiciousKeywords)
                        if (val.ToLower().Contains(kw)) suspicious = true;
                    if (suspicious)
                    { Log($"  [SUSPICIOUS] {name}", Color.Orange); threatCount++; }
                    else
                        Log($"  [OK] {name}", Color.LimeGreen);
                }
        }

        lblStatus.Text = "Scanning temp folders...";
        progressBar.Value = 80;
        Log("\n[ SCANNING TEMP FILES ]", Color.Yellow);
        try
        {
            foreach (var file in Directory.GetFiles(Path.GetTempPath()))
            {
                string ext = Path.GetExtension(file).ToLower();
                foreach (var danger in dangerousExtensions)
                    if (ext == danger)
                    { Log($"  [SUSPICIOUS] {Path.GetFileName(file)}", Color.Orange); threatCount++; }
            }
        }
        catch { }

        progressBar.Value = 100;
        Log("\n========================================");
        Log($"   SCAN COMPLETE — {threatCount} threat(s) found",
            threatCount > 0 ? Color.Red : Color.LimeGreen);
        Log("========================================");
        lblStatus.Text = $"Done! {threatCount} threat(s) found.";
    }

    void BtnClean_Click()
    {
        Log("\n[ CLEANING TEMP FILES ]", Color.Yellow);
        int deleted = 0;
        try
        {
            foreach (var file in Directory.GetFiles(Path.GetTempPath()))
            {
                string ext = Path.GetExtension(file).ToLower();
                foreach (var danger in dangerousExtensions)
                    if (ext == danger)
                        try
                        {
                            File.Delete(file);
                            Log($"  [DELETED] {Path.GetFileName(file)}", Color.LimeGreen);
                            deleted++;
                        }
                        catch { Log($"  [SKIPPED] {Path.GetFileName(file)}", Color.Gray); }
            }
        }
        catch { }
        Log($"\n  Done! {deleted} file(s) removed.", Color.Cyan);
        lblStatus.Text = $"Cleanup done! {deleted} removed.";
    }

    void BtnReport_Click()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "ScanReport.txt");
        File.WriteAllText(path, txtResults.Text);
        Log("\n  Report saved to Desktop!", Color.Cyan);
        lblStatus.Text = "Report saved!";
    }
}