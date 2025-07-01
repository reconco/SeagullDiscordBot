@echo off
set SKIP_VERSION_UPDATE=true
dotnet build -r linux-arm64 "..\SeagullDiscordBot"
pause