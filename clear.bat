@echo off
setlocal enabledelayedexpansion

echo Полная очистка кэшей и выходных данных...
echo.

set errors=0
set deleted=0

REM Оригинальная версия (для истории):
REM for %%d in (bin obj .vs _ReSharper.Caches) do (
REM     for /f %%f in ('dir /s /b /d /a %%d') do (
REM         @if exist "%%f" (
REM             echo rd "%%f"
REM             rd /s /q "%%f"
REM         )
REM     )
REM )
REM rd /s /q TestResults

REM Функция для удаления с проверкой существования
echo Очистка директорий в корне проекта...
for %%d in (bin obj .vs _ReSharper.Caches TestResults) do (
    if exist "%%d" (
        echo Удаляю %%d...
        rmdir /s /q "%%d" 2>nul
        if !errorlevel! equ 0 (
            echo [OK] %%d удален
            set /a deleted+=1
        ) else (
            echo [ОШИБКА] Не удалось удалить %%d
            set /a errors+=1
        )
    ) else (
        echo [ПРОПУСК] %%d не найден
    )
)

echo.

REM Рекурсивный поиск и удаление bin и obj во всех поддиректориях
echo Рекурсивная очистка bin и obj в поддиректориях...
for /r %%d in (.) do (
    for %%t in (bin obj) do (
        if exist "%%d\%%t" (
            echo Удаляю "%%d\%%t"...
            rmdir /s /q "%%d\%%t" 2>nul
            if !errorlevel! equ 0 (
                echo [OK] "%%d\%%t" удален
                set /a deleted+=1
            ) else (
                echo [ОШИБКА] Не удалось удалить "%%d\%%t"
                set /a errors+=1
            )
        )
    )
)

echo.
echo ============================================
echo Удалено директорий: !deleted!
if !errors! equ 0 (
    echo [УСПЕШНО] Полная очистка завершена без ошибок
) else (
    echo [ВНИМАНИЕ] Возникло !errors! ошибок при очистке
)
echo ============================================

pause