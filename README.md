# Zizi Bot .NET

[![CodeFactor](https://www.codefactor.io/repository/github/wintendev/ZiziBot.net/badge)](https://www.codefactor.io/repository/github/wintendev/ZiziBot.net)
[![License](https://img.shields.io/github/license/WinTenDev/ZiziBot.NET?label=License&color=brightgreen&cacheSeconds=3600)](./LICENSE)
![Lines of code](https://img.shields.io/tokei/lines/github/WinTenDev/ZiziBot.NET?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/WinTenDev/ZiziBot.NET?style=flat-square)
[![Zizi Bot](https://github.com/WinTenDev/ZiziBot.NET/actions/workflows/zizibot-build.yml/badge.svg?branch=master)](https://github.com/WinTenDev/ZiziBot.NET/actions/workflows/zizibot-build.yml)

[![GitHub last commit](https://img.shields.io/github/last-commit/WinTenDev/ZiziBot.NET?style=flat-square)](https://github.com/WinTenDev/ZiziBot.NET)
![GitHub commit activity](https://img.shields.io/github/commit-activity/m/WinTenDev/ZiziBot.NET?style=flat-square)
![GitHub commit activity](https://img.shields.io/github/commit-activity/w/WinTenDev/ZiziBot.NET?style=flat-square)

Official repository Zizi Bot, written in .NET

# Preparation

1. .NET 5 SDK
2. MySQL/MariaDB
3. Nginx or OpenLiteSpeed for reverse proxy (Optional)
4. ClickHouse (Optional, for analytic)
5. GoogleDrive API (Optional)
6. RavenDB (Optional)
7. Uptobox Token (Optional)
8. Datadog (Optional, for logging)

# Run Development

- Clone this repo and open Zizi.sln using your favorite IDE or Text Editor.
- Install MySQL/MariaDB and create database e.g. zizibot.
- Copy appconfig.example.json to appconfig.jon and fill some property.
- Press Start in your IDE to start debugging or via CLI.
- Your bot has ran local as Developvent using Poll mode.

# Run Production

- Install .NET 5 ASP.NET Runtime
- Server with domain name include HTTPS support (e.g https://yoursite.co.id)
- Run `dotnet publish` and upload your binary.
- Setup proxy for Web Server for Zizi.Bot localhost, [here example](https://www.google.com/search?client=firefox-b-d&q=nginx+reverse+proxy+example).
- Launch bot with `./Zizi.Bot`, then WebHook will automatically ensured.

Currently running on [Zizi Bot](t.me/MissZiziBot) and under beta in [Zizi Beta Bot](t.me/MissZiziBetaBot)
