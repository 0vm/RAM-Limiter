# RAM Limiter

RAM Limiter is a utility designed to optimise the RAM usage of any application through the process of Garbage Collection (GC) and working-set trimming on Windows.

## Overview

Old Video, Now you can limit any application and as many applications.

![RAM Limiter Demonstration](https://user-images.githubusercontent.com/79897291/173233207-912f3cb1-bc42-45fa-9f81-36da025f58a4.gif)
https://user-images.githubusercontent.com/79897291/172990167-0e113c2d-5edd-4ffa-9e06-8ac7cb1946ea.mp4
(The wallpaper was a meme back then, I do not condone the actions of foreign governments)

RAM Limiter was developed to address the challenge of applications, like Discord, that tend to cache objects unnecessarily, leading to high RAM usage. It originally used GC.Collect to reduce managed memory pressure in targeted apps. Over time the approach has been improved to use OS working-set trimming (EmptyWorkingSet) and a monitoring loop that trims only when needed, reducing CPU overhead while still freeing RAM.

This tool proves particularly useful for systems with limited RAM, where applications like Discord could use up to 1.3GB. By releasing these resources, it allows RAM-intensive games and other applications to run more smoothly.

The RAM Limiter is a standalone solution that eliminates the need for other software like Razer Cortex™. Moreover, it provides an updated and maintained alternative to older versions of this tool.

## What's new in this version

- Trim process working sets using the documented EmptyWorkingSet API instead of repeatedly calling SetProcessWorkingSetSize.
- Replaced raw threads with Task-based monitors and CancellationToken support so monitors can be started and stopped cleanly.
- Added a lightweight command-line interface and JSON config file support for automation and headless usage.
- Reduced noisy console output and throttled status messages to lower CPU overhead.

## Usage

You can run RAM Limiter interactively (menu) or automate it with a CLI or config file.

Interactive mode:
- Run the executable with no arguments and choose an option from the menu (Discord, Chrome, OBS, custom, etc.).

Command-line options:
- --processes p1,p2    Comma-separated process names to monitor (e.g., discord,chrome)
- --interval <ms>      Interval in milliseconds between trims (default 5000)
- --autostart           Start monitors immediately from config and run headless
- --config <path>      Path to JSON config file (defaults to `./config.json`)
- --help, -h           Show help

Examples:
- Start interactive UI: run `RAMLimiter.exe` with no args.
- Start monitors from CLI: `RAMLimiter.exe --processes discord,chrome --interval 6000`
- Use a config file and autostart: create `config.json` next to the exe and run `RAMLimiter.exe --autostart` or `RAMLimiter.exe --config path\to\config.json --autostart`.

Config file example (config.json):

```json
{
  "processes": ["discord", "chrome"],
  "intervalMs": 5000,
  "autostart": true
}
```

Notes:
- Process names are the executable name without extension (e.g., `discord`, `chrome`, `obs64`).
- CLI arguments override values in the config file.
- The program will request elevation on start if needed; running as Administrator gives the best chance to trim other processes.

## Inspiration

[This Tool](https://github.com/farajyeet/discord-ram-limiter) is no longer maintained. It was found to consume more CPU resources than Discord itself, resulting in a trade-off between free CPU and free RAM. This not only led to increased power usage but also negated the purpose of freeing up RAM.

Our version of the RAM Limiter improves upon the original by focusing on efficient memory management without overutilising the CPU. Some parts of the code were reused from the original repository and [our other project](https://github.com/0vm/Pinger).

## Tags

Limit RAM usage in Discord,
Limit RAM usage in Google Chrome,
Reduce RAM consumption in Google Chrome,
High RAM usage in Google Chrome,
Discord RAM management,
Reduce Discord's memory usage,
Discord RAM optimization,
Discord RAM optimisation,
Memory leak in Discord,
High RAM usage in Discord,
OBS RAM management,
Reduce OBS memory usage,
OBS RAM optimization,
OBS RAM optimisation,
OBS memory leak troubleshooting,
High RAM usage in OBS,
Limit OBS RAM usage
