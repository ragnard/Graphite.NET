# StatsD.NET

A simple [StatsD](https://github.com/etsy/statsd) client library for .NET, 
including a WCF behavior for measuring duration of service operation calls.

## Usage

```csharp
// Create a client for sending metrics to "localhost:8125", prefixing all keys with "foo.bar"
using(var client = new StatsClient("localhost", 8125, "foo.bar"))
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

Also included with StatsD.NET is a WCF endpoint behavior that measures 
and reports the duration of service operation invocations.

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
        <add name="timeOperations" type="StatsD.WCF.OperationTimingEndpointBehaviorExtensionElement, StatsD" />
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
