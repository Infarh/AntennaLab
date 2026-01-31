@echo off
setlocal enabledelayedexpansion

echo Рекурсивная очистка bin и obj директорий...
echo.

set errors=0
set deleted=0

REM Оригинальная версия (для истории):
REM for %%d in (bin obj) do for /f %%f in ('dir /s /b /d %%d') do rd /s /q "%%f"

REM Очистка рекурсивных bin директорий
for /r %%d in (.) do (
    if exist "%%d\bin" (
        echo Удаляю "%%d\bin"...
        rmdir /s /q "%%d\bin" 2>nul
        if !errorlevel! equ 0 (
            echo [OK] "%%d\bin" удален
            set /a deleted+=1
        ) else (
            echo [ОШИБКА] Не удалось удалить "%%d\bin"
            set /a errors+=1
        )
    )
)

echo.

REM Очистка рекурсивных obj директорий
for /r %%d in (.) do (
    if exist "%%d\obj" (
        echo Удаляю "%%d\obj"...
        rmdir /s /q "%%d\obj" 2>nul
        if !errorlevel! equ 0 (
            echo [OK] "%%d\obj" удален
            set /a deleted+=1
        ) else (
            echo [ОШИБКА] Не удалось удалить "%%d\obj"
            set /a errors+=1
        )
    )
)

echo.
echo Удалено директорий: !deleted!
if !errors! equ 0 (
    echo [УСПЕШНО] Очистка завершена без ошибок
) else (
    echo [ВНИМАНИЕ] Возникло !errors! ошибок при очистке
)

pause