@echo off
chcp 65001 >nul
cd /d "%~dp0"

for /f "usebackq delims=" %%v in (`powershell -NoProfile -Command "$d=(Get-Date).ToUniversalTime().AddHours(3); '1.'+$d.ToString('yy.Mdd.Hmm')"`) do set "VERSION=%%v"

echo Версия: %VERSION%

call Publish-small.bat %VERSION%
if errorlevel 1 exit /b 1

call Publish-full-x64.bat %VERSION%
if errorlevel 1 exit /b 1

call Publish-full-x32.bat %VERSION%
if errorlevel 1 exit /b 1

echo Все публикации завершены.
pause
