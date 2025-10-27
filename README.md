# AppAmbit .NET MAUI SDK

**Track. Debug. Distribute.**
**AppAmbit: track, debug, and distribute your apps from one dashboard.**

Lightweight SDK for analytics, events, logging, crashes, and offline support. Simple setup, minimal overhead.

> Full product docs live here: **[docs.appambit.com](https://docs.appambit.com)**

---

## Contents

* [Features](#features)
* [Requirements](#requirements)
* [Install](#install)
* [Quickstart](#quickstart)
* [Usage](#usage)
* [Release Distribution](#release-distribution)
* [Privacy and Data](#privacy-and-data)
* [Troubleshooting](#troubleshooting)
* [Contributing](#contributing)
* [Versioning](#versioning)
* [Security](#security)
* [License](#license)

---

## Features

* Session analytics
* Event tracking with rich properties
* Error logging for quick diagnostics 
* Crash capture with stack traces and threads
* Offline support with batching, retry, and queue
* Create mutliple app profiles for staging and production
* Lightweight, modern .NET MAUI API for iOS and Android

---

## Requirements

* .NET 8.0 or newer with the .NET MAUI workload
* Visual Studio 2022 (17.6 or newer) or VS Code with .NET MAUI extensions
* **Supported targets:**

  * iOS
  * Android
* **Coming soon:** macOS and Windows

---

## Install

### NuGet

Add the package to your MAUI project:

```bash
dotnet add package com.AppAmbit.Sdk
# or specify version
dotnet add package com.AppAmbit.Sdk --version 1.0.2
```

Or, using Visual Studio:

* Right-click your project → **Manage NuGet Packages…**
* Search for **AppAmbit** and install the latest stable version.

---

## Quickstart

Initialize AppAmbit early in your application lifecycle (e.g. in `MauiProgram.cs`.

```csharp
using AppAmbit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("YOUR-APPKEY");

        return builder.Build();
    }
}
```

---

## Usage

* **Session activity** – automatically tracks user session starts, stops, and durations
* **Track events** – send structured events with custom properties

  ```csharp
      Analytics.TrackEvent("Audio started", new Dictionary<string, string> {
        { "Category", "Music" },
        { "FileName", "favorite.mp3"}
    });
  ```
* **Logs**: add structured log messages for debugging

  ```csharp
    Crashes.LogError("This code should not be reached");
  ```
* **Crash Reporting**: uncaught crashes are automatically captured and uploaded on next launch

---

## Release Distribution

* Push the artifact to your AppAmbit dashboard for distribution via email and direct installation.

---

## Privacy and Data

* The SDK batches and transmits data efficiently
* You control what is sent — avoid secrets or sensitive PII
* Complies with store policies for iOS and Android.

For more details, see **[docs.appambit.com](https://docs.appambit.com)**.

---

## Troubleshooting

* **No data in dashboard** → verify API key, check network access, and ensure `AppAmbitSdk.Start()` is called once at startup.
* **NuGet restore issues** → run `dotnet restore` or clear local caches with `dotnet nuget locals all --clear`.
* **Crashes not appearing** → remember crashes are reported on the next app launch.

---

## Contributing

We welcome issues and pull requests.

* Fork the repo
* Create a feature branch
* Add tests where applicable
* Open a PR with a clear summary

Please follow .NET coding guidelines and document public APIs.

---

## Versioning

Semantic Versioning (`MAJOR.MINOR.PATCH`) is used.

* Breaking changes → **major**
* New features → **minor**
* Fixes → **patch**

---

## Security

If you find a security issue, please contact **[hello@appambit.com](mailto:hello@appambit.com)** rather than opening a public issue.

---

## License

Open source under the terms described in the [LICENSE](./LICENSE) file.

---

## Links

* **Docs**: [docs.appambit.com](https://docs.appambit.com)
* **Dashboard**: [appambit.com](https://appambit.com)
* **Discord**: [discord.gg](https://discord.gg/nJyetYue2s)
* **Examples**: Sample .NET MAUI demo (iOS and Android) included in repo.

---
