# Legacy Compatibility API
Adressed via URI root path `/vw_rest_rssi`.

## RSSI query
Using directly the root path, the API returns the RX RSSI values for all monitored IP addresses. Please note that for point-to-multipoint links the same host is supposed to hold multiple IP addresses of different transfer subnets on the same Wifi interface (IP aliasing). P2MP without IP aliasing into different transfer networks is **not** supported.

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

## List of value query failurs
URI path: `/vw_rest_rssi/failing[/timeout|nontimeout]`

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
