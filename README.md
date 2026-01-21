<div align="center">

# 🐀 Ratgore

**Форк Space Station 14 с уникальными механиками и контентом**

[![Discord](https://img.shields.io/discord/1318776836599320657?style=for-the-badge&logo=discord&logoColor=white&label=Discord&color=%237289da)](https://discord.gg/3FMFTxYQYJ)
[![GitHub License](https://img.shields.io/github/license/odleer/ratgore?style=for-the-badge)](./LEGAL-RU.md)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?&style=for-the-badge)](https://dotnet.microsoft.com/)

[Discord сервер](https://discord.gg/3FMFTxYQYJ) • [Лицензия](./LEGAL-RU.md) • [English](./README-EN.md)

</div>

---

## 📋 О проекте

**Ratgore** — это форк [Space Station 14](https://github.com/space-wizards/space-station-14), космического симулятора на движке Robust Toolbox. Проект добавляет уникальные механики, контент и улучшения геймплея с фокусом на атмосферу и уникальный игровой опыт.

## 🚀 Быстрый старт

### Требования

- **Git** — [скачать](https://git-scm.com/downloads)
- **.NET SDK 9.0.101 или выше** — [скачать](https://dotnet.microsoft.com/download/dotnet/9.0)

### 🍃 Windows

```
# 1. Клонируйте репозиторий
git clone https://github.com/ss14-art/ratgore.git
cd ratgore

# 2. Загрузите движок
git submodule update --init --recursive

# 3. Соберите проект
Scripts\bat\buildAllRelease.bat

# 4. Запустите клиент и сервер
Scripts\bat\runQuickAll.bat
```

**Готово!** Подключитесь к **localhost** в клиенте и играйте

### 🐧 Linux / macOS

```
# 1. Клонируйте репозиторий
git clone https://github.com/ss14-art/ratgore.git
cd ratgore

# 2. Загрузите движок
git submodule update --init --recursive

# 3. Соберите проект
chmod +x Scripts/sh/buildAllRelease.sh
Scripts/sh/buildAllRelease.sh

# 4. Запустите клиент и сервер
chmod +x Scripts/sh/runQuickAll.sh
Scripts/sh/runQuickAll.sh
```

**Готово!** Подключитесь к **localhost** в клиенте и играйте

## 📜 Лицензия

Код проекта распространяется под лицензией **GNU AGPLv3**. Ассеты имеют различные лицензии (в основном CC-BY-SA 3.0).

Подробную информацию о лицензиях смотрите в файле [LEGAL-RU.md](./LEGAL-RU.md).


