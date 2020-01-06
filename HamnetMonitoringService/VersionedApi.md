# Versioned API
Adressed via URI root path `/api/v<version>`.  
`<version>` starts at 1 and every incompatible change (e.g. removal or renaming of fields) will result in an increment of the version number.


## Server Status and API version
URI path: `/api/status[?Referesh=<refreshIntervalSeconds>]`

This request is an exception to the versioned API and it is guaranteed, that this request produces backward-compatible results, forever. Use this request to determine the API version to use for talking to the server.

For manual use you may append a query parameter `Refresh` giving a refresh-interval in seconds at which the page will be reloaded.

Returns the server version information, uptime, highest supported API version and, in future, probably some more server status information. Example:

```json
{
    "serverVersion": "0.1.62-beta+a9a3d6b411",
    "processUptime": "00:00:03.3108184",
    "maximumSupportedApiVersion": 1,
    "databaseStatistic": {
        "ResultDatabase": {
            "UniqueRssiValues": "834",
            "TotalFailures": "99",
            "TimeoutFailures": "91",
            "NonTimeoutFailures": "8",
            "LastAquisitionStart": "30.09.2019 14:59:11",
            "LastAquisitionEnd": "30.09.2019 14:51:38",
            "LastMaintenanceStart": "30.09.2019 21:34:03",
            "LastMaintenanceEnd": "01.01.0001 00:00:00"
        },
...
        "CacheDatabase": {
            "UniqueCacheEntries": "957"
        },
        "DeviceDatabase": {
            "NumberOfUniqueDevices": "78",
            "NumberOfUniqueDevicesVersions": "86"
        }
    }
}
```


## API Version 1
All URI paths of this version are starting with `/api/v1`.

### RSSI query
URI root path: `/api/v1/rssi`.

Returns the RX RSSI values for all monitored IP addresses in the same format as the [legacy API](LegacyApi.md).  
Please note that for point-to-multipoint links the same host is supposed to hold multiple IP addresses of different transfer subnets on the same Wifi interface (IP aliasing). P2MP without IP aliasing into different transfer networks is **not** supported.

```json
[
    {
        "foreignid": "44.137.62.130",
        "metricid": 1,
        "metric": "RSSI",
        "unixtimestamp": 1569739244,
        "stamp": "2019-09-29T06:40:44+00:00",
        "value": "-61.5"
    },
    ...
 ]
```

#### List of RSSI query failures
URI path: `/api/v1/rssi/failing[/timeout|nontimeout]`

The last fragment `timeout` or `nontimeout` performs filtering for only such errors appearing due or not due to commuication timeouts. If omitted, all errors will be reported without filtering.

**Note:** With the current implementation, the error list will be completely deleted when a new query process starts. So when querying the failurs the list might be empty even though failurs will soon come up again. Actually this call is not meant for machine processing but mainly for human debugging.

```json
[
    {
        "Subnet": "44.143.28.184/29",
        "TimeStamp": "2019-09-29T08:36:35.0175649",
        "ErrorInfo": "SnmpAbstraction.HamnetSnmpException: Side #2 (SnmpAbstraction.CachingHamnetQuerier) seems to have no peerings with side #1 (SnmpAbstraction.CachingHamnetQuerier)"
    },
    {
        "Subnet": "44.143.111.0/29",
        "TimeStamp": "2019-09-29T08:36:42.893718",
        "ErrorInfo": "SnmpAbstraction.HamnetSnmpException: Timeout talking to device '44.143.111.1' during applicability check\nCollected Errors:\nSnmpAbstraction.MikrotikDetectableDevice: SnmpException talking to device '44.143.111.1' during applicability check: Request has reached maximum retries."
    },
    ...
]
```

### Immediate node and link testing
URI root path: `/api/v1/linktest`.

All replies of this API will at least return a JSON structure with the element `errorDetails` which is an array of strings containing error information. Usually this contains text of exceptions encountered when performing the operation. Example:
```json
{
    "errorDetails": [
        "SnmpException: Request has reached maximum retries."
    ]
}
```

**If the array is empty there was no error and the remaining data shall be assumed fully valid.**


