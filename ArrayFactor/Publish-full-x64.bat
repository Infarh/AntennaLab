@echo off
chcp 65001 >nul
cd /d "%~dp0"

set "VERSION=%~1"
set "VERSION_ARG="
if not "%VERSION%"=="" set "VERSION_ARG=/p:Version=%VERSION%"

dotnet publish -v d /p:PublishProfile=Properties/PublishProfiles/Full-x64.pubxml %VERSION_ARG%
if errorlevel 1 (
    echo Ошибка публикации Full-x64.
    exit /b 1
)

set "PublishDir=!release\AntennaLab-Full-x64"
set "ArchiveName=!release\AntennaLab-Full-x64.zip"

if exist "%ArchiveName%" del "%ArchiveName%"

powershell -NoProfile -Command "Compress-Archive -Path '%PublishDir%\*' -DestinationPath '%ArchiveName%'"
if errorlevel 1 (
    echo Ошибка создания архива %ArchiveName%.
    exit /b 1
)

echo Публикация завершена. Архив создан: %ArchiveName%
