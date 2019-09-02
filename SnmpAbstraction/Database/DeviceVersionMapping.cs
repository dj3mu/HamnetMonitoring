using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the device version to oid lookup mapping table
    /// </summary>
    [Table("DeviceVersionMappings")]
    internal class DeviceVersionMapping
    {
        /// <summary>
        /// Gets the relation to the device version ID table.
        /// </summary>
        [Column("DeviceVersionId")]
        [Key]
        public int DeviceVersionId { get; set; }

        /// <summary>
        /// Gets the device version data structure.
        /// </summary>
        [ForeignKey("DeviceVersionId")]
        public DeviceVersion Vendor { get; set; }

        /// <summary>
        /// Gets the ID that references the device specific OID mapping table.
        /// </summary>
        [Column("OidMappingId")]
        public int OidMappingId { get; set; }

        /// <summary>
        /// Gets the list of Device Specific OIDs matching this lookup.
        /// </summary>
        /// <value></value>
        [ForeignKey("OidMappingId")]
        public List<DeviceSpecificOid> DeviceSpecificOid { get; set; }
    }
}