#### Ping
URI path: `/api/v1/linktest/ping/<hostname or IP>`.

Tries to ping the given `<hostname or IP>` and returns the results. For example, the request URI path `/api/v1/linktest/ping/44.0.0.1` could return:
```json
{
    "address": "44.0.0.1",
    "roundtripTime": "00:00:00.2670000",
    "timeToLive": 53,
    "dontFragment": false,
    "bufferSize": 32,
    "status": "Success",
    "errorDetails": []
}
```

#### Host Info (or test for working SNMP)
URI path: `/api/v1/linktest/info/<hostname or IP>`.

Gets the basic system information for the given `<hostname or IP>` and returns the results. For example, the request URI path `/api/v1/linktest/info/44.224.10.78?EnableCaching=false` could return:
```json
{
    "description": "RouterOS RB750Pr2",
    "contact": "DG1MHM, DJ3MU",
    "location": "Aussichtsturm Ebersberg",
    "name": "router.db0ebe",
    "uptime": "295.19:38:15",
    "model": "RB750Pr2",
    "version": "6.43.8",
    "maximumSnmpVersion": "Ver2",
    "address": "44.224.10.78",
    "errorDetails": []
}
```

**Note:** If you want to make sure that the device is actually responding to SNMP requests, make sure to append the `?EnableCaching=false` or else you might get data from cache only without triggering any real SNMP request. You can detect a cache-only result when field `uptime` gets reported as `null` (JSON _null_, not zero !).


#### Link Test
URI path: `/api/v1/linktest/link/<host or IP #1>/<host or IP #2>?<options>`

Tries to obtain the full link details of a radio link between the two given IP addresses or host names.

For example the request URI path `/api/v1/linktest/link/44.137.69.173/44.137.69.170&EnableCaching=false` could return

```json
{
    "details": [
        {
            "macString1": "B8:69:F4:95:10:82",
            "macString2": "B8:69:F4:95:11:AA",
            "address1": "44.137.69.173",
            "address2": "44.137.69.170",
            "modelAndVersion1": "RBLHGG-60ad v 6.44.0",
            "modelAndVersion2": "RBLHGG-60ad v 6.44.0",
            "rxLevel1at2": -68.0,
            "rxLevel2at1": -70.0,
            "linkUptime": "00:00:00",
            "sideOfAccessPoint": 1
        }
    ],
    "errorDetails": []
}
```

#### HamnetDB-based link test
URI path: `/api/v1/linktest/network/<network>?<options>`

Queries the HamnetDB for the radio nodes with active _monitoring_ flag inside the given `<network>`. The `<network>` can be CIDR notation or IP/netmask where forward slash must, obviously, be URL-escaped.

For example the request URI path `/api/v1/linktest/network/44.224.10.64%2F29` could return

```json
{
    "details": [
        {
            "macString1": "64:D1:54:7C:D6:E5",
            "macString2": "64:D1:54:5F:7B:B6",
            "address1": "44.224.10.66",
            "address2": "44.224.10.69",
            "modelAndVersion1": "RBLHG5HPnD v 6.44.1",
            "modelAndVersion2": "RBDynaDishG-5HacDr3 v 6.43.8",
            "rxLevel1at2": -80.544595368907054,
            "rxLevel2at1": -71.806689519339045,
            "linkUptime": "00:16:34",
            "sideOfAccessPoint": 2
        }
    ],
    "errorDetails": []
}
```


### Network tools functionality
URI root path: `/api/v1/tools`.

All replies of this API will at least return a JSON structure with the element `errorDetails` which is an array of strings containing error information. Usually this contains text of exceptions encountered when performing the operation. Example:
```json
{
    "errorDetails": [
        "TikCommandException: invalid user name or password (6)"
    ]
}
```

#### Supported Features
URI path: `/api/v1/tools/hostsSupportingFeature/<comma-separated-feature-list>`

Requests a list of hosts that are **currently** known to support **all of** the given features.

Supported features currently are:

| Feature    |  Description                                                                                           |
|------------|--------------------------------------------------------------------------------------------------------|
| Rssi       | Support for querying link RSSI values. Includes capability to provide interface and wireless peer list |
| BgpPeers   | Support for querying BGP peers.                                                                        |
| Traceroute | Support for network test operations like `traceroute` and `ping`                                       |

