using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace RAMLIMITER
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        static bool IsAdmin() // Method to check the program's current privileges - created by Hypn0tick | github.com/Hypn0tick
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void ElevatePrivileges(string args) // Method to elevate the program's privileges if the user chooses to do so - created by Hypn0tick | github.com/Hypn0tick
        {
            if (!IsAdmin())
            {
                var proc = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = Assembly.GetEntryAssembly().Location,
                    Arguments = args,
                    Verb = "runas"
                };

                Console.Clear();
                Console.WriteLine("RAM Limiter does not currently have admin. privileges.\nWould you like to run as admin? (y/n)");
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                {
                    try
                    {
                        Process.Start(proc);
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not elevate program.\n" + ex);
                    }
                }
            }
        }

        static int GetProcessIdByName(string name)
        {
            int pid = -1;
            long workingSet = 0;
            foreach (var proc in Process.GetProcessesByName(name))
            {
                if (proc.WorkingSet64 > workingSet)
                {
                    workingSet = proc.WorkingSet64;
                    pid = proc.Id;
                }
            }
            return pid;
        }

        static void LimitRamForProcess(string name, int min, int max, int interval)
        {
            while (true)
            {
                int pid = GetProcessIdByName(name);
                if (pid == -1)
                {
                    Thread.Sleep(interval);
                    continue;
                }

                try
                {
                    using (var proc = Process.GetProcessById(pid))
                    {
                        SetProcessWorkingSetSize(proc.Handle, min, max);
                    }

                    using (var searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem"))
                    {
                        var mem = searcher.Get().Cast<ManagementObject>().Select(mo => new
                        {
                            Free = double.Parse(mo["FreePhysicalMemory"].ToString()),
                            Total = double.Parse(mo["TotalVisibleMemorySize"].ToString())
                        }).FirstOrDefault();

                        if (mem != null)
                        {
                            double percent = ((mem.Total - mem.Free) / mem.Total) * 100;
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"{name.ToUpper()}: Total RAM usage: {percent:F2}%");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error limiting RAM for {name}: {ex.Message}");
                }

                Thread.Sleep(interval);
            }
        }

        static void CustomRamLimiter(int min, int max)
        {
            Console.WriteLine("Enter process names separated by commas (e.g., chrome, obs64, discord):");
            var names = Console.ReadLine().Split(',').Select(p => p.Trim().ToLower());

            foreach (var name in names)
            {
                new Thread(() => LimitRamForProcess(name, min, max, 3000)) { IsBackground = true }.Start();
            }
            Thread.Sleep(Timeout.Infinite);
        }

        static void Main(string[] args)
        {
            new Thread(() =>
            {
                var s = "        ";
                var random = new Random();
                while (true)
                {
                    switch (random.Next(1, 3))
                    {
                        case 1:
                            Console.Title = "Have you starred the repo? " + s + "| github.com/0vm";
                            break;
                        case 2:
                            Console.Title = "https://github.com/0vm/RAM-Limiter  " + s + "| github.com/0vm";
                            break;
                    }
                    Thread.Sleep(2000);
                }
            })
            { IsBackground = true }.Start();

            ElevatePrivileges(string.Concat(args));

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Limit Discord: 1");
                Console.WriteLine("Limit Chrome: 2");
                Console.WriteLine("Limit OBS: 3");
                Console.WriteLine("Limit Discord & Chrome: 4");
                Console.WriteLine("Limit Custom: 5");
                Console.WriteLine("Exit: 0");

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
                    new Thread(() => LimitRamForProcess("discord", -1, -1, 5000)) { IsBackground = true }.Start();
                else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
                    new Thread(() => LimitRamForProcess("chrome", -1, -1, 5000)) { IsBackground = true }.Start();
                else if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3)
                    new Thread(() => LimitRamForProcess("obs64", -1, -1, 5000)) { IsBackground = true }.Start();
                else if (key == ConsoleKey.D4 || key == ConsoleKey.NumPad4)
                {
                    new Thread(() => LimitRamForProcess("discord", -1, -1, 5000)) { IsBackground = true }.Start();
                    new Thread(() => LimitRamForProcess("chrome", -1, -1, 5000)) { IsBackground = true }.Start();
                }
                else if (key == ConsoleKey.D5 || key == ConsoleKey.NumPad5)
                    CustomRamLimiter(-1, -1);
                else if (key == ConsoleKey.D0 || key == ConsoleKey.NumPad0)
                    Environment.Exit(0);
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid input. Try again.");
                    Thread.Sleep(2000);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }
    }
}
