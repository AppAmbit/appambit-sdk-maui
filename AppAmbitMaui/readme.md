# AppAmbit

AppAmbit allows you to monitor real-world usage of your iOS and Android devices with crash and analytics data all in one place.

## How to use SDK

1. Register AppAmbit account and team at **appambit.com**.
2. Create App in apps section and copy **App Key**.
3. Install the **AppAmbit NuGet package** on your .NET MAUI application.
4. Add **UserAppAmbit** to your builder chain and pass paste your **App Key** into parameters:

```
// Add the using to the top
using AppAmbit;

public static MauiApp Create()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()  
        .UseAppAmbit("your_app_key"); // Make sure to add this line and pass your app key
    
    return builder.Build();
}
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

## No internet access

When there isn't any network connectivity, the SDK saves information in the local storage. Once the device gets internet access back, the SDK will send data to the AppAmbit portal.

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