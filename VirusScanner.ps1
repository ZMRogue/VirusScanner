# My Virus Scanner

Write-Host "Scanning running process..." -ForegroundColor Cyan


# List of known bad process names
$BadProcesses = @(
    "xmrig", "minerd", "remcos", "njrat",
    "darkcomet", "quasar", "asyncrat", "nanocore"
)

# Get all running processes on the PC
$RunningProcesses = Get-Process

# Check each process
foreach ($Process in $RunningProcesses) {
    if ($BadProcesses -contains $Process.Name.ToLower()) {
        Write-Host "[THREAT FOUND] $($Process.Name) - PID: $($Process.Id)" -ForegroundColor Red
    }
}

Write-Host "Process scan complete!" -ForegroundColor Green

# Step 3

Write-Host "`nScanning startup programs..." -ForegroundColor Cyan

$StartupKeys = @(
    "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run",
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Run"
)

$SuspiciousKeywords = @("temp", "appdata", ".vbs", ".bat", ".cmd", ".ps1")

foreach ($Key in $StartupKeys) {
    $Entries = Get-ItemProperty -Path $Key -ErrorAction SilentlyContinue
    if ($Entries) {
        $Entries.PSObject.Properties | Where-Object { $_.Name -notlike "PS*" } | ForEach-Object {
            $Name  = $_.Name
            $Value = $_.Value

            $isSuspicious = $SuspiciousKeywords | Where-Object { $Value -like "*$_*" }

            if ($isSuspicious) {
                Write-Host "[SUSPICIOUS STARTUP] $Name => $Value" -ForegroundColor Red
            } else {
                Write-Host "[OK] $Name" -ForegroundColor Green
            }
        }
    }
}

Write-Host "Startup scan complete!" -ForegroundColor Cyan

# Step 4: Scan Temp Folders for Suspicious Files
Write-Host "`nScanning temp folders for suspicious files..." -ForegroundColor Cyan

$ScanFolders = @(
    $env:TEMP,
    "C:\Windows\Temp"
)

$DangerousExtensions = @(".exe", ".bat", ".vbs", ".cmd", ".ps1", ".scr", ".hta")

foreach ($Folder in $ScanFolders) {
    if (Test-Path $Folder) {
        Get-ChildItem -Path $Folder -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $DangerousExtensions -contains $_.Extension.ToLower() } |
        ForEach-Object {
            Write-Host "[SUSPICIOUS FILE] $($_.FullName)" -ForegroundColor Red
        }
    }
}

Write-Host "File scan complete!" -ForegroundColor Cyan

# Step 5: Cleanup

Write-Host "`nStarting cleanup of temp files..." -ForegroundColor Cyan

$CleanFolders = @($env:TEMP, "C:\Windows\Temp")
$Deleted = 0

foreach ($Folder in $CleanFolders) {
    if (Test-Path $Folder) {
        Get-ChildItem -Path $Folder -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $DangerousExtensions -contains $_.Extension.ToLower() } |
        ForEach-Object {
            try {
                Remove-Item $_.FullName -Force -ErrorAction Stop
                Write-Host "[DELETED] $($_.Name)" -ForegroundColor Green
                $Deleted++
            } catch {
                Write-Host "[SKIPPED] $($_.Name) - in use or protected" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host "`nCleanup complete! $Deleted file(s) removed." -ForegroundColor Cyan

#Step6: Save Report
Write-Host "`nSaving report..." -ForegroundColor Cyan

$ReportPath = "C:\Users\magpa\Desktop\ScanReport.txt"
$Date = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$Report = @"
================================
  VIRUS SCAN REPORT
  Date: $Date
================================

[SCAN RESULTS]
- Process scan: Complete
- Startup scan: Complete
- File scan: Complete
- Cleanup: $Deleted file(s) removed

[NOTE]
If any SUSPICIOUS items were found,
review them manually or run a full
Windows Defender scan to confirm.

================================
"@

$Report | Out-File -FilePath $ReportPath -Encoding UTF8
Write-Host "Report saved to Desktop: ScanReport.txt" -ForegroundColor Green
Write-Host "`nScan finished! Your PC has been checked." -ForegroundColor Cyan

