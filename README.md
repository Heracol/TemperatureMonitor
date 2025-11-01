# Temperature Monitor

A lightweight and easy-to-use Windows temperature monitoring tool built with C#.
It displays your CPU and GPU temperatures directly in the taskbar, with simple customization options for colors, update interval, and startup behavior.

![Example](https://github.com/user-attachments/assets/668e842d-46d9-496a-b02a-aa792db26ad8)

## Features

- 🧠 CPU Temperature Display: Always visible in the taskbar
- 🎮 Optional GPU Temperature: Monitor your graphics card as well
- 🎨 Customizable Appearance: Choose your font and background colors
- 🔄 Adjustable Update Interval: Control how often the temperature refreshes
- ⚡ Start with Windows: Automatically launches on startup
- 🎈 Lightweight: Minimal memory and CPU usage

## How to Use

1. Download the latest release from the [Releases](https://github.com/Heracol/TemperatureMonitor/releases) page.
2. Run TemperatureMonitor.exe.
3. The current temperature will appear in your taskbar.

You can right-click the icon to:

- Change colors
- Enable/disable GPU monitoring
- Adjust update frequency
- Enable Start with Windows

## System Requirements

- Windows 10 or later (tested on 10; please share if it works on 11)
- .NET Runtime (if not already installed)
- Supports most modern CPUs and GPUs with temperature sensors

## Configuration (Optional)

Settings are saved automatically, but you can also manually edit them in:

`%AppData%\TemperatureMonitor\settings.json`

Notes:

- Enabling Start with Windows adds an entry in Task Scheduler.
- To manually remove, open Task Scheduler => find Temperature Monitor => delete the entry.

## Uninstall

Uninstalling is simple:

- Disable auto-start in the app if enabled.
- Delete TemperatureMonitor.exe.
- If auto-start wasn’t disabled, remove the Task Scheduler entry manually (see Configuration above).

## Project Overview

This project is written in C# (.NET 8) using Windows Forms and LibreHardwareMonitor for reading sensor data.

## Bugs, Feedback, and Feature Requests

If you encounter any bugs or issues, please open an issue in the [Issues](https://github.com/Heracol/TemperatureMonitor/issues) section.

You’re also welcome to:
- Suggest new features
- Share feedback or improvement ideas
- Report performance or compatibility issues

Every bit of feedback helps make the app better, thank you! 🙌

## Acknowledgements

- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) for hardware sensor access