**Important:** This request uses data of the device cache. Hence it only returns **known** hosts. This is, hosts that have already been queried.
If you need to know if a very specific hosts supports a specific feature, please use the `/api/v1/linktest/info/<hostNameOrIp>` request and evaluate the
`supportedFeatures` response element.

Example result:
```json
[
    {
        "description": "RouterOS RB750Pr2",
        "contact": "",
        "location": "",
        "name": "router.db0zm",
        "uptime": null,
        "model": "RB750Pr2",
        "version": "6.45.1",
        "maximumSnmpVersion": "Ver1",
        "address": "44.225.20.193",
        "errorDetails": [],
        "supportedFeatures": [
            "Rssi",
            "BgpPeers",
            "Traceroute"
        ],
        "defaultApi": "VendorSpecific"
    },
    {
        "description": "RouterOS RB750Pr2",
        "contact": "",
        "location": "",
        "name": "router.db0ebe",
        "uptime": null,
        "model": "RB750Pr2",
        "version": "6.46.0",
        "maximumSnmpVersion": "Ver1",
        "address": "44.225.21.1",
        "errorDetails": [],
        "supportedFeatures": [
            "Rssi",
            "BgpPeers",
            "Traceroute"
        ],
        "defaultApi": "VendorSpecific"
    },
...
]
```


#### Traceroute
URI path: `/api/v1/tools/traceroute/<start host or IP>/<destination IP>?<options>`

or

URI path: `/api/v1/tools/traceroute/<start host or IP>/<destination IP>/<count>?<options>`

or

URI path: `/api/v1/tools/traceroute/<start host or IP>/<destination IP>/<count>/<timeout-in-seconds>?<options>`

or

URI path: `/api/v1/tools/traceroute/<start host or IP>/<destination IP>/<count>/<timeout-in-seconds>/<max-hop-count>?<options>`

Performs a traceroute operation from `start host or IP` to `destination IP`.

Optionally a `count` can be added to specify how many packets shall be sent for the traceroute operation. If not specified, `count` defaults to 1. Minimum is 1, maximum 100.

Optionally a `timeout-in-seconds` can be added after `count`. Unit is seconds and, if not specified, defaults to 1.0 seconds. Minimum is 0.1 seconds, maximum is 60.0 seconds.

Optionally a `max-hop-count` can be added after `count` and `timeout-in-seconds`. This allows the set the maximum number of hops that a packet will survive. If not specified, defaults to 128. Minimum is 10, maximum is 255.

Example result:
```json
 {
    "errorDetails": [],
    "fromAddress": "44.225.21.1",
    "toAddress": "44.224.90.86",
    "hopCount": 20,
    "hops": [
        {
            "address": "44.224.10.73",
            "lossPercent": 0,
            "sentCount": 1,
            "status": "ok",
            "lastRttMs": 1.1,
            "averageRttMs": 1.1,
            "bestRttMs": 1.1,
            "worstRttMs": 1.1
        },
        {
            "address": "44.224.10.105",
            "lossPercent": 0,
            "sentCount": 1,
            "status": "ok",
            "lastRttMs": 5.4,
            "averageRttMs": 5.4,
            "bestRttMs": 5.4,
            "worstRttMs": 5.4
        },
...
]
```

If the device at `start host or IP` does not support Traceroute, there will be an error response like:
```json
{
    "errorDetails": [
        "HamnetSnmpException: Unsupported device at address '44.224.90.86': No applicable handler found (allowed APIs: VendorSpecific)"
    ]
}
```

### BGP information
URI root path: `/api/v1/bgp`.

All replies of this API will at least return a JSON structure with the element `errorDetails` which is an array of strings containing error information. Usually this contains text of exceptions encountered when performing the operation. Example:
```json
{
    "errorDetails": [
        "TikCommandException: invalid user name or password (6)"
    ]
}
```

#### Get BGP peers
There are two ways to request details of a BGP peering. Either all peer's details of the queried host can be retrieved using the

URI path: `/api/v1/bgp/peers/<host or IP>?<options>`

