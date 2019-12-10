using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SnmpSharpNet;

namespace SnmpAbstraction
{

    /// <summary>
    /// Model for the database data types table.
    /// </summary>
    [Table("CacheData")]
    internal class CacheData : ICacheData
    {
        /// <inheritdoc />
        [Key, Column("DeviceAddress")]
        public IpAddress Address { get; set; }

        /// <inheritdoc />
        [Column("SystemData")]
        public IDeviceSystemData SystemData { get; set; }

        /// <inheritdoc />
        [Column("WirelessPeerInfo")]
        public IWirelessPeerInfos WirelessPeerInfos { get; set; }

        /// <inheritdoc />
        [Column("InterfaceDetails")]
        public IInterfaceDetails InterfaceDetails { get; set; }

        /// <inheritdoc />
        [Column("LastModification")]
        public DateTime LastModification { get; set; }

        /// <inheritdoc />
        [Column("ApiUsed")]
        public QueryApis ApiUsed { get; set; }
        
        /// <inheritdoc />
        [Column("DeviceHandlerClass")]
        public string DeviceHandlerClass { get; set; }
    }
}
