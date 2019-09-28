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
        /// Gets a comma-separated list of IDs, in order of precendence (first ID will be tried first)
        /// that references the device specific OID mapping tables.
        /// </summary>
        [Column("OidMappingIds")]
        public string OidMappingIds { get; set; }

        /// <summary>
        /// Gets the list of Device Specific OIDs matching this lookup.
        /// </summary>
        /// <value></value>
        [ForeignKey("OidMappingId")]
        public List<DeviceSpecificOid> DeviceSpecificOid { get; set; }
    }
}