Alternatively a single peer's details can be obtained causing a little less traffic by using

URI path: `/api/v1/bgp/peers/<host or IP>/<peer IP>?<options>`

**Please note:** While the first parameter can be a host name or an IP address, the second parameter _must_ be an IP address (no name resolution is supported).

A response could look like:
```json
{
    "bgpPeers": [
        {
            "localAddress": "44.224.10.78",
            "peeringName": "DB0ZM",
            "remoteAddress": "44.224.10.73",
            "uptime": "62.13:54:46",
            "prefixCount": 2138,
            "peeringState": "established"
        },
        {
            "localAddress": "44.224.10.70",
            "peeringName": "DB0AAT",
            "remoteAddress": "44.224.10.65",
            "uptime": "13:28:49",
            "prefixCount": 1590,
            "peeringState": "established"
        },
        {
            "localAddress": "44.224.10.217",
            "peeringName": "DB0ON",
            "remoteAddress": "44.224.10.222",
            "uptime": "54.19:49:33",
            "prefixCount": 164,
            "peeringState": "established"
        }
    ],
    "errorDetails": []
}
```

### BGP all monitored (according to HamnetDB) query
URI path: `/api/v1/bgp/monitoredRouters`.

Returns the BGP peers for all monitored routers. That is routers which have the "monitor BGP peers" flag set.  

```json
{
    "bgpPeers": [
        {
            "remoteAddress": "44.224.10.73",
            "peeringName": "DB0ZM",
            "localAddress": "44.224.10.78",
            "uptime": "62.19:30:34",
            "prefixCount": 2139,
            "peeringState": "established"
        },
        {
            "remoteAddress": "44.224.10.222",
            "peeringName": "DB0ON",
            "localAddress": "44.224.10.217",
            "uptime": "55.01:25:21",
            "prefixCount": 162,
            "peeringState": "established"
        }
    ],
    "errorDetails": []
}
```

#### List of BGP query failures
URI path: `/api/v1/bgp/monitoredRouters/failing[/timeout|nontimeout]`

The last fragment `timeout` or `nontimeout` performs filtering for only such errors appearing due or not due to commuication timeouts. If omitted, all errors will be reported without filtering.

**Note:** With the current implementation, the error list will be completely deleted when a new query process starts. So when querying the failurs the list might be empty even though failurs will soon come up again. Actually this call is not meant for machine processing but mainly for human debugging.


### Cache Info
URI root path: `/api/v1/cacheInfo`.

Returns the entire cache content in JSON format. Expensive call and not too much useful for regular use. Mainly intended for debugging caching issues.


## Supported querier options
For the `<options>` URL query string the following values are supported to configure the behaviour of the querier:

| Option name | Default | Description                                                          |
|-------------|---------|----------------------------------------------------------------------|
| Port        | 161     | The UDP port number to use for the SNMP requests.                    |
| ProtocolVersion | 1       | The SNMP version to use. Supported values: 0 -> SNMPv1, 1 -> SNMPv2c |
| Community   | public  | The SNMP community string                                            |
| Timeout     | 0:0:30  | The timeout per SNMP request packet. After this amount if time without reply, a retry will be done.
| Retries     | 1       | The number of retries to send the SNMP request (e.g. in case of timeout) |
| Ver2cMaximumValuesPerRequest | 0 | The maximum number of values per SNMPv2c request. Ignored in case of SNMPv1 |
| Ver2cMaximumRequests | 5 | The maximum number of SNMPv2c requests. Ignored in case of SNMPv1 |
| EnableCaching | true | If **true**, the cache database will be used to reduce network traffic (if details of the device are already available in cache). If **false** all required data will be re-queried from the devices including identification of the device and SW version. |
| LoginUser | "" | A user name to use when login / authentication is required to access a specific set of data. |
| LoginPassword | "" | A password to use when login / authentication is required to access a specific set of data. |
| AllowedApis | VendorSpecific,Snmp | The APIs to allow for talking to the device. Defaults to both, vendor-specific and SNMP. If available, vendor-specific is preferred. |
| QuerierClassHint | "" | For development purposes only. The name of the class implementing the querier for this device. |