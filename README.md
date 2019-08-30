# `Hamnet` SNMP Query Tool

From past experience it was noticed that monitoring of `Hamnet` nodes is not straight forward. None of the Open Source available tools seems to support what is needed. Instead the generic approach of such tools offers a lot of features that are actually irrelevant for `Hamnet` use.

Additionally it turned out that a lot of traffic is caused by the monitoring tool querying plenty of values that are not actually important to *the* `Hamnet` itself. This is a problem is `Hamnet` links can be quite limited in bandwidth. So a requirement is to cause as few traffic for monitoring as possible.

For example: None of the tools supports detection and retrieval of the statistics of the two sides of a `Hamnet` RF link. Hard-coding them is possible but whenever the hardware changes, manual adaption of monitoring would be required. 

So we decided to create our own tool supporting only the really required features for `Hamnet` use while transmitting as few data as possible.

## Alpha State
The tool is still in alpha state. Actually, as of 2019-08-30, consider it a only little more than a P.o.c.

