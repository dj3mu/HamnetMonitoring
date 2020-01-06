# `Hamnet` SNMP Monitoring Tools

**This project is the data collecting backend for the [Hamnet Database](https://hamnetdb.net).**

**The [Hamnet Database](https://hamnetdb.net) is accessing the tool via the REST API of the [background service](HamnetMonitoringService/README.md). It's using the tool for both, getting regularly collected and stored values (e.g. RSSI) but also for interactive (i.e. on user click) commands like link tests.**

**The data collected by the tool is also stored in an [Influx Database](https://www.influxdata.com/) for which a [Grafana visualization](https://grafana.com/) is [publically available in the Hamcloud](https://grafana.hamnetdb.net)**

`Hamnet` is the Internet of the Radio Amateurs [(German Wikipedia)](https://de.wikipedia.org/wiki/HAMNET) based on radio links on licensed (unattended automatic Amateur Radio station) or ISM (standard WLAN frequencies).

From past experience it was noticed that monitoring of `Hamnet` nodes is not as straight forward as it is for regular Internet or LAN. None of the tools available did support what is actually needed. Instead the generic approach of such tools offered a lot of features that are actually irrelevant for `Hamnet` use.

Additionally it turned out that a lot of traffic can be caused by the monitoring tools, querying plenty of values that are not actually important to *the* `Hamnet` itself. This is a problem as `Hamnet` links can be quite limited in bandwidth. So a requirement is to cause as few traffic for monitoring as possible.  
Another requirement is to be able to auto-provision the monitoring from the central [Hamnet Database](https://hamnetdb.net).

For example: None of the tools supported detection and retrieval of the statistics of the two sides of a `Hamnet` RF link. Hard-coding them was possible but whenever the topology or the hardware changes, (mostly manual) adjustment of monitoring was required. 

So we decided to create our own tool supporting only the really required features for `Hamnet` monitoring use while exchanging as few data as possible with the involved nodes.


## Tools
There are independent tool components. Each of which can exists fully on its own without the need for another tool to be present.

### HamnetMonitoringService background service
There's a [background service](HamnetMonitoringService/README.md) for continuous monitoring including auto-provisioning from [Hamnet Database](https://hamnetdb.net).

### HamnetMonitoringCmdLine command line interface
_Deprecated_

There used to be a [command line tool](HamnetMonitorCmdLine/README.md) for manual execution of query operations.  
It's not maintained any more in favor of manually using the [background service](HamnetMonitoringService/README.md) REST API from a Web Browser.

## Libraries
A couple a libraries support the above tools:

### SnmpAbstraction library
This library abstract communication to the devices. The entry-point into the library is the `IHamnetQuerier` interface which can be obtained using the `SnmpQuerierFactory` like

```csharp
var querier = SnmpQuerierFactory.Instance.Create(
    "44.x.y.z",
    QuerierOptions.Default
        .WithProtocolVersion(SnmpVersion.Ver2)
        .WithCaching(false)
        .WithAllowedApis(QueryApis.VendorSpecific | QueryApis.Snmp));
```

### HamnetDbAbstraction library
This library centralizes access to the [Hamnet Database](https://hamnetdb.net).

It's accessed via `IHamnetDbAccess` an instance of which is obtained using `HamnetDbProvider` passing an instance of an `Microsoft.Extensions.Configuration.IConfigurationSection`:

```csharp
var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbIConfigurationSection))
```

Example configuration sections can be found in [HamnetMonitoringService appsettings examples](https://github.com/dj3mu/HamnetMonitoring/blob/develop/HamnetMonitoringService/appsettings-Release.json).


## Device Database schema
The device database is a SQLite database containing the device- and even device-version
specific mappings of "Retrievable Values" to SNMP OIDs.

As of now there's no schema description available yet. You may use any kind of SQLite
browser tool (e.g. [DB Browser for SQLite](https://sqlitebrowser.org/)) to look into the database and change values.  
Final plan is to have a (command line?) interface to create or remove entries for specific devices.

However, such tool is not yet available.  
I apologize!
