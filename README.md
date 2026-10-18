Используемые nuget пакеты

Api:

    Refit.* — REST-клиент на интерфейсах.
    Microsoft.Extensions.Http.Resilience — retry/timeout/circuit-breaker (Polly).

Client:

    CommunityToolkit.Mvvm — генераторы INPC и команд.
    Microsoft.Extensions.DependencyInjection — DI.
    Polly.Core — типы исключений от Polly (например BrokenCircuitException).
    Serilog.* — логи в файл через ILogger.

Tests:

    xunit / Microsoft.NET.Test.Sdk / runner — тесты.
    Moq — моки.
    FluentAssertions — читаемые проверки.

Конфигурации

    Debug
    Release
    FakeApi — in-process fake api, работает без сети.

FakeApi — отдельная конфигурация WclTest.Client: условный ProjectReference на Api.Fake +
#if FAKE_API в DI. В Debug/Release fake не подключается и не собирается (нет .Build.0 в .sln).


<img width="901" height="589" alt="scrn" src="https://github.com/user-attachments/assets/db4a350f-2666-4a3f-a43d-e7413e07d9a5" />
