using System.Management;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace TurboSafeOptimizer;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private async void Scan_Click(object sender, RoutedEventArgs e)
    {
        ScanButton.IsEnabled = false;
        StatusText.Text = "Analizando...";
        try
        {
            var report = await Task.Run(CreateReport);
            Output.Text = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            StatusText.Text = "Diagnóstico terminado";
        }
        catch (Exception ex)
        {
            Output.Text = $"ERROR: {ex.Message}";
            StatusText.Text = "No se pudo completar el diagnóstico";
        }
        finally { ScanButton.IsEnabled = true; }
    }

    private static Dictionary<string, object?> CreateReport()
    {
        var cpu = FirstValue("Win32_Processor", "Name");
        var gpu = FirstValue("Win32_VideoController", "Name");
        var ram = FirstValue("Win32_ComputerSystem", "TotalPhysicalMemory");
        var os = FirstValue("Win32_OperatingSystem", "Caption");
        var build = FirstValue("Win32_OperatingSystem", "BuildNumber");
        var report = new Dictionary<string, object?>
        {
            ["timestampUtc"] = DateTime.UtcNow,
            ["computer"] = Environment.MachineName,
            ["windows"] = os,
            ["build"] = build,
            ["cpu"] = cpu,
            ["gpu"] = gpu,
            ["ramBytes"] = ram,
            ["architecture"] = Environment.Is64BitOperatingSystem ? "x64" : "x86",
            ["rule"] = "NO MEJORÓ = NO SE QUEDA",
            ["automaticTweaksEnabled"] = false
        };
        SaveReport(report);
        return report;
    }

    private static string? FirstValue(string cls, string property)
    {
        using var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {cls}");
        foreach (ManagementObject obj in searcher.Get()) return obj[property]?.ToString();
        return null;
    }

    private static void SaveReport(Dictionary<string, object?> report)
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TurboSafeOptimizer", "Logs");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"scan-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
    }
}
