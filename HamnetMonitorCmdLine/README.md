# `Hamnet` Monitoring command line tool
A command line tool for manual execution of query operations.

### Commands
The tool supports the following sub-commands:
| Command    | Description                                                     |
|------------|-----------------------------------------------------------------|
| SystemData | Query for basic system data. Effectively that is almost only the data which is generically available and used to finally identify the device. However, the full device identification will also be executed. This command requires at least one host to be specified (`-h`).<br/>Note: The command does a full query for all available data. Lazy-loading is forced. |
| InterfaceData | Query for data of the local network interfaces of the device. This includes all kinds of interfaces. This command requires at least one host to be specified (`-h`).<br/>Note: The command does a full query for all available data. Lazy-loading is forced. |
| WirelessPeers | Query for data of the currently active, wireless peers of the device. This command requires at least one host to be specified (`-h`).<br/>Note: The command does a full query for all available data. Lazy-loading is forced. |
| LinkDetails | Query for detail data of the radio link between that two or more given devices. This command requires at least TWO hosts to be specified (`-h`).<br>This command is the main reason why the tool exists: It provides and algorithm to detect whether there's a radio link between the devices and extract its most important properties (e.g. RX Level).<br/>Note: Only the absolute necessary data for determining the link details are queried. Lazy-loading is active. |

### Global options
| Option     | Description                                                     |
|------------|-----------------------------------------------------------------|
| -h, --host | Specify one or more hosts to be queried. Sub-commands which don't require more than one host, will be executed individually for each specified host. Sub-commands that require more than one host will be executed once considering the whole host list. |
| -s, --snmpversion | Allows to specifiy the SNMP protocol version. Defaults to 2 (i.e. SNMP v 2c).<br/>Note: The device detection will always be done with version 1. Once detected, the protocol will fall back to the lowest of the version specified here or given in the device database for the detected device. So it's safe to specify higher protocols than supported by the device.<br/>Attention: SNMP v 3 has never been tested up to now. |
| --stats | Print SNMP PDU statistics at the end of the command execution. |


### Examples:
#### Query Link Details
```
> HamnetMonitorCmdLine  LinkDetails --stats -s 2 -h 44.143.111.34 44.143.111.38
Device 44.143.111.34:
  Link Details:
    Device 44.143.111.34:
      Link between side #1 (44.143.111.34) and side #2 (44.143.111.38):
      Side #1 MAC: E4:8D:8C:3D:F8:F5
      Side #2 MAC: 4C:5E:0C:83:F5:87
      Side of AP : 1
      Rx level of side #1 at side #2: -63.5 dBm
      Rx level of side #2 at side #1: -65.5 dBm
      Link Uptime: 5.07:26:32
      --> Query took 153.453 ms

  --> Query took 153.453 ms

Overall statistics:
===================
  Total Requests : 29
  Total Responses: 49
  Total Errors   : 0

Per request-type statistics:
============================

Get:
  Total Requests : 24
  Total Responses: 24
  Total Errors   : 0

GetBulk:
  Total Requests : 5
  Total Responses: 25
  Total Errors   : 0

Per device statistics:
======================

44.143.111.34:
  Total Requests : 15
  Total Responses: 27
  Total Errors   : 0

44.143.111.38:
  Total Requests : 14
  Total Responses: 22
  Total Errors   : 0
```


#### Query System Data
```
>HamnetMonitorCmdLine  SystemData -h 44.143.111.34 44.143.111.38
Device 44.143.111.34:
    - System Model                     : RB912UAG-5HPnD
    - System SW Version                : 6.44.1
    - System Name        (queried=True): OE5XGR link WKB
    - System location    (queried=True): JN68LD
    - System description (queried=True): RouterOS RB912UAG-5HPnD
    - System admin       (queried=True): oe5hpm@oevsv.at
    - System uptime      (queried=True): 66.23:11:58
    - System root OID    (queried=True): 1.3.6.1.4.1.14988.1
  --> Query took 162.7719 ms
Device 44.143.111.38:
    - System Model                     : RB911G-5HPnD
    - System SW Version                : 6.44.1
    - System Name        (queried=True): router.dm0wkb
    - System location    (queried=True):
    - System description (queried=True): RouterOS RB911G-5HPnD
    - System admin       (queried=True): dm5hr@darc.de
    - System uptime      (queried=True): 114.20:53:39
    - System root OID    (queried=True): 1.3.6.1.4.1.14988.1
  --> Query took 130.8001 ms
```


#### Query Interface data
```
>HamnetMonitorCmdLine  InterfaceData -h 44.143.111.34
Device 44.143.111.34:
  Interface Details:
    Device 44.143.111.34:
      Interface #1 (ether1):
        - Type: EthernetCsmacd
        - MAC : E4:8D:8C:3D:F8:F4
      --> Query took 69.9352 ms

    Device 44.143.111.34:
      Interface #2 (wlan1):
        - Type: Ieee80211
        - MAC : E4:8D:8C:3D:F8:F5
      --> Query took 48.9571 ms

    Device 44.143.111.34:
      Interface #6 (wds-dm0wkb):
        - Type: Other
        - MAC : E4:8D:8C:3D:F8:F5
      --> Query took 41.939 ms

    Device 44.143.111.34:
      Interface #8 (br-transfer-wkb):
        - Type: Bridge
        - MAC : E4:8D:8C:3D:F8:F5
      --> Query took 46.1594 ms

    Device 44.143.111.34:
      Interface #9 (vlan20-transfer-wkb):
        - Type: L2vlan
        - MAC : E4:8D:8C:3D:F8:F4
      --> Query took 47.9297 ms

  --> Query took 325.715 ms
  ```


#### Query Wireless Peers
  ```
>HamnetMonitorCmdLine  WirelessPeers -h 44.143.111.34
Device 44.143.111.34:
  Peer Infos:
    Device 44.143.111.34:
      Peer 4C:5E:0C:83:F5:87:
        - Mode           : AP
        - On interface ID: 2
        - Link Uptime    : 5.07:30:31
        - RX signal [dBm]: -65.2 dBm
        - TX signal [dBm]: -64.0 dBm
      --> Query took 91.7855 ms

  --> Query took 33.8324 ms
```

## Building and installing
To build you need the _DotNet Core SDK_ installed. Currently the tool is developed using DotNet Core v 2.2.

To build simply run:
```
> dotnet restore
> dotnet build
```
from the repository root folder.

To publish as a stand-alone application run:
```
> dotnet restore
> dotnet publish -c Release -r <linux-x64|win10-x64>
```
Use the `-r` arguments according to the target platform. This will produce a subfolder
```
HamnetMonitorCmdLine/bin/Release/netcoreapp2.2/<linux-x64|win10-x64>/publish
```
which contains the executable as well as all dependencies.

To install, simply copy the `publish` folder with all its content and subfolders to the target system.

The tool creates a log file the the current folder using [log4net](https://logging.apache.org/log4net/).
