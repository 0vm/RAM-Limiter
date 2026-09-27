using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace RAMLIMITER
{
    class Program
    {
        // Use EmptyWorkingSet which is the documented way to trim another processes working set
        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool EmptyWorkingSet(IntPtr hProcess);

        // GlobalMemoryStatusEx to get system memory stats (lightweight)
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        static bool IsAdmin()
        {
            var id = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void ElevatePrivileges(string[] args)
        {
            if (IsAdmin())
                return;

            var arguments = args == null || args.Length == 0 ? string.Empty : string.Join(" ", args);

            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Assembly.GetEntryAssembly().Location,
                Arguments = arguments,
                Verb = "runas"
            };

            Console.Clear();
            Console.WriteLine("RAM Limiter does not currently have admin. privileges.\nWould you like to run as admin? (y/n)");
            if (Console.ReadKey(true).Key == ConsoleKey.Y)
            {
                try
                {
                    Process.Start(psi);
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not elevate program.\n" + ex.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        // Choose the process instance with the largest working set (safely)
        static int GetLargestProcessIdByName(string name)
        {
            try
            {
                var procs = Process.GetProcessesByName(name);
                int bestId = -1;
                long bestWs = -1;
                foreach (var p in procs)
                {
                    try
                    {
                        if (p.WorkingSet64 > bestWs)
                        {
                            bestWs = p.WorkingSet64;
                            bestId = p.Id;
                        }
                    }
                    catch
                    {
                        // accessing WorkingSet64 can throw for protected processes; ignore
                    }
                    finally
                    {
                        p.Dispose();
                    }
                }
                return bestId;
            }
            catch
            {
                return -1;
            }
        }

        // Async monitor that trims working set periodically using EmptyWorkingSet
        static async Task MonitorProcessAsync(string name, int intervalMs, CancellationToken ct)
        {
            name = name?.Trim().ToLower();
            if (string.IsNullOrEmpty(name))
                return;

            double lastReportedLoad = -1;
            var reportThrottle = TimeSpan.FromSeconds(5);
            var lastReport = DateTime.MinValue;

            while (!ct.IsCancellationRequested)
            {
                int pid = GetLargestProcessIdByName(name);
                if (pid == -1)
                {
                    if (DateTime.UtcNow - lastReport > reportThrottle)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{name}: not running");
                        Console.ResetColor();
                        lastReport = DateTime.UtcNow;
                    }

                    try { await Task.Delay(intervalMs, ct); } catch (TaskCanceledException) { break; }
                    continue;
                }

                try
                {
                    using (var proc = Process.GetProcessById(pid))
                    {
                        // Attempt to trim the working set
                        try
                        {
                            if (!EmptyWorkingSet(proc.Handle))
                            {
                                // fail silently but log occasionally
                                if (DateTime.UtcNow - lastReport > reportThrottle)
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.WriteLine($"{name}: EmptyWorkingSet failed (pid {pid})");
                                    Console.ResetColor();
                                    lastReport = DateTime.UtcNow;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (DateTime.UtcNow - lastReport > reportThrottle)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"{name}: error trimming process (pid {pid}): {ex.Message}");
                                Console.ResetColor();
                                lastReport = DateTime.UtcNow;
                            }
                        }
                    }

                    // Get system memory usage using GlobalMemoryStatusEx (cheap)
                    var mem = new MEMORYSTATUSEX();
                    mem.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                    if (GlobalMemoryStatusEx(ref mem))
                    {
                        double percent = mem.dwMemoryLoad; // already percentage used (0-100)

                        // Only print when it changes noticeably or throttle frequency
                        if (Math.Abs(percent - lastReportedLoad) > 1.0 || DateTime.UtcNow - lastReport > reportThrottle)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"{name.ToUpper()}: Total RAM usage: {percent:F2}% (pid {pid})");
                            Console.ResetColor();
                            lastReportedLoad = percent;
                            lastReport = DateTime.UtcNow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error handling {name}: {ex.Message}");
                    Console.ResetColor();
                }

                try { await Task.Delay(intervalMs, ct); } catch (TaskCanceledException) { break; }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"Stopped monitoring {name}");
            Console.ResetColor();
        }

        class Config
        {
            public string[] Processes { get; set; }
            public int IntervalMs { get; set; } = 5000;
            public bool AutoStart { get; set; } = false;
        }

        static void PrintHelp()
        {
            Console.WriteLine("Usage: RAMLimiter.exe [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --processes p1,p2    Comma-separated process names to monitor");
            Console.WriteLine("  --interval <ms>      Interval in milliseconds between trims (default 5000)");
            Console.WriteLine("  --autostart           Start monitors immediately and run headless");
            Console.WriteLine("  --config <path>      Path to JSON config file (defaults to ./config.json)");
            Console.WriteLine("  --help                Show this help");
        }

        static Config LoadConfig(string path)
        {
            if (!File.Exists(path))
                return null;

            try
            {
                var text = File.ReadAllText(path);
                var cfg = new Config();

                // crude JSON parsing: processes array
                var procList = TryGetJsonArray(text, "processes");
                if (procList != null) cfg.Processes = procList.ToArray();

                var interval = TryGetJsonNumber(text, "intervalMs");
                if (interval.HasValue) cfg.IntervalMs = (int)interval.Value;

                var autostart = TryGetJsonBool(text, "autostart");
                if (autostart.HasValue) cfg.AutoStart = autostart.Value;

                return cfg;
            }
            catch
            {
                return null;
            }
        }

        static List<string> TryGetJsonArray(string text, string key)
        {
            try
            {
                var idx = text.IndexOf('"' + key + '"', StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return null;
                var start = text.IndexOf('[', idx);
                if (start == -1) return null;
                var end = text.IndexOf(']', start);
                if (end == -1) return null;
                var inside = text.Substring(start + 1, end - start - 1);
                var items = new List<string>();
                int i = 0;
                while (i < inside.Length)
                {
                    // find next quoted string
                    while (i < inside.Length && inside[i] != '"') i++;
                    if (i >= inside.Length) break;
                    int s = i + 1;
                    int e = inside.IndexOf('"', s);
                    if (e == -1) break;
                    var item = inside.Substring(s, e - s).Trim();
                    if (!string.IsNullOrEmpty(item)) items.Add(item);
                    i = e + 1;
                }
                return items;
            }
            catch { return null; }
        }

        static double? TryGetJsonNumber(string text, string key)
        {
            try
            {
                var idx = text.IndexOf('"' + key + '"', StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return null;
                var colon = text.IndexOf(':', idx);
                if (colon == -1) return null;
                int i = colon + 1;
                while (i < text.Length && (char.IsWhiteSpace(text[i]) || text[i] == '\"')) i++;
                int j = i;
                while (j < text.Length && (char.IsDigit(text[j]) || text[j] == '.' || text[j] == '-')) j++;
                var numS = text.Substring(i, j - i);
                if (double.TryParse(numS, out var v)) return v;
                return null;
            }
            catch { return null; }
        }

        static bool? TryGetJsonBool(string text, string key)
        {
            try
            {
                var idx = text.IndexOf('"' + key + '"', StringComparison.OrdinalIgnoreCase);
                if (idx == -1) return null;
                var colon = text.IndexOf(':', idx);
                if (colon == -1) return null;
                int i = colon + 1;
                while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
                var rest = text.Substring(i).TrimStart();
                if (rest.StartsWith("true", StringComparison.OrdinalIgnoreCase)) return true;
                if (rest.StartsWith("false", StringComparison.OrdinalIgnoreCase)) return false;
                return null;
            }
            catch { return null; }
        }

        static void ParseAndApplyArgs(string[] args, out Config cfg, out bool showHelp, out bool runHeadless)
        {
            cfg = new Config();
            showHelp = false;
            runHeadless = false;

            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                if (string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) || string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase))
                {
                    showHelp = true; return;
                }
                if (string.Equals(a, "--config", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    configPath = args[++i];
                    continue;
                }
                if (string.Equals(a, "--processes", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    cfg.Processes = args[++i].Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
                    continue;
                }
                if (string.Equals(a, "--interval", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], out var ms)) cfg.IntervalMs = Math.Max(100, ms);
                    continue;
                }
                if (string.Equals(a, "--autostart", StringComparison.OrdinalIgnoreCase))
                {
                    cfg.AutoStart = true; runHeadless = true; continue;
                }
            }

            var fileCfg = LoadConfig(configPath);
            if (fileCfg != null)
            {
                // apply file values only when not provided by CLI (CLI already wrote into cfg.Processes/IntervalMs/AutoStart)
                if ((cfg.Processes == null || cfg.Processes.Length == 0) && fileCfg.Processes != null)
                    cfg.Processes = fileCfg.Processes;
                if (cfg.IntervalMs == 0 && fileCfg.IntervalMs != 0)
                    cfg.IntervalMs = fileCfg.IntervalMs;
                if (!cfg.AutoStart && fileCfg.AutoStart)
                    cfg.AutoStart = fileCfg.AutoStart;
            }

            // default to empty processes array
            if (cfg.Processes == null) cfg.Processes = new string[0];
        }

        static int Main(string[] args)
        {
            ParseAndApplyArgs(args, out var cfg, out var showHelp, out var runHeadless);
            if (showHelp)
            {
                PrintHelp();
                return 0;
            }

            ElevatePrivileges(args);

            var monitors = new Dictionary<string, CancellationTokenSource>(StringComparer.OrdinalIgnoreCase);

            // If autostart is requested, start monitors from config and run until keypress
            if (cfg.AutoStart && cfg.Processes.Length > 0)
            {
                foreach (var p in cfg.Processes)
                    StartMonitor(p, cfg.IntervalMs > 0 ? cfg.IntervalMs : 5000, monitors);

                Console.WriteLine("Autostart monitors running. Press Enter to exit.");
                Console.ReadLine();

                foreach (var cts in monitors.Values) try { cts.Cancel(); } catch { }
                return 0;
            }

            bool running = true;
            while (running)
            {
                // existing interactive menu (keeps behavior)
                Console.Clear();
                Console.WriteLine("Limit Discord: 1");
                Console.WriteLine("Limit Chrome: 2");
                Console.WriteLine("Limit OBS: 3");
                Console.WriteLine("Limit Discord & Chrome: 4");
                Console.WriteLine("Limit Custom: 5");
                Console.WriteLine("Stop monitoring a process: 6");
                Console.WriteLine("List active monitors: 7");
                Console.WriteLine("Exit: 0");

                var key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.D1:
                    case ConsoleKey.NumPad1:
                        StartMonitor("discord", 5000, monitors);
                        break;
                    case ConsoleKey.D2:
                    case ConsoleKey.NumPad2:
                        StartMonitor("chrome", 5000, monitors);
                        break;
                    case ConsoleKey.D3:
                    case ConsoleKey.NumPad3:
                        StartMonitor("obs64", 5000, monitors);
                        break;
                    case ConsoleKey.D4:
                    case ConsoleKey.NumPad4:
                        StartMonitor("discord", 5000, monitors);
                        StartMonitor("chrome", 5000, monitors);
                        break;
                    case ConsoleKey.D5:
                    case ConsoleKey.NumPad5:
                        Console.WriteLine("Enter process names separated by commas (e.g., chrome, obs64, discord):");
                        var line = Console.ReadLine() ?? string.Empty;
                        var names = line.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
                        foreach (var n in names)
                            StartMonitor(n, 3000, monitors);
                        break;
                    case ConsoleKey.D6:
                    case ConsoleKey.NumPad6:
                        Console.WriteLine("Enter process name to stop monitoring:");
                        var stopName = Console.ReadLine() ?? string.Empty;
                        StopMonitor(stopName.Trim(), monitors);
                        break;
                    case ConsoleKey.D7:
                    case ConsoleKey.NumPad7:
                        Console.WriteLine("Active monitors:");
                        foreach (var k in monitors.Keys)
                            Console.WriteLine(" - " + k);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                    case ConsoleKey.D0:
                    case ConsoleKey.NumPad0:
                        running = false;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid input. Try again.");
                        Thread.Sleep(1000);
                        Console.ResetColor();
                        break;
                }
            }

            // Shutdown monitors
            foreach (var cts in monitors.Values)
            {
                try { cts.Cancel(); } catch { }
            }

            // allow tasks to finish briefly
            Thread.Sleep(500);

            return 0;
        }

        static void StartMonitor(string name, int intervalMs, Dictionary<string, CancellationTokenSource> monitors)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            name = name.Trim().ToLower();
            if (monitors.ContainsKey(name))
            {
                Console.WriteLine($"Already monitoring {name}");
                Thread.Sleep(800);
                return;
            }

            var cts = new CancellationTokenSource();
            monitors[name] = cts;
            Task.Run(() => MonitorProcessAsync(name, intervalMs, cts.Token));
            Console.WriteLine($"Started monitoring {name}");
            Thread.Sleep(500);
        }

        static void StopMonitor(string name, Dictionary<string, CancellationTokenSource> monitors)
        {
            if (string.IsNullOrWhiteSpace(name))
                return;

            name = name.Trim().ToLower();
            if (!monitors.TryGetValue(name, out var cts))
            {
                Console.WriteLine($"Not monitoring {name}");
                Thread.Sleep(800);
                return;
            }

            try
            {
                cts.Cancel();
            }
            catch { }
            monitors.Remove(name);
            Console.WriteLine($"Stopped monitoring {name}");
            Thread.Sleep(500);
        }
    }
}
