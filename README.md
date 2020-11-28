# WinTenBot.NET

Official repository Zizi.Bot, written in .NET

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
- Setup proxy for Web Server by Zizi.Bot localhost, [here example](https://www.google.com/search?client=firefox-b-d&q=nginx+reverse+proxy+example).
- Launch bot with `./Zizi.Bot`, then WebHook will automatically ensured.

Currently running on [Zizi Bot](t.me/MissZiziBot) and under beta in [Zizi Beta Bot](t.me/MissZiziBetaBot)
