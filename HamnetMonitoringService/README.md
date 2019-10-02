# `Hamnet` Monitoring Service
A background servic that polls the monitoring values at a defined interval and provides a REST API to query the data.

The service can be run by a simple [systemd unit file](https://www.digitalocean.com/community/tutorials/understanding-systemd-units-and-unit-files).

Configuration is done using an `appsettings.json` file for the service settings as well as a `log4net.config` for the logging settings.


## Configuration file settings
The service is configured using a file called `appsettings.json`. The settings are mostly self-explaining from the key name. Documentation will be added only later.

## REST API
There are two kinds of APIs provided:
* The [legacy API](LegacyApi.md) emulates the API of the previous monitoring tools.
* The [versioned API](VersionedApi.md) provides the data using versioned URLs and offers extended functionalities like real-time link testing.
