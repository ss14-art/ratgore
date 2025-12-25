<div align="center">

# 🐀 Ratgore

**A Space Station 14 fork with unique mechanics and custom content**

[![Discord](https://img.shields.io/discord/1318776836599320657?style=for-the-badge&logo=discord&logoColor=white&label=Discord&color=%237289da)](https://discord.gg/3FMFTxYQYJ)
[![GitHub License](https://img.shields.io/github/license/odleer/ratbite?style=for-the-badge)](./LEGAL.md)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?&style=for-the-badge)](https://dotnet.microsoft.com/)

[Discord Server](https://discord.gg/3FMFTxYQYJ) • [License](./LEGAL-RU.md) • [Русский](./README.md)

</div>

---

## 📋 About the Project

**Ratgore** is a fork of [Space Station 14](https://github.com/space-wizards/space-station-14), a space simulation game built on the Robust Toolbox engine.  
The project introduces unique mechanics, original content, and gameplay improvements with a strong focus on atmosphere and a distinctive gameplay experience.

## 🚀 Quick Start

### Requirements

- **Git** — [download](https://git-scm.com/downloads)
- **.NET SDK 9.0.101 or higher** — [download](https://dotnet.microsoft.com/download/dotnet/9.0)

### 🍃 Windows

```

# 1. Clone the repository

git clone https://github.com/odleer/ratgore.git
cd ratgore

# 2. Download the engine

git submodule update --init --recursive

# 3. Build the project

Scripts\bat\buildAllRelease.bat

# 4. Run the client and server

Scripts\bat\runQuickAll.bat

```

**Done!** Connect to **localhost** in the client and start playing 🎮

### 🐧 Linux / macOS

```

# 1. Clone the repository

git clone https://github.com/odleer/ratgore.git
cd ratgore

# 2. Download the engine

git submodule update --init --recursive

# 3. Build the project

chmod +x Scripts/sh/buildAllRelease.sh
Scripts/sh/buildAllRelease.sh

# 4. Run the client and server

chmod +x Scripts/sh/runQuickAll.sh
Scripts/sh/runQuickAll.sh

```

**Done!** Connect to **localhost** in the client and start playing 🎮

## 🤝 Contributing

We welcome contributions to the project! Before getting started, please familiarize yourself with our commit formatting rules.

### 📝 Commit Format

The changelog is updated **automatically** via GitHub Actions.  
Use the correct prefixes to ensure proper categorization of changes.

#### Bracketed format

```

[ADD] Added a new rat mechanic
[FIX] Fixed a cheese-related bug
[REMOVE] Removed an obsolete system
[TWEAK] Improved weapon balance

```

#### Non-bracketed format (with colon or comma)

```

Add: added a new rat mechanic
Fix: fixed a cheese-related bug
Tweak: improved weapon balance

```

#### Change Types

- **Add** — new features, mechanics, content
- **Fix** — bug fixes and error corrections
- **Remove** — removal of obsolete code or content
- **Tweak** — balance changes, improvements, optimization

#### Ignored Commits

The following prefixes **do not appear** in the changelog:

- `[AUTO]` — automated updates
- `[MAPS]` / `[maps]` — map and shuttle changes
- `[IGNORE]` — technical commits, refactoring

## 📜 License

The project code is licensed under **GNU AGPLv3**.  
Assets use various licenses (primarily CC-BY-SA 3.0).

For detailed license information, see the [LEGAL.md](./LEGAL.md) file.
