@echo off
setlocal enabledelayedexpansion

echo Очистка локальных кэшей проекта...
echo.

set errors=0

REM Очистка Visual Studio кэша
if exist .vs (
    echo Удаляю .vs...
    rmdir /s /q .vs 2>nul
    if !errorlevel! equ 0 (
        echo [OK] .vs удален
    ) else (
        echo [ОШИБКА] Не удалось удалить .vs
        set /a errors+=1
    )
) else (
    echo [ПРОПУСК] .vs не найден
)
echo.

REM Очистка ReSharper кэша
if exist _ReSharper.Caches (
    echo Удаляю _ReSharper.Caches...
    rmdir /s /q _ReSharper.Caches 2>nul
    if !errorlevel! equ 0 (
        echo [OK] _ReSharper.Caches удален
    ) else (
        echo [ОШИБКА] Не удалось удалить _ReSharper.Caches
        set /a errors+=1
    )
) else (
    echo [ПРОПУСК] _ReSharper.Caches не найден
)
echo.

REM Очистка .NET кэшей (опционально)
if exist bin (
    echo Удаляю bin...
    rmdir /s /q bin 2>nul
    if !errorlevel! equ 0 (
        echo [OK] bin удален
    ) else (
        echo [ОШИБКА] Не удалось удалить bin
        set /a errors+=1
    )
) else (
    echo [ПРОПУСК] bin не найден
)
echo.

if exist obj (
    echo Удаляю obj...
    rmdir /s /q obj 2>nul
    if !errorlevel! equ 0 (
        echo [OK] obj удален
    ) else (
        echo [ОШИБКА] Не удалось удалить obj
        set /a errors+=1
    )
) else (
    echo [ПРОПУСК] obj не найден
)
echo.

if !errors! equ 0 (
    echo [УСПЕШНО] Все кэши очищены без ошибок
) else (
    echo [ВНИМАНИЕ] Возникло !errors! ошибок при очистке
)

pause