using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace TurboSafeOptimizer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadSystemInfo();
    }

    private async Task LoadSystemInfo()
    {
        try
        {
            CpuValue.Text = FirstValue("Win32_Processor", "Name") ?? "Unavailable";
            GpuValue.Text = FirstValue("Win32_VideoController", "Name") ?? "Unavailable";
            var total = Convert.ToUInt64(FirstValue("Win32_ComputerSystem", "TotalPhysicalMemory") ?? "0");
            var free = Convert.ToUInt64(FirstValue("Win32_OperatingSystem", "FreePhysicalMemory") ?? "0") * 1024UL;
            var used = total > free ? total - free : 0;
            MemoryValue.Text = total == 0 ? "Unavailable" : $"{used / 1073741824d:0.0} / {total / 1073741824d:0.0} GB";
            WindowsValue.Text = FirstValue("Win32_OperatingSystem", "Caption")?.Replace("Microsoft ", "") ?? "Windows";
            StatusText.Text = "READY";
        }
        catch
        {
            CpuValue.Text = "Unavailable"; GpuValue.Text = "Unavailable"; MemoryValue.Text = "Unavailable"; WindowsValue.Text = "Windows"; StatusText.Text = "LIMITED";
        }
        await Task.CompletedTask;
    }

    private static string? FirstValue(string cls, string property)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {cls}");
            foreach (ManagementObject obj in searcher.Get()) return obj[property]?.ToString();
        }
        catch { }
        return null;
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button) return;
        switch (button.Tag?.ToString())
        {
            case "Dashboard": StatusText.Text = "READY"; break;
            case "General": Open("ms-settings:privacy"); break;
            case "Cleaner": CleanTemporaryFiles(true); break;
            case "Networking": NetworkTools(); break;
            case "Gaming": Open("ms-settings:gaming-gamemode"); break;
            case "System": Start("msinfo32.exe"); break;
            case "Restore": CreateRestorePoint(); break;
        }
    }

    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "SCANNING...";
        try
        {
            await LoadSystemInfo();
            var report = new Dictionary<string, object?>
            {
                ["timestampUtc"] = DateTime.UtcNow,
                ["computer"] = Environment.MachineName,
                ["windows"] = WindowsValue.Text,
                ["cpu"] = CpuValue.Text,
                ["gpu"] = GpuValue.Text,
                ["memory"] = MemoryValue.Text,
                ["architecture"] = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                ["automaticTweaksEnabled"] = false,
                ["rule"] = "NO MEJORÓ = NO SE QUEDA"
            };
            SaveReport(report);
            StatusText.Text = "SCAN COMPLETE";
            MessageBox.Show("System scan completed. A JSON report was saved to your local TurboSafeOptimizer logs folder.", "TurboSafe Optimizer", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "SCAN FAILED";
            MessageBox.Show($"The scan could not be completed.\n\n{ex.Message}", "TurboSafe Optimizer", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Restore_Click(object sender, RoutedEventArgs e) => CreateRestorePoint();

    private void CreateRestorePoint()
    {
        var result = MessageBox.Show("Create a Windows restore point before making major system changes?", "System Restore", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            RunPowerShell("Checkpoint-Computer -Description 'TurboSafe Optimizer Restore Point' -RestorePointType 'MODIFY_SETTINGS'", true);
            MessageBox.Show("Restore point request completed.", "System Restore", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Windows could not create the restore point. System Protection may be disabled or administrator approval may be required.\n\n{ex.Message}", "System Restore", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Clean_Click(object sender, RoutedEventArgs e) => CleanTemporaryFiles(true);

    private void CleanTemporaryFiles(bool showMessage)
    {
        try
        {
            long deleted = 0;
            var folders = new[] { Path.GetTempPath(), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp") };
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (var file in Directory.GetFiles(folder))
                {
                    try { deleted += new FileInfo(file).Length; File.Delete(file); } catch { }
                }
                foreach (var dir in Directory.GetDirectories(folder))
                {
                    try { Directory.Delete(dir, true); } catch { }
                }
            }
            StatusText.Text = "CLEANUP COMPLETE";
            if (showMessage) MessageBox.Show($"Temporary-file cleanup finished.\n\nApproximately {deleted / 1048576d:0.0} MB processed.\nFiles in use were skipped.", "Cleaner", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Cleanup failed.\n\n{ex.Message}", "Cleaner", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void Network_Click(object sender, RoutedEventArgs e) => NetworkTools();

    private void NetworkTools()
    {
        var result = MessageBox.Show("Run Windows network maintenance?\n\n• Flush DNS cache\n• Reset Winsock\n\nA restart may be required.", "Networking", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            RunCommand("ipconfig.exe", "/flushdns", true);
            RunCommand("netsh.exe", "winsock reset", true);
            StatusText.Text = "NETWORK COMPLETE";
            MessageBox.Show("Network maintenance completed. Restart Windows if requested.", "Networking", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex) { MessageBox.Show($"Network maintenance failed.\n\n{ex.Message}", "Networking", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void Gaming_Click(object sender, RoutedEventArgs e) => Open("ms-settings:gaming-gamemode");
    private void System_Click(object sender, RoutedEventArgs e) => Start("msinfo32.exe");

    private static void Open(string uri)
    {
        try { Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"Windows Settings could not be opened.\n\n{ex.Message}", "TurboSafe Optimizer", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private static void Start(string fileName)
    {
        try { Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show($"Windows could not start the tool.\n\n{ex.Message}", "TurboSafe Optimizer", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private static void RunCommand(string fileName, string arguments, bool administrator)
    {
        var info = new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = true, CreateNoWindow = true };
        if (administrator) info.Verb = "runas";
        using var process = Process.Start(info);
        process?.WaitForExit();
        if (process is not null && process.ExitCode != 0) throw new InvalidOperationException($"Windows returned exit code {process.ExitCode}.");
    }

    private static void RunPowerShell(string command, bool administrator)
    {
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
        var info = new ProcessStartInfo { FileName = "powershell.exe", Arguments = $"-NoProfile -EncodedCommand {encoded}", UseShellExecute = true, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        if (administrator) info.Verb = "runas";
        using var process = Process.Start(info);
        process?.WaitForExit();
        if (process is not null && process.ExitCode != 0) throw new InvalidOperationException($"PowerShell returned exit code {process.ExitCode}.");
    }

    private static void SaveReport(Dictionary<string, object?> report)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TurboSafeOptimizer", "Logs");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"scan-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
    }
}