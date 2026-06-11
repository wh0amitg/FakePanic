# FakePanic
![.NET Framework](https://img.shields.io/badge/.NET-4.7.2-5C2D91.svg?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120.svg?style=flat-square&logo=c-sharp&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)

A lightweight Windows prank/joke application that simulates a critical system failure. 

The project is pre-configured to compile into a **single, standalone executable file** (`.exe`). All required dependencies are embedded directly into the binary, and clutter files (like documentation XMLs) are automatically stripped out during the build process to ensure a completely clean output directory.

## 🚀 Features
* **Monolithic Build (Portable):** External libraries (including NAudio and system dependencies) are packed inside `FakePanic.exe` using Costura.Fody.
* **Zero Clutter:** Custom MSBuild targets guarantee that no `.xml`, `.pdb`, or `.config` files are left behind in your final Release folder.

## ⚙️ Requirements

**For Users:**
* OS: Windows 7 / 8 / 10 / 11
* Runtime: [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472) or higher

**For Developers:**
* Visual Studio 2019 / 2022
* NuGet Package Restore enabled (for NAudio and Costura.Fody)

## 🛠️ Compilation & Build

1. Clone the repository:
   ```bash
   git clone https://github.com/wh0amitg/FakePanic.git

## ⚠️ Disclaimer & Safety

This application is developed **strictly for educational and entertainment purposes (pranks/jokes)**. 
* **NOT Malware:** It does not contain any malicious payloads, virus behavior, or spyware.
* **Completely Harmless:** It does not modify system files, inject into other processes, encrypt data, or alter registry keys. It is purely a visual and audio simulation.
* **Easy to Close:** You can close the program by pressing the CTRL + F12 key combination.
