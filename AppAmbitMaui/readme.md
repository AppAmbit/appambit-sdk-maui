# AppAmbit: Getting Started with MAUI

This guide walks you through setting up the AppAmbit .NET SDK in your application, focusing on AppAmbit Analytics and Crash Reporting.

## 1. Prerequisites

Before getting started, ensure you meet the following requirements:

- The target devices run iOS 11.0 or higher, or Android 5.0 (API level 21) or higher.
- You are not using another SDK for crash reporting.

### Supported Platforms

- MAUI for iOS
- MAUI for Android
- MAUI for Windows (coming soon)
- MAUI for macOS (coming soon)

<div class="note">
  <strong>Note:</strong> Xamarin is currently not supported by the AppAmbit library.
</div>

## 2. Creating Your App in the AppAmbit Portal

1. Visit [AppAmbit.com](http://appambit.com/).
2. Sign in or create an account. Navigate to "Apps" and click on "New App".
3. Provide a name for your app.
4. Select the appropriate release type and target OS.
5. Click "Create" to generate your app.
6. Retrieve the App Key from the app details page.
7. Use this App Key as a parameter when calling `.UseAppAmbit("<YOUR-APPKEY>")` in your project.

## Adding the AppAmbit SDK to Your Solution

### [NuGet](https://www.nuget.org/packages/com.AppAmbit.Sdk)

Add the package to your MAUI project:

```bash
dotnet add package com.AppAmbit.Sdk
# or specify version
dotnet add package com.AppAmbit.Sdk --version 0.0.5
```

Or, using Visual Studio:

* Right-click your project → **Manage NuGet Packages…**
* Search for **AppAmbit** and install the latest stable version.

---

## Initializing the SDK

To begin using AppAmbit, you need to explicitly enable the services you wish to use. No services are activated by default.

### Import the Namespace

Add the required `using` directive to your file:

```csharp
using AppAmbit;
```

### Initialize AppAmbit

Call `.UseAppAmbit("<YOUR-APPKEY>")` during application initialization:

```csharp
.UseAppAmbit("<YOUR-APPKEY>");
```

Here's an example of how to configure it within your `MauiProgram` class:

```csharp
using AppAmbit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseAppAmbit("<YOUR-APPKEY>");
        
        return builder.Build();
    }
}
```

This code automatically generates a session, the session management is automatic.

#### Android Requirements

Add the following permissions in your `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
<uses-permission android:name="android.permission.INTERNET" />
```

#### iOS and macOS (coming soon) Requirements

For iOS, add the required URL exceptions in your `Info.plist` file:

```xml
<key>NSAppTransportSecurity</key>
<dict>
    <key>NSExceptionDomains</key>
    <dict>
        <key>appambit.com</key>
        <dict>
            <key>NSIncludesSubdomains</key>
            <true/>
            <key>NSThirdPartyExceptionAllowsInsecureHTTPLoads</key>
            <true/>
        </dict>
    </dict>
</dict>
```

## Crashes
**AppAmbit.Crashes** allows you to track errors by using handled exceptions. To do so, use the `TrackError` method:
```c-sharp
catch (Exception exception) 
{ 
    Crashes.TrackError(exception); 
}
```
An app can optionally attach properties to a handled error report to provide further context. Pass the properties as a dictionary of key/value pairs (strings only) as shown in the example below.
```c-sharp
catch (Exception exception) 
{ 
    var properties = new Dictionary<string, string> 
    { 
	{ "userId", "{user.Id}" }, 
	{ "isGuest", "true"} 
    }; 
    Crashes.TrackError(exception, properties);
}
```
AppAmbit will automatically generate a crash log every time your app crashes. The log is first written to the device's storage and when the user starts the app again, the crash report will be sent to AppAmbit. Collecting crashes works for both development, beta and production apps, i.e. those submitted through App Store Connect or Google Play Console. Crash logs contain valuable information for you to help fix the crash.


## Analytics


**AppAmbit.Analytics** helps you understand user behavior and customer engagement to improve your app. The SDK automatically captures session count and device properties like model, OS version, etc. You can define your own custom events to measure things that matter to you. All the information captured is available in the AppAmbit portal for you to analyze the data.

**Custom Events**
You can track your own custom events with custom properties  to understand the interaction between your users and the app. Once you've started the SDK, use the  `TrackEvent()`  method to track your events with properties.
```c-sharp
Analytics.TrackEvent("Order Placed", new Dictionary<string, string> { { "userId", "{user.Id}" }, { "orderId", "{order.Id}"} });
```
Properties for events are entirely optional – if you just want to track an event, use this sample instead:
```c-sharp
Analytics.TrackEvent("Order Placed");
```

## Offline Behavior

If the device is offline, the SDK will store sesssions, events, logs, and crashes locally. Once internet connectivity is restored, the SDK will automatically send the stored sesssions, events, logs, and crashes in batches.

## Network Connectivity Handling

- If the device transitions from offline to online, any pending requests are retried immediately.

## License

MIT License

Copyright (c) 2025 AppAmbit

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
