# `Hamnet` SNMP Monitoring Tools

The `Hamnet` is the Internet of the Radio Amateurs [(German Wikipedia)](https://de.wikipedia.org/wiki/HAMNET) based on radio links on licensed (unattended automatic Amateur Radio station) or ISM (standard WLAN frequencies).

From past experience it was noticed that monitoring of `Hamnet` nodes is not as straight forward as it is for regular Internet or LAN. None of the tools available seems to support what is needed. Instead the generic approach of such tools offers a lot of features that are actually irrelevant for `Hamnet` use.

Additionally it turned out that a lot of traffic can be caused by the monitoring tools, querying plenty of values that are not actually important to *the* `Hamnet` itself. This is a problem as `Hamnet` links can be quite limited in bandwidth. So a requirement is to cause as few traffic for monitoring as possible.  
Another requirement is to be able to auto-provision the monitoring from the central [Hamnet Database](https://hamnetdb.net).

For example: None of the tools supports detection and retrieval of the statistics of the two sides of a `Hamnet` RF link. Hard-coding them is possible but whenever the hardware changes, (mostly manual) adaption of monitoring would be required. 

So we decided to create our own tool supporting only the really required features for `Hamnet` monitoring use while exchanging as few data as possible with the involved nodes.

**This project is the data collecting backend for the [Hamnet Database](https://hamnetdb.net). The [Hamnet Database](https://hamnetdb.net) is accessing the tool via the REST API of the [background service](HamnetMonitoringService/README.md). It's using the tool for both, getting regularly collected and stored values (e.g. RSSI) but also for interactive (i.e. on user click) commands like link tests.**


## Beta State
The tool is still in beta state. Actually, as of 2019-09-26, it is still under development.

## Components
There are independent tool components. Each of which can exists fully on its own without the need for another tool to be present.

### Command Line
There's a [command line tool](HamnetMonitorCmdLine/README.md) for manual execution of query operations.

### Background Service
There's a [background service](HamnetMonitoringService/README.md) for continuous monitoring including auto-provisioning from [Hamnet Database](https://hamnetdb.net).


## Device Database schema
The device database is a SQLite database containing the device- and even device-version
specific mappings of "Retrievable Values" to SNMP OIDs.

As of now there's no schema description available yet. You may use any kind of SQLite
browser tool to look into the database and change values.  
Final plan is to have a command line interface to create or remove entries for specific devices. However, such tool is not yet available.  
I apologize!
