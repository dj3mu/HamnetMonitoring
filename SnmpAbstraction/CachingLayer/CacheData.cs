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
    internal class CacheData
    {
        /// <summary>
        /// Gets the device address that this entry is for.
        /// </summary>
        [Key, Column("DeviceAddress")]
        public IpAddress Address { get; set; }

        /// <summary>
        /// Gets device system data.
        /// </summary>
        [Column("SystemData")]
        public IDeviceSystemData SystemData { get; set; }

        /// <summary>
        /// Gets the bytes of the serialized wireless peer info data.
        /// </summary>
        [Column("WirelessPeerInfo")]
        public IWirelessPeerInfos WirelessPeerInfos { get; set; }

        /// <summary>
        /// Gets the bytes of the serialized interface data.
        /// </summary>
        [Column("InterfaceDetails")]
        public IInterfaceDetails InterfaceDetails { get; set; }

        /// <summary>
        /// Gets the date and time of last modification of this table row.
        /// </summary>
        [Column("LastModification")]
        public DateTime LastModification { get; set; }
    }
}
