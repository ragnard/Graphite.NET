# Graphite.NET

A simple [Graphite](http://graphite.wikidot.com/) client library for .NET, 
including a WCF behavior for measuring duration of service operation calls.

A client for [StatsD](https://github.com/etsy/statsd/) is also included.

Install via NuGet:
```
Install-Package Graphite.NET
```

## Graphite

### Usage

```csharp
// Import namespace
using Graphite;

// ...

// Create an UDP client for sending metrics to "localhost:2003", prefixing all keys with "foo.bar"
using(var client = new GraphiteUdpClient("localhost", 2003, "foo.bar"))
{
    // Report a metric
    client.Send("baz", 93284928374);

    // Report a metric specifying timestamp
    client.Send("baz", 93284928374, DateTime.Now.AddSeconds(42));
}
```

## StatsD

### Usage

```csharp
// Import namespace
using Graphite.StatsD;

// ...

// Create a client for sending metrics to "localhost:8125", prefixing all keys with "foo.bar"
using(var client = new StatsDClient("localhost", 8125, "foo.bar"))
{
    // Increment a counter
    client.Incremenet("counter1"); // sends 'foo.bar.counter1:1|c'

    // Increment a counter by 42
    client.Incremenet("counter2", 42); // sends 'foo.bar.counter2:42|c'

    // Decrement a counter by 5, sampled every 1/10th time
    client.Decrement("counter3", 5, 0.1); // sends 'foo.bar.counter3:-5|c@0.1

    // Report that the blahonga operation took 42 ms
    client.Timing("blahonga", 42); // sends 'foo.bar.blahonga:42|ms'
}
```

## WCF endpoint behavior

Also included with Graphite.NET is a WCF endpoint behavior that measures 
and reports the duration of service operation invocations.

Currently only support for StatsD is implemented.

To use the behavior, first register it as an behavior extension, then use
it in suitable `endpointBehavior` elements.

The behavior will time all service operation invocations, sending metrics on
the form `KeyPrefix.ContractName.OperationName`.

Example configuration:

```xml
<system.serviceModel>
  <!-- Register behavior extension -->
    <extensions>
      <behaviorExtensions>
        <add name="timeOperations" type="Graphite.WCF.OperationTimingEndpointBehaviorExtensionElement, Graphite" />
      </behaviorExtensions>
    </extensions>
	<!-- Use the behavior where suitable -->
	<behaviors>
      <endpointBehaviors>
        <behavior name="...">
          <timeOperations hostname="..." port="..." keyPrefix="..." />
        </behavior>
      </endpointBehaviors>
    </behaviors>
</system.serviceModel>   
```

The `OperationTimingEndpointBehaviorExtensionElement` has three attributes:

* `hostname` (**required**): address to StatsD instance
* `port` (_optional_, default is 8125): the port StatsD is listening on
* `keyPrefix` (_optional_, default is none): namespace to prefix keys with
