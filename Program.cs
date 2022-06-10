using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RAMLIMITER
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static int GetDiscord()
        {
            int DiscordId = -1;
            long workingSet = 0;
            foreach (Process discord in Process.GetProcessesByName("Discord"))
            {
                if (discord.WorkingSet64 > workingSet)
                {
                    workingSet = discord.WorkingSet64;
                    DiscordId = discord.Id;
                }
            }
            return DiscordId;
        }

        public static int GetChrome()
        {
            int chromeId = -1;
            long workingSet1 = 0;
            foreach (Process chrome in Process.GetProcessesByName("Chrome"))
            {
                if (chrome.WorkingSet64 > workingSet1)
                {
                    workingSet1 = chrome.WorkingSet64;
                    chromeId = chrome.Id;
                }
            }
            return chromeId;
        }

        static void Both(int min, int max)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    while (GetChrome() != -1)
                    {
                        if (GetChrome() != -1)
                        {
                            GC.Collect();

                            GC.WaitForPendingFinalizers();

                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                SetProcessWorkingSetSize(Process.GetProcessById(GetChrome()).Handle, min, max);
                            }

                            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

                            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                            }).FirstOrDefault();

                            if (memoryValues != null)
                            {


                                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("CHROME: Total ram usage: {0}", percent);
                                Thread.Sleep(3000);
                            }

                            Thread.Sleep(1);
                        }
                    }
                }
            }).Start();
            Thread.Sleep(5000);


            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    while (GetDiscord() != -1)
                    {
                        if (GetDiscord() != -1)
                        {
                            GC.Collect();

                            GC.WaitForPendingFinalizers();

                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                SetProcessWorkingSetSize(Process.GetProcessById(GetDiscord()).Handle, min, max);
                            }

                            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

                            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new
                            {
                                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                            }).FirstOrDefault();

                            if (memoryValues != null)
                            {


                                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine("DISCORD: Total ram usage: {0}", percent);
                                Thread.Sleep(3000);
                            }

                            Thread.Sleep(1);
                        }
                    }
                }

            }).Start();
            Thread.Sleep(-1);





        }

        static void DiscordRamLimiter(int min, int max)
        {
            while (GetDiscord() != -1)
            {
                if (GetDiscord() != -1)
                {
                    GC.Collect();

                    GC.WaitForPendingFinalizers();

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        SetProcessWorkingSetSize(Process.GetProcessById(GetDiscord()).Handle, min, max);
                    }

                    var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

                    var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                        FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                        TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                    }).FirstOrDefault();

                    if (memoryValues != null)
                    {


                        var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("DISCORD: Total ram usage: {0}", percent);
                        Thread.Sleep(10000);
                    }

                    Thread.Sleep(1);
                }
            }
        }


        static void ChromeRamLimiter(int min, int max)
        {
                    while (GetChrome() != -1)
                    {
                        if (GetChrome() != -1)
                        {
                            GC.Collect();

                            GC.WaitForPendingFinalizers();

                            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            {
                                SetProcessWorkingSetSize(Process.GetProcessById(GetChrome()).Handle, min, max);
                            }

                            var wmiObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

                            var memoryValues = wmiObject.Get().Cast<ManagementObject>().Select(mo => new {
                                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                            }).FirstOrDefault();

                            if (memoryValues != null)
                            {


                                var percent = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
                                Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("CHROME: Total ram usage: {0}", percent);
                        Thread.Sleep(5000);
                            }

                            Thread.Sleep(1);
                        }
                    }
        }

        static void Main(string[] args)
        {
            new Thread(() => // taken from pinger, originally from zf9
            {

                var s = "        "; // 8 spaces for the titles spacing, looks very messy but who cares.

                Thread.CurrentThread.IsBackground = true;
                for (int i = 0; i < int.MaxValue; i++)
                {
                    var random = new Random();
                    var randomNumber = random.Next(1, 5);

                    if (randomNumber == 3)
                    {
                        Console.Title = "Have you starred the repo? " + s + s + "" + s + s + s + s + s + s + s + s + s + $"| github.com/0vm";
                        Thread.Sleep(1500);
                        Console.Title = "Have you starred the repo? " + s + s + "" + s + s + s + s + s + s + s + s + s + $"| github.com/0vm";
                        Thread.Sleep(1500);
                    }
                    else if (randomNumber == 2)
                    {
                        Console.Title = "Chrome & Discord RAM Limiter" + s + "" + s + s + s + s + s + s + s + s + s + $"| github.com/0vm";
                        Thread.Sleep(1500);
                        Console.Title = "Chrome & Discord RAM Limiter" + s + "" + s + s + s + s + s + s + s + s + s + $"| github.com/0vm";
                        Thread.Sleep(1500);
                    }
                    else if (randomNumber == 1)
                    {
                        Console.Title = "https://github.com/0vm/Chrome-and-Discord-RAM-Limiter  " + s + " " + s + s + $"  | github.com/0vm";
                        Thread.Sleep(1500);
                        Console.Title = "https://github.com/0vm/Chrome-and-Discord-RAM-Limiter  " + s + " " + s + s + $"  | github.com/0vm";
                        Thread.Sleep(1500);
                    }
                }
                Thread.Sleep(-1);
            }).Start();
            start:
            Console.WriteLine("Just Limit Discord: 1");
            Console.WriteLine("Just Limit Chrome: 2");
            Console.WriteLine("Limit Both: 3");
            ConsoleKey response = Console.ReadKey(true).Key;
            Console.WriteLine();
            if (response == ConsoleKey.D1)
            {
                Console.Clear();
                DiscordRamLimiter(-1, -1);
            }
            else if (response == ConsoleKey.D2)
            {
                Console.Clear();
                ChromeRamLimiter(-1, -1);
            }
            else if (response == ConsoleKey.D3)
            {
                Console.Clear();
                Both(-1, -1);
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Don't Use The Numpad, Use The Numbers At The Top Of Your Keyboard.");
                Thread.Sleep(2500);
                goto start;
            }

            



            


            
        }
    }
}
