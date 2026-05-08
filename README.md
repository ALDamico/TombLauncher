<div align="center">

# 🏺 Tomb Launcher

**Your all-in-one companion for Tomb Raider custom levels.**

[![License: MIT](https://img.shields.io/badge/License-MIT-gold.svg)](LICENSE.md)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Avalonia UI](https://img.shields.io/badge/Avalonia_UI-purple?logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggZD0iTTEyIDJMMyAyMmgxOEwxMiAyeiIgZmlsbD0id2hpdGUiLz48L3N2Zz4=)](https://avaloniaui.net/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-lightgrey)]()

</div>

---

## What is Tomb Launcher?

If you've ever wanted to play fan-made Tomb Raider levels but found the process of downloading, installing, and managing them frustrating, **Tomb Launcher** is for you.

The Tomb Raider community has been creating incredible custom levels for over two decades. It all started with the Tomb Raider Level Editor (TRLE) — originally shipped with *Tomb Raider: Chronicles* back in 2000 — and has since evolved into a vibrant ecosystem of engines and tools. Today, builders use a variety of platforms to craft their adventures:

- **TRLE** — The original Level Editor, still widely used
- **TRNG** — An enhanced scripting engine built on top of TRLE, adding powerful new capabilities
- **TombEngine** — A modern, open-source engine rewrite with support for new features and improved visuals
- **TR1X** / **TR2X** — Open-source reimplementations of the Tomb Raider 1 and 2 engines, enabling custom levels for the classic games

Talented builders have crafted thousands of adventures spanning ancient temples, sunken shipwrecks, snowy mountain fortresses, and places Lara Croft has never been before. Some of these levels rival the quality of the original games.

But here's the problem: finding these levels means browsing multiple websites, each with a different interface. Installing them usually involves downloading ZIP files, manually extracting them into the right folder, making sure the right version of the game engine is in place, and hoping everything works. If it doesn't? You're on your own.

**Tomb Launcher changes that.** It brings everything you need into one clean, modern interface. Search across multiple community sites, download with a single click, and start playing — Tomb Launcher handles the setup behind the scenes. It also keeps track of everything you've played, so you can pick up where you left off or revisit old favorites anytime.

Whether you're a seasoned raider who's been playing custom levels since the early 2000s or someone who just discovered the TRLE community, Tomb Launcher is designed to make your experience as smooth as possible.

---

## 📸 Screenshots

<div align="center">
<table>
<tr>
  <td align="center"><img src="https://www.tomblauncher.app/assets/images/welcome-page.png" alt="Welcome Dashboard" width="420"/><br/><sub>Welcome Dashboard</sub></td>
  <td align="center"><img src="https://www.tomblauncher.app/assets/images/game%20search.png" alt="Game Search" width="420"/><br/><sub>Game Search</sub></td>
</tr>
<tr>
  <td align="center"><img src="https://www.tomblauncher.app/assets/images/games-list%20%28grid%20view%29.png" alt="Library – Grid View" width="420"/><br/><sub>Library – Grid View</sub></td>
  <td align="center"><img src="https://www.tomblauncher.app/assets/images/games-list%20%28datagrid%20view%29.png" alt="Library – List View" width="420"/><br/><sub>Library – List View</sub></td>
</tr>
</table>
</div>

---

## ✨ Features

### 🔍 Search & Discover

Finding custom levels used to mean browsing three different websites with very different search experiences. Tomb Launcher brings them all together.

You can search for levels from within the app, pulling results simultaneously from:
- [**TRLE.net**](https://www.trle.net) — The original and largest Tomb Raider Level Editor community site
- [**TRCustoms.org**](https://trcustoms.org) — A modern, community-driven platform with rich metadata
- [**AspideTR.com**](https://aspidetr.com) — An Italian community portal with a curated collection of levels

Results from all sources appear side by side, so you can compare ratings, read descriptions, and pick the adventure that suits your mood — all without leaving the app.

### 📦 One-Click Install

Found a level you want to play? Just click **Download**. Tomb Launcher takes care of the rest:

1. Downloads the level archive from the source
2. Extracts the files into a managed folder
3. Sets up the level so it's ready to launch

No more hunting for the right folder, no wondering if you extracted things correctly. Just click, wait a few seconds, and play.

### 🎮 Game Library

Every level you install lives in your personal library — a clean, organized view of your entire collection. At a glance, you can see:

- Which levels are installed
- When you last played each one
- How much time you've spent on each level
- Whether a level came from TRLE.net, TRCustoms, or AspideTR

Think of it as your personal Tomb Raider dashboard.

### 📊 Statistics

Curious about your gaming habits? Tomb Launcher silently tracks your play sessions and turns them into interesting insights:

- **Most played levels** — See which adventures you kept coming back to
- **Average session length** — Are you a quick raider or a marathon explorer?
- **Play patterns by day of the week** — Discover which days you raid the most
- **Disk space usage** — See how much room your collection is taking up, broken down per level

All data stays local on your machine. Tomb Launcher doesn't phone home or share anything with anyone.

### 💾 Savegame Management

Each level's savegames are tracked and associated with the level itself. You can see your save files at a glance without digging through folders. Never lose progress again — and never accidentally overwrite a save from a different level.

### 🎲 Feeling Lucky?

Can't decide what to play next? Tomb Launcher can pick a random level from your library for you. It's a great way to rediscover levels you installed months ago and forgot about.

### 🌍 Localization

Tomb Launcher is currently available in **English** and **Italian**, with more languages welcome (see [Contributing](#-contributing)).

The app automatically detects your system language and switches accordingly. You can also change the language manually from the settings.

### 🖥️ Cross-Platform

Tomb Launcher runs natively on both **Windows** and **Linux**. It's built with [Avalonia UI](https://avaloniaui.net/), a modern cross-platform UI framework, so the experience is consistent regardless of your operating system.

On Linux, you can run it as an AppImage, install it via DEB/RPM, or build from source. On Windows, a traditional installer gets you up and running in seconds.

### 🤖 AI-Assisted Troubleshooting (Laura)

> **Experimental** — this feature requires a local LLM backend and is disabled by default.

Stuck on a level that won't launch? Laura is Tomb Launcher's built-in AI assistant, designed to help you diagnose and fix common issues with custom levels and legacy game engines.

**Opening the chat**

On any game's detail page, click the **Talk to Laura** button. Laura automatically receives context about the game you're troubleshooting — engine type, last exit code, crash logs, and stderr output — so you don't have to paste anything manually.

**Enabling AI features**

AI features are turned off by default. To enable them, open **Settings → AI** and choose a backend:

| Backend | Notes |
|---------|-------|
| [Ollama](https://ollama.com/) | Run local LLMs with a single command. Recommended for most users. |
| [LM Studio](https://lmstudio.ai/) | A desktop app for running local models with a friendly UI. |

Configure the endpoint URL to match your backend (e.g. `http://localhost:11434` for Ollama). Any model supported by the chosen backend can be used, though a reasoning-capable model will give better results.

**Knowledge base**

Laura's answers are grounded in a curated knowledge base of troubleshooting guides and known issues for Tomb Raider custom levels. The knowledge base is fetched automatically from [tomblauncher.app](https://www.tomblauncher.app) and kept up to date in the background — no manual updates needed.

**Known limitations**

- Requires a running local LLM backend (Ollama or LM Studio). No cloud API keys are needed, and no data leaves your machine.
- Answer quality depends on the model you choose. Larger models generally perform better.
- The feature is experimental. Results may occasionally be incomplete or incorrect.

---

## 📥 Installation

Head to the [Releases](https://github.com/ALDamico/TombLauncher/releases) page and download the latest version for your platform.

### Windows

Download the `.exe` installer and run it. The setup wizard will guide you through the installation. Once installed, you'll find Tomb Launcher in your Start Menu.

### Linux

Several package formats are available:

- **AppImage** — Download, make executable (`chmod +x`), and run. No installation required.
- **DEB** — For Debian/Ubuntu-based distributions. Install with `sudo dpkg -i tomb-launcher.deb`.
- **RPM** — For Fedora/openSUSE-based distributions. Install with `sudo rpm -i tomb-launcher.rpm`.

You can also launch Tomb Launcher from the terminal with `tomb-launcher`.

---

## 🗺️ Roadmap

Tomb Launcher is under active development. Here's what's planned for future releases:

- 🛠️ **Built-in troubleshooting** — Automatically detect and fix common issues with legacy game engines on modern systems
- 🕹️ **AntiMicroX integration** — Seamless gamepad support through [AntiMicroX](https://github.com/AntiMicroX/antimicrox), so you can play from the couch
- 🎨 **dgVoodoo integration** *(Windows only)* — A graphics wrapper that improves compatibility with modern GPUs, solving visual glitches and rendering issues
- 🖥️ **Widescreen fix auto-apply** — Automatically patch levels to support 16:9 and ultrawide aspect ratios instead of the original 4:3
- ⚙️ **Per-game settings** — Customize resolution, keymaps, and other options for each individual level
- 🍎 **macOS support** — A native build for macOS, bringing Tomb Launcher to Apple Silicon and Intel Macs

Have a feature request? Feel free to [open an issue](https://github.com/ALDamico/TombLauncher/issues) and share your ideas.

---

## 🤓 For the Nerds

If you're a developer, here's what's under the hood.

### Tech Stack

| Layer | Technology |
|-------|-----------|
| **UI Framework** | [Avalonia UI](https://avaloniaui.net/) |
| **Runtime** | .NET 10 |
| **Architecture** | MVVM with [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/) |
| **Database** | SQLite via Entity Framework Core |
| **Charts** | [LiveCharts2](https://livecharts.dev/) |
| **Packaging** | [PupNet Deploy](https://github.com/kuiperzone/PupNet-Deploy) (Linux), [InnoSetup](https://jrsoftware.org/isinfo.php) (Windows) |

### Building from Source

**Prerequisites:**
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

**Clone and build:**
```bash
git clone https://github.com/ALDamico/TombLauncher.git
cd TombLauncher
dotnet build TombLauncher.slnx
```

**Run the application:**
```bash
dotnet run --project src/TombLauncher
```

**Run the tests:**
```bash
dotnet test
```

### Project Structure

The solution follows a clean separation of concerns:

```
TombLauncher/
├── src/
│   ├── TombLauncher/              # Main application (UI, ViewModels, Services)
│   ├── TombLauncher.Core/         # Core logic (platform-agnostic, no UI dependencies)
│   ├── TombLauncher.Contracts/    # Shared interfaces and DTOs
│   ├── TombLauncher.Controls/     # Reusable Avalonia UI controls
│   ├── TombLauncher.Data/         # Database access and EF Core migrations
│   └── TombLauncher.Localization/ # Localization resources (i18n)
├── tests/
│   └── TombLauncher.Tests/        # Unit tests (xUnit)
└── deploy/                        # Packaging configs (PupNet, InnoSetup)
```

The architecture keeps the core logic decoupled from the UI layer. `TombLauncher.Core` has no knowledge of Avalonia and can be tested in isolation. The UI layer follows the MVVM pattern, with ViewModels acting as the bridge between the views and the underlying services.

---

## 🤝 Contributing

Tomb Launcher is **not currently accepting code contributions**. The project is in active early development and the architecture is still evolving.

However, if you'd like to help **translate Tomb Launcher into your language**, that would be wonderful! The app currently supports English and Italian, and adding a new language is straightforward. Feel free to [open an issue](https://github.com/ALDamico/TombLauncher/issues) to get in touch.

---

## ❓ FAQ

**Q: Is Tomb Launcher affiliated with Core Design, Crystal Dynamics, or Eidos Interactive?**
No. Tomb Launcher is an independent, fan-made tool. It is not affiliated with or endorsed by any of the companies behind the Tomb Raider franchise.

**Q: Does Tomb Launcher include any game files?**
No. Tomb Launcher is a management tool. It helps you download custom levels from community websites, but it does not distribute any copyrighted game assets.

**Q: Where are my levels stored?**
Tomb Launcher keeps its data in a local application data folder on your machine. You can see the exact path in the settings.

**Q: Can I use Tomb Launcher to manage the official Tomb Raider games?**
No. Tomb Launcher is designed specifically for custom levels built with the Tomb Raider Level Editor (TRLE) and its derivatives, including TRNG, TombEngine, TR1X, and TR2X.

---

## 🤖 AI Disclaimer

Portions of Tomb Launcher's code have been written with the assistance of AI-powered coding agents. All AI-generated code is reviewed and validated by a human developer before being merged. The project maintainer takes full responsibility for the quality, correctness, and security of the codebase.

---

## 📄 License

Tomb Launcher is released under the [MIT License](LICENSE.md).

---

<div align="center">

*Made with ❤️ for the Tomb Raider community.*

</div>
