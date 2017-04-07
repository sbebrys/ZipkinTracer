# ZipkinTracer
A .NET Core implementation of the Zipkin Tracer Client.

## Overview
This nuget package implements the zipkin tracer client for .net core applications.

**ZipkinTracer** : library is generating zipkin spans from trace headers or outgoing requests and sending it to the zipkin collector using HTTP as the transport protocol. For more information
and implementations in other languages, please check [Openzipkin](https://github.com/openzipkin/).

### Enable/Disable zipkin tracing

Zipkin will record traces if IsSampled HTTP header is true.  
This will happen if :
- **a)** the caller of the app has set the IsSampled HTTP header value to true.
- **OR**
- **b)** the url request is not in the `ExcludedPathList` of ZipkinConfig , and using the `SampleRate`, it will
determine whether or not to trace this request. `SampleRate` is the approximate percentage of traces being recorded in
zipkin.

## Configurations
Please use `ZipkinConfig` class to configure the module and verify these values and modify them according to your
service/environment.

- `Bypass` - Controls whether the requests should be sent through the Zipkin module
  - **false**: Enables the ZipkinMiddleware/ZipkinMessageHandler
  - **true**: Disables the ZipkinMiddleware/ZipkinMessageHandler
- `ZipkinBaseUri` - is the zipkin scribe/collector server URI with port to send the Spans
- `Domain` - is a valid public facing base url for your app instance. Zipkin will use to label the trace.
  - by default this looks at the incoming requests and uses the hostname from them. It's a `Func<HttpContext, Uri>` - customise this to your requirements.
- `SpanProcessorBatchSize` - how many Spans should be sent to the zipkin scribe/collector in one go.
- `SampleRate` - 1 decimal point float value between 0 and 1. This value will determine randomly if the current request will be traced or not.	 
- `NotToBeDisplayedDomainList`(optional) - It will be used when logging host name by excluding these strings in service name attribute
	e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as "abc" only    
- `ExcludedPathList`(optional) - Path list that is not needed for tracing. Each item must start with "/".
- `Create128BitTraceId` - Create new traces using 128 bit (32 hex character) traceId


```csharp
var config = new ZipkinConfig(new Uri("http://zipkin.xyz.net:9411"), request => new Uri("https://yourservice.com"))
{
	Bypass = request => request.GetUri().AbsolutePath.StartsWith("/allowed"),
	SpanProcessorBatchSize = 10,
	SampleRate = 0.5
}
```

To initialize tracer You must register `Zipkin Tracer` in DI container in Kestrel Startup class when call 'AddZipkinTracer(new ZipkinConfig(...){ ... })'

## Tracing

### Server trace (Inbound request)
Server Trace relies on Kestrel Middleware. Please create Kestrel Startup class then call `UseZipkinTracer()`.


```csharp
using ZipkinTracer.DependencyInjection;
using ZipkinTracer.Owin;

public class Startup
{
    public void Configuration(IApplicationBuilder app)
    {
         app.UseZipkinTracer();
    }
    public void ConfigureServices(IServiceCollection services)
    {
		var config = new ZipkinConfig(new Uri("http://zipkin.xyz.net:9411"), request => new Uri("https://yourservice.com"))
		{
			Bypass = request => request.GetUri().AbsolutePath.StartsWith("/allowed"),
			SpanProcessorBatchSize = 10,
			SampleRate = 0.5
		}
        app.AddZipkinTracer(config);
    }
}

```

### Client trace (Outbound request)
Client Trace relies on HttpMessageHandler for HttpClient. Please pass a ZipkinMessageHandler instance into HttpClient.

```csharp
using ZipkinTracer.Http;

public class HomeController : Controller
{
	private readonly IZipkinTracer _tracer;

	public HomeController(IZipkinTracer tracer)
	{
		_tracer = tracer;
	}

    public async Task<ActionResult> Index()
    {
        using (var httpClient = new HttpClient(new ZipkinMessageHandler(_tracer))))
        {
            var response = await httpClient.GetAsync("http://www.google.com");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }
        }
        return View();
    }
}
```

#### Troubleshooting

##### Logs

Logging internal to the library is provided via the [AspNetCore Logging abstraction](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging) which can use many third-party logging framework.

## Contributors
ZipkinTracer is based on Medidata.ZipkinTracerModule owned by (c) Medidata Solutions Worldwide and owned by its major contributors:
* Tomoko Kwan
* [Kenya Matsumoto](https://github.com/kenyamat)
* [Brent Villanueva](https://github.com/bvillanueva-mdsol)
* [Laszlo Schreck](https://github.com/lschreck-mdsol)
* [Jordi Carres](https://github.com/jcarres-mdsol)
* [Herry Kurniawan](https://github.com/hkurniawan-mdsol)
