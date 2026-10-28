# Разработка и работа агентов

Здесь собраны процедуры работы агентов, инструменты переноса и накопленные инженерные доказательства. Устройство проекта и контракты компонентов описаны в [инженерной документации](../engineering/README.md).

1. Прочитайте [архитектуру и границы ответственности](../engineering/architecture.md), затем [CONTRIBUTING](../../CONTRIBUTING.md).
2. Используйте SDK из [global.json](../../global.json) и [справочник сборки](../engineering/build-and-packaging.md). Проверяйте Native AOT и платформенные изменения на соответствующей ОС.
3. Для распознавания отделяйте работающий публичный путь от исследовательских прототипов. [Индекс исследований](research/README.md) объясняет, где искать состояние и доказательства.

| Задача | Материалы |
|---|---|
| Сборка, пакеты, smoke-проверки | [Build and packaging](../engineering/build-and-packaging.md) |
| Перенос движка | [Recognition port automation](tooling/recognition-port-automation.md) |
| Пакетный экспорт функций | [Batch function export](tooling/batch-function-export.md) |
| Сборка и происхождение Giac | [Giac runtime](tooling/giac-runtime-build.md), [POSIX](tooling/giac-posix-runtime.md) |
| Нагрузочные сценарии | [Performance](tooling/performance.md) |
| Подробности функций и интеграции | [Маски](../engineering/reference/window-masks.md), [AI-перо](../engineering/reference/ai-pen.md), [Giac](../engineering/reference/math-solver.md), [полный справочник рисования](../engineering/reference/desktop-guide.md) |
| Библиотеки и пакеты | [InkRecognition](../../src/InkRecognition/README.md), [NuGet README](../../src/InkRecognition/PACKAGE.md), [ресурсы моделей](../../models/README.md) |
| Сервер, хранение и API | [Self-hosting](../engineering/deployment.md), [исходники сервера](../../src/BlurMask.Server) |

Исследовательская заметка фиксирует конкретную проверку и дату. Не превращайте числовой контроль, сравнение исходников или пройденный отдельный этап в утверждение о качестве распознавания. Сохраняйте первую ошибку и происхождение входных данных; изменение квот и подмена входа не исправляют алгоритм.

При обновлении документации проверяйте относительные ссылки. `docs/releases/`, `docs/licenses/`, `docs/THIRD-PARTY-NOTICES.txt` и `docs/ai-fonts.json` сохраняют пути, используемые выпуском, упаковкой и инструментами.
