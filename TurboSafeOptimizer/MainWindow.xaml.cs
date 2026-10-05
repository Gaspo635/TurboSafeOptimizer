using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;
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
            CpuText.Text = GetFirstValue("Win32_Processor", "Name") ?? "Unknown CPU";
            GpuText.Text = GetFirstGpu() ?? "Unknown GPU";

            var memory = GetMemory();
            RamText.Text = memory.totalGb > 0
                ? $"{memory.usedGb:0.0} / {memory.totalGb:0.0} GB"
                : "Unavailable";

            WindowsText.Text = GetFirstValue("Win32_OperatingSystem", "Caption")?.Replace("Microsoft ", "")
                ?? "Windows";

            StatusText.Text = "System ready";
        }
        catch
        {
            StatusText.Text = "Limited system data";
        }

        await Task.CompletedTask;
    }

    private static string? GetFirstValue(string className, string property)
    {
        using var searcher = new ManagementObjectSearcher(
            $"SELECT {property} FROM {className}");

        foreach (ManagementObject item in searcher.Get())
        {
            return item[property]?.ToString();
        }

        return null;
    }

    private static string? GetFirstGpu()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT Name FROM Win32_VideoController");

        foreach (ManagementObject item in searcher.Get())
        {
            var name = item["Name"]?.ToString();

            if (!string.IsNullOrWhiteSpace(name) &&
                !name.Contains("Microsoft Basic", StringComparison.OrdinalIgnoreCase))
            {
                return name;
            }
        }

        return null;
    }

    private static (double totalGb, double usedGb) GetMemory()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem");

        foreach (ManagementObject item in searcher.Get())
        {
            var total = Convert.ToDouble(item["TotalVisibleMemorySize"]) / 1024 / 1024;
            var free = Convert.ToDouble(item["FreePhysicalMemory"]) / 1024 / 1024;
            return (total, total - free);
        }

        return (0, 0);
    }

    private void Optimize_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "iClover will perform safe maintenance:\n\n" +
            "• Create a restore point\n" +
            "• Clean temporary files\n" +
            "• Flush DNS cache\n\n" +
            "Personal documents and photos are not targeted.\n\nContinue?",
            "Optimize PC",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            StatusText.Text = "Creating restore point...";
            CreateRestorePoint();

            StatusText.Text = "Cleaning temporary files...";
            CleanTemp();

            StatusText.Text = "Flushing DNS...";
            RunProcess("ipconfig.exe", "/flushdns");

            StatusText.Text = "Optimization complete";

            MessageBox.Show(
                "Optimization completed.\n\n" +
                "Windows temporary files were processed and DNS cache was flushed.",
                "iClover Tweaks",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = "Operation stopped";

            MessageBox.Show(
                "The operation could not be completed.\n\n" + ex.Message,
                "iClover Tweaks",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void Restore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CreateRestorePoint();

            MessageBox.Show(
                "Windows restore point request completed.",
                "System Restore",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Windows could not create the restore point.\n\n" + ex.Message,
                "System Restore",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private static void CreateRestorePoint()
    {
        RunPowerShell(
            "Checkpoint-Computer -Description 'iClover Tweaks Restore Point' " +
            "-RestorePointType 'MODIFY_SETTINGS'");
    }

    private static void CleanTemp()
    {
        var folders = new[]
        {
            Path.GetTempPath(),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp")
        };

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder))
                continue;

            foreach (var file in Directory.GetFiles(folder))
            {
                try { File.Delete(file); }
                catch { }
            }

            foreach (var directory in Directory.GetDirectories(folder))
            {
                try { Directory.Delete(directory, true); }
                catch { }
            }
        }
    }

    private void Menu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
            return;

        OpenSection(button.Tag?.ToString() ?? "Dashboard");
    }

    private void Card_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
            return;

        OpenSection(button.Tag?.ToString() ?? "Dashboard");
    }

    private void OpenSection(string section)
    {
        switch (section)
        {
            case "Dashboard":
                PageTitle.Text = "Optimization Center";
                PageSubtitle.Text = "A clean control center for Windows maintenance, gaming and diagnostics.";
                break;

            case "Windows":
                PageTitle.Text = "Windows Tweaks";
                PageSubtitle.Text = "Open supported Windows configuration panels.";
                OpenSettings("ms-settings:privacy");
                break;

            case "Cleaner":
                PageTitle.Text = "System Cleaner";
                PageSubtitle.Text = "Clean temporary files without targeting personal documents.";
                CleanTemp();
                StatusText.Text = "Temporary files processed";
                break;

            case "Network":
                PageTitle.Text = "Networking";
                PageSubtitle.Text = "Refresh common Windows network caches.";
                RunProcess("ipconfig.exe", "/flushdns");
                StatusText.Text = "DNS cache flushed";
                break;

            case "Advanced":
                PageTitle.Text = "Advanced";
                PageSubtitle.Text = "Windows advanced system configuration.";
                StartProgram("sysdm.cpl");
                break;

            case "Services":
                PageTitle.Text = "Services";
                PageSubtitle.Text = "Windows service management.";
                StartProgram("services.msc");
                break;

            case "Gaming":
                PageTitle.Text = "Game Mode";
                PageSubtitle.Text = "Windows gaming configuration.";
                OpenSettings("ms-settings:gaming-gamemode");
                break;

            case "Tasks":
                PageTitle.Text = "Task Manager";
                PageSubtitle.Text = "Inspect running applications and processes.";
                StartProgram("taskmgr.exe");
                break;

            case "Info":
                PageTitle.Text = "System Information";
                PageSubtitle.Text = "Detailed Windows and hardware information.";
                StartProgram("msinfo32.exe");
                break;

            case "Restore":
                PageTitle.Text = "Restore Point";
                PageSubtitle.Text = "Create a Windows restore point before major changes.";
                Restore_Click(this, new RoutedEventArgs());
                break;
        }
    }

    private static void OpenSettings(string uri)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = uri,
            UseShellExecute = true
        });
    }

    private static void StartProgram(string fileName)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = true
        });
    }

    private static void RunProcess(string fileName, string arguments)
    {
        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        process?.WaitForExit();
    }

    private static void RunPowerShell(string command)
    {
        var encoded = Convert.ToBase64String(
            System.Text.Encoding.Unicode.GetBytes(command));

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -EncodedCommand {encoded}",
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });

        process?.WaitForExit();

        if (process is not null && process.ExitCode != 0)
            throw new InvalidOperationException(
                $"PowerShell returned exit code {process.ExitCode}.");
    }
}