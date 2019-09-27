# `Hamnet` SNMP Monitoring Tools

The `Hamnet` is the Internet of the Radio Amateurs [(German Wikipedia)](https://de.wikipedia.org/wiki/HAMNET) based solely on radio links on licensed (unattended automatic Amateur Radio station) or ISM (standard WLAN frequencies).

From past experience it was noticed that monitoring of `Hamnet` nodes is not straight forward. None of the tools available seems to support what is needed. Instead the generic approach of such tools offers a lot of features that are actually irrelevant for `Hamnet` use.

Additionally it turned out that a lot of traffic can be caused by the monitoring tools, querying plenty of values that are not actually important to *the* `Hamnet` itself. This is a problem as `Hamnet` links can be quite limited in bandwidth. So a requirement is to cause as few traffic for monitoring as possible.  
Another requirement is to be able to auto-provision the monitoring from the central [Hamnet Database](https://hamnetdb.net).

For example: None of the tools supports detection and retrieval of the statistics of the two sides of a `Hamnet` RF link. Hard-coding them is possible but whenever the hardware changes, (mostly manual) adaption of monitoring would be required. 

So we decided to create our own tool supporting only the really required features for `Hamnet` monitoring use while exchanging as few data as possible with the involved nodes.


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


## For developers
The project has been started by DJ3MU.  
It's written in C# as Dotnet Core application. It's using quite a couple of useful Nuget package for which I want to take the change to say "Thank you" to the package developers.

The code does not in any way claim to be perfect. I've tried to obey software design concepts as much as I could. But I'm really open to all kinds of improvement requests. Feel free to contact me or submit pull requests and I'll do my best to improve.

Everybody is encouraged to actively use the tool and report back bugs and/or new requirements.

### Building and deploying
From the root folder of repository run
```shell
dotnet build
```
To publish to a [self-contained, frame-work-dependent executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#framework-dependent-executables-fde) (i.e. an executable with all required DLLs inside same folder) use
```shell
dotnet publish -c Release -r win10-x64
```
for or
```shell
dotnet publish -c Release -r linux-x64
```
For more platforms see the dotnet core [runtime identifier catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).


## Configuration Management
Even though I've started on a single master branch, the tool is now using a [GitFlow](https://datasift.github.io/gitflow/IntroducingGitFlow.html) development model.

Continuous integration is still to be set up.

There are a couple of Unit Tests available. But they leave a lot of space for improvements.

Even a release concept is still to be implemented. There's currently no packaging of the self-contained publish result for Linux (RPM / Debian packages) or Windows (msi or exe).

Pull requests are appreciated.
