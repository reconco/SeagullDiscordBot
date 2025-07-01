@echo off
set SKIP_VERSION_UPDATE=true
dotnet publish -r linux-arm64 "..\SeagullDiscordBot"
pause