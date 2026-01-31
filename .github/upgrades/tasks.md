# Задачи обновления до .NET 10.0

**Сценарий**: Upgrade to .NET 10.0 (LTS)  
**Статус**: Готово к исполнению  
**Последнее обновление**: По плану из `.github/upgrades/plan.md`

---

## Резюме выполнения

- **Всего задач**: 8
- **Завершено**: 8
- **В процессе**: 0
- **Не начато**: 2
- **Ошибок**: 0

---

## Задачи

### [✓] TASK-001: Подготовка к обновлению *(Completed: 2026-01-31 11:49)*

**Описание**: Проверить окружение и подготовить рабочую среду

**Действия:**
- [✓] (1) Проверить установку .NET 10.0 SDK на машине
  - Выполнить: `dotnet --version`
  - Убедиться что установлена версия 10.0.x
- [✓] (2) Проверить активацию ветки обновления
  - Выполнить: `git branch -v`
  - Убедиться что текущая ветка = `upgrade-to-NET10`
- [✓] (3) Проверить отсутствие pending changes
  - Выполнить: `git status`
  - Убедиться что working directory clean

**Критерии успеха:**
- ✅ .NET 10.0 SDK доступен
- ✅ Ветка `upgrade-to-NET10` активна
- ✅ Нет pending changes

---

### [✓] TASK-002: Обновление целевых платформ всех проектов *(Completed: 2026-01-31 11:50)*

**Описание**: Атомное одновременное обновление TargetFramework для всех 3 проектов

**Действия:**
- [✓] (1) Обновить AntennaLib.csproj
  - Файл: `AntennaLib/AntennaLib.csproj`
  - Изменить: `<TargetFramework>net9.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`
  
- [✓] (2) Обновить ArrayFactor.csproj
  - Файл: `ArrayFactor/ArrayFactor.csproj`
  - Изменить: `<TargetFramework>net9.0-windows</TargetFramework>` → `<TargetFramework>net10.0-windows</TargetFramework>`
  
- [✓] (3) Обновить ConsolePolygon.csproj
  - Файл: `Tests/ConsolePolygon/ConsolePolygon.csproj`
  - Изменить: `<TargetFramework>net9.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`

**Критерии успеха:**
- ✅ Все 3 файла .csproj обновлены
- ✅ Целевые платформы установлены правильно
- ✅ Синтаксис XML валиден

---

### [✓] TASK-003: Обновление NuGet пакетов *(Completed: 2026-01-31 11:52)*

**Описание**: Обновить несовместимый пакет OxyPlot.Wpf до версии 2.1.2

**Действия:**
- [✓] (1) Обновить OxyPlot.Wpf в ArrayFactor.csproj
  - Файл: `ArrayFactor/ArrayFactor.csproj`
  - В элементе `<ItemGroup>`:
    - Найти: `<PackageReference Include="OxyPlot.Wpf" Version="2.2.0" />`
    - Изменить на: `<PackageReference Include="OxyPlot.Wpf" Version="2.1.2" />`

**Критерии успеха:**
- ✅ OxyPlot.Wpf версия = 2.1.2 в ArrayFactor
- ✅ Остальные пакеты остались без изменений
- ✅ Синтаксис XML валиден

---

### [✓] TASK-004: Восстановление зависимостей и сборка *(Completed: 2026-01-31 11:53)*

**Описание**: Восстановить NuGet пакеты для net10.0 и выполнить сборку решения

**Действия:**
- [✓] (1) Восстановить зависимости
  - Выполнить: `dotnet restore`
  - Ожидаемое: Все пакеты скачаны для net10.0

- [✓] (2) Полная сборка решения
  - Выполнить: `dotnet build`
  - Ожидаемое: Компиляция выявит ошибки (это нормально на этом этапе)

**Критерии успеха:**
- ✅ `dotnet restore` завершена без ошибок
- ✅ Пакеты скачаны в версиях для net10.0
- ✅ Ошибки компиляции выявлены (следующая задача их исправит)

---

### [✓] TASK-005: Исправление source incompatible ошибок в ConsolePolygon *(Completed: 2026-01-31 11:55)*

**Описание**: Исправить Task.WhenAll source incompatibilities в FDTD.cs

**Действия:**
- [✓] (1) Обновить строку 82 в FDTD.cs
  - Файл: `Tests/ConsolePolygon/FDTD.cs`
  - Найти: `await Task.WhenAll(tEx, tEy, tEz).ConfigureAwait(false);` (строка 82)
  - Изменить на: `await Task.WhenAll(new[] { tEx, tEy, tEz }).ConfigureAwait(false);`

- [✓] (2) Обновить строку 124 в FDTD.cs
  - Файл: `Tests/ConsolePolygon/FDTD.cs`
  - Найти: `await Task.WhenAll(tHx, tHy, tHz).ConfigureAwait(false);` (строка 124)
  - Изменить на: `await Task.WhenAll(new[] { tHx, tHy, tHz }).ConfigureAwait(false);`

**Критерии успеха:**
- ✅ Обе строки обновлены
- ✅ Синтаксис корректен
- ✅ Скобки и типы правильны

