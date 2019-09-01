using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the device versions table
    /// </summary>
    [Table("DeviceVersions")]
    public class DeviceVersion
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
        [ForeignKey("Device")]
        public int VendorId { get; set; }

        /// <summary>
        /// Gets the structured device data.
        /// </summary>
        public Device Device { get; set; }

        /// <summary>
        /// Gets the device version's minimum software version.
        /// </summary>
        [Column("DeviceMinSwVersion")]
        public string MinimumVersion { get; set; }

        /// <summary>
        /// Gets the device version's maximum software version or null if not applicable.
        /// </summary>
        [Column("DeviceMaxSwVersion")]
        public string MaximumVersion { get; set; }
    }
}
