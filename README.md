# RAM Limiter

RAM Limiter is a utility designed to optimise the RAM usage of any application through the process of Garbage Collection (GC).

## Overview
Old Video, Now you can limit any application and as many applications.

![RAM Limiter Demonstration](https://user-images.githubusercontent.com/79897291/173233207-912f3cb1-bc42-45fa-9f81-36da025f58a4.gif)
https://user-images.githubusercontent.com/79897291/172990167-0e113c2d-5edd-4ffa-9e06-8ac7cb1946ea.mp4
(The wallpaper was a meme back then, I do not condone the actions of foreign governments)

RAM Limiter was developed to address the challenge of applications, like Discord, that tend to cache objects unnecessarily, leading to high RAM usage. It leverages the `GC.Collect` method for efficient garbage collection, thereby freeing up the memory that these applications consume.

This tool proves particularly useful for systems with limited RAM, where applications like Discord could use up to 1.3GB. By releasing these resources, it allows RAM-intensive games and other applications to run more smoothly.

The RAM Limiter is a standalone solution that eliminates the need for other software like Razer Cortex™. Moreover, it provides an updated and maintained alternative to the original version of this tool, which is no longer supported and has been outdated for over a year.

## Usage

RAM Limiter works by automatically freeing up the RAM that is being used by any application. This not only provides your device with more available memory but also results in less battery consumption.

Upon launching the application, you can choose the application you want to optimise - Chrome, Discord, or OBS. These are the applications that are known to consume significant RAM resources, but the tool can be used with any application.

## Inspiration
[This Tool](https://github.com/farajyeet/discord-ram-limiter) is no longer maintained. It was found to consume more CPU resources than Discord itself, resulting in a trade-off between free CPU and free RAM. This not only led to increased power usage but also negated the purpose of freeing up RAM.


Our version of the RAM Limiter improves upon the original by focusing on efficient memory management without overutilising the CPU. Some parts of the code were reused from the original repository and [our other project](https://github.com/0vm/Pinger).

Thank you to @luke-beep and @hypn0tick for contributing / feedback!

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
