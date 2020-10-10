using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the device versions table
    /// </summary>
    [Table("DeviceVersions")]
    internal class DeviceVersion
    {
        /// <summary>
        /// Gets the ID (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the relation to the device.
        /// </summary>
        [Column("DeviceId")]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets the structured device data.
        /// </summary>
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }

        /// <summary>
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        [Column("MinSupportedSnmpVersion")]
        public int? MinimumSupportedSnmpVersion { get; set; }

        /// <summary>
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        [Column("MaxSupportedSnmpVersion")]
        public int MaximumSupportedSnmpVersion { get; set; }

        /// <summary>
        /// Gets the device version's minimum software version.
        /// </summary>
        [Column("DeviceHigherOrEqualSwVersion")]
        public SemanticVersion HigherOrEqualVersion { get; set; }

        /// <summary>
        /// Gets the device version's maximum software version or null if not applicable.
        /// </summary>
        [Column("DeviceLowerThanSwVersion")]
        public SemanticVersion LowerThanVersion { get; set; }

        /// <summary>
        /// Gets the handler class to use for this device (if null a hard-coded default handler will be used).
        /// </summary>
        [Column("HandlerClassName")]
        public string HandlerClassName { get; set; }
    }
}