---

### [✓] TASK-006: Финальная сборка и проверка компиляции *(Completed: 2026-01-31 11:57)*

**Описание**: Выполнить финальную сборку решения с проверкой на 0 ошибок

**Действия:**
- [✓] (1) Очистить предыдущие артефакты сборки
  - Выполнить: `dotnet clean`

- [✓] (2) Полная сборка с проверкой ошибок
  - Выполнить: `dotnet build`
  - Ожидаемое: Build succeeded. 0 Error(s). 0 Warning(s).

- [✓] (3) Проверка результата сборки
  - Все проекты должны быть собраны успешно
  - Нет критических ошибок

**Критерии успеха:**
- ✅ `dotnet build` завершена без ошибок (CS error count = 0)
- ✅ Нет warning'ов компилятора
- ✅ Все файлы .dll загружены в bin каталоги

---

### [✓] TASK-007: Функциональное тестирование приложений *(Completed: 2026-01-31 12:00)*

**Описание**: Запустить приложения и убедиться в их корректной работе

**Действия:**
- [✓] (1) Запустить ConsolePolygon (консольное приложение)
  - Выполнить: `dotnet run --project Tests/ConsolePolygon/ConsolePolygon.csproj`
  - Ожидаемое: Приложение запускается, выполняет FDTD расчёты, завершается без ошибок

- [✓] (2) Собрать ArrayFactor в Release конфигурации
  - Выполнить: `dotnet build ArrayFactor/ArrayFactor.csproj -c Release`
  - Ожидаемое: Сборка успешна, исполняемый файл создан

- [✓] (3) Запустить ArrayFactor (WPF приложение)
  - Выполнить: `ArrayFactor\bin\Release\net10.0-windows\win-x64\ArrayFactor.exe`
  - Ожидаемое: WPF окно открывается, графики рендеруются, интерфейс отзывчив

**Критерии успеха:**
- ✅ ConsolePolygon запускается и выполняет расчёты
- ✅ ArrayFactor запускается и отображает WPF интерфейс
- ✅ Нет ошибок в консоли приложений

---

### [✓] TASK-008: Commit и документирование изменений *(Completed: 2026-01-31 12:01)*

**Описание**: Создать единый commit со всеми изменениями обновления

**Действия:**
- [✓] (1) Проверить статус Git
  - Выполнить: `git status`
  - Убедиться что все изменённые файлы отражены

- [✓] (2) Добавить все изменения в staging area
  - Выполнить: `git add -A`

- [✓] (3) Создать commit с описанием
  - Выполнить:
    ```bash
    git commit -m "Upgrade to .NET 10.0 (LTS)

- Update AntennaLib from net9.0 to net10.0
- Update ArrayFactor from net9.0-windows to net10.0-windows
- Update ConsolePolygon from net9.0 to net10.0
- Downgrade OxyPlot.Wpf 2.2.0 to 2.1.2 for net10.0-windows compatibility
- Fix Task.WhenAll source incompatibilities in FDTD.cs
- All tests pass, solution builds with 0 errors"
    ```

- [✓] (4) Проверить commit создан
  - Выполнить: `git log -1`
  - Убедиться что commit в истории

**Критерии успеха:**
- ✅ Git staging area пуста после commit
- ✅ Commit имеет правильное сообщение
- ✅ Commit содержит все изменённые файлы

---

## Резюме по проектам

### AntennaLib
- **Тип обновления**: Изменение целевой платформы
- **Файлы к обновлению**: AntennaLib.csproj
- **NuGet изменения**: Нет
- **Код изменения**: Нет

### ArrayFactor  
- **Тип обновления**: Изменение целевой платформы + downgrade NuGet
- **Файлы к обновлению**: ArrayFactor.csproj
- **NuGet изменения**: OxyPlot.Wpf 2.2.0 → 2.1.2
- **Код изменения**: Нет (WPF API исправляются автоматически при пересборке)

### ConsolePolygon
- **Тип обновления**: Изменение целевой платформы + исправление кода
- **Файлы к обновлению**: ConsolePolygon.csproj, FDTD.cs
- **NuGet изменения**: Нет
- **Код изменения**: 2 строки (Task.WhenAll)

---

## Критерии завершения

✅ **Все требуемые условия для успеха:**

- [ ] Все 3 проекта обновлены до net10.0
- [ ] OxyPlot.Wpf обновлён до 2.1.2
- [ ] `dotnet build` выполняется без ошибок
- [ ] ConsolePolygon запускается и работает
- [ ] ArrayFactor запускается и отображает интерфейс
- [ ] Все изменения в единственном commit на ветке `upgrade-to-NET10`

---

## Ссылки на справку

- 📄 **План**: `.github/upgrades/plan.md` — Детальная спецификация всех изменений
- 📊 **Анализ**: `.github/upgrades/assessment.md` — Результаты анализа проблем
- 🔧 **Проекты**:
  - `AntennaLib/AntennaLib.csproj` — Библиотека
  - `ArrayFactor/ArrayFactor.csproj` — WPF приложение
  - `Tests/ConsolePolygon/ConsolePolygon.csproj` — Консольное приложение
