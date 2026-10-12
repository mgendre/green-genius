---
name: dotnet-logging
description: "How to log in the .NET code here — ILogger<T> from the constructor, a message template with named values, which level, what never goes in a log. Read before writing a log line, catching an exception, or when you want to print something to understand a failure."
---

# Logging

Reference: `GreenGenius.App/Services/PlantImporter.cs`. The only way to write a log is `ILogger<T>`. Never `Console.WriteLine`, `Console.Error`, `Debug.WriteLine` or `Trace`, not even for a moment to understand a failure.

## Recipe

1. The class receives `ILogger<TheClass> logger` in its primary constructor. Nothing to register: the container provides it.
2. The message is a constant template with named values in `{PascalCase}`; the values follow as arguments, in the same order.
3. An exception is the first argument, never a part of the text.

```csharp
public sealed class PlantImporter(ApplicationDbContext dbContext, ILogger<PlantImporter> logger)
{
    private async Task ImportAsync(string key)
    {
        try
        {
            // …
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Plant {Key} failed to import from JSON", key);
        }
    }
}
```

## Which level

| Level | When |
|---|---|
| `LogError` | Something failed and the work it belonged to is lost: with the exception. |
| `LogWarning` | Something unexpected was skipped and the work goes on: `"Plant {Key} skipped: empty or invalid JSON"`. |
| `LogInformation` | A step a person running the app wants to see once: an import finished, with its counts. Never inside a loop over rows. |
| `LogDebug` | A detail useful only while developing. |

## Rules

- A handler of `Features/` logs nothing and catches nothing: an exception goes up to `GlobalExceptionHandler`, which answers. Never a `try`/`catch` in a handler to log an error and throw it again.
- An exception is logged once, where it is handled. Logged and thrown again is the same error twice.
- Never `$"…{value}…"`, `string.Format` or `+` in the message: the template and its arguments.
- Never the message of an exception as text (`ex.Message`): pass `ex`.
- Never a secret, a connection string, a password, a token, or the content of a request body. An id is enough to find a row.
- English, one sentence without a final period.

## To understand a failing test

Do not print. Read what the test already says: the assertion message, the status code and the `ProblemDetails` body, the stack trace `dotnet test` prints. Still unknown: assert the intermediate value in the test, run it, read the failure. A `Console` line left in the code is a failed task.

## In a unit test

A class that takes `ILogger<T>` gets `NullLogger<T>.Instance` (`Microsoft.Extensions.Logging.Abstractions`). Mock the logger only when the log line is the behaviour under test.
