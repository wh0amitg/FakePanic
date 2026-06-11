# FakePanic

![.NET Framework](https://img.shields.io/badge/.NET-4.7.2-5C2D91.svg?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-239120.svg?style=flat-square&logo=c-sharp&logoColor=white)
![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)

A lightweight Windows prank/joke application that simulates a critical system failure. 

The project is pre-configured to compile into a **single, standalone executable file** (`.exe`). All required dependencies are embedded directly into the binary, and clutter files (like documentation XMLs) are automatically stripped out during the build process to ensure a completely clean output directory.

---

## 📥 Download & How to Run

### 👤 For End Users (Just want to play the prank)
1. Navigate to the **[Releases](https://github.com/wh0amitg/FakePanic/releases)** page of this GitHub repository.
2. Download the latest standalone **`FakePanic.exe`** file.
3. Run the executable. 

> 💡 **No Installation Required:** Modern versions of **Windows 10 and Windows 11** come with **.NET Framework 4.7.2 pre-installed** by default. You do not need to download or install anything else to run the app. If you are on an older OS version and the app fails to launch, download the [.NET Framework 4.7.2 Runtime](https://dotnet.microsoft.com/download/dotnet-framework/net472).

### 💻 For Developers (Want to view/modify the code)
To open and work with the source code, you need to download and install:
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community edition is free).
* During installation, make sure to check the **.NET Desktop Development** workload.

---

## 🚀 Features
* **Monolithic Build (Portable):** External libraries (including NAudio and system dependencies) are packed inside `FakePanic.exe` using Costura.Fody.
* **Zero Clutter:** Custom MSBuild targets guarantee that no `.xml`, `.pdb`, or `.config` files are left behind in your final Release folder.

---

## ⚠️ Disclaimer & Safety

This application is developed **strictly for educational and entertainment purposes (pranks/jokes)**. 
* **NOT Malware:** It does not contain any malicious payloads, virus behavior, or spyware.
* **Completely Harmless:** It does not modify system files, inject into other processes, encrypt data, or alter registry keys. It is purely a visual and audio simulation.
* **Easy to Close:** You can instantly close the program at any time by pressing the **`CTRL + F12`** key combination.

---

## 🧠 How It Works (Under the Hood)

The application is built using a clean, lightweight stack to ensure high performance and zero external dependencies for the end-user:

* **UI & Core Logic:** Developed in **C#** utilizing **Windows Forms (WinForms)** on top of **.NET Framework 4.7.2**. It leverages native Windows API calls to handle full-screen rendering and window management for a convincing effect.
* **Audio Processing:** Uses the **NAudio** library to interact with Windows audio APIs, enabling low-level playback of error sound effects or simulated system alerts.
* **Dependency Embedding:** Powered by **Costura.Fody**. Instead of forcing the user to carry multiple `.dll` files around, Costura grabs all external assemblies (like NAudio and its modules) and compresses them directly inside the final compiled `FakePanic.exe`.
* **Output Optimization:** Features a custom **MSBuild Target** pipeline that intercepts the compilation process. It automatically strips away bulky `.xml` documentation files, `.pdb` debugging databases, and `.config` files, ensuring that the compilation folder yields a single, pure executable ready for deployment.

---

## 🛠️ Compilation & Build

If you want to compile the executable yourself:

1. Clone the repository:
   ```bash
   git clone [https://github.com/wh0amitg/FakePanic.git](https://github.com/wh0amitg/FakePanic.git)
