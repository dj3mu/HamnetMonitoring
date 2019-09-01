using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the devices table
    /// </summary>
    [Table("Devices")]
    public class Device
    {
        /// <summary>
        /// Gets the ID (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the relation to the device's vendor.
        /// </summary>
        [Column("VendorId")]
        public int VendorId { get; set; }

        /// <summary>
        /// Gets the readable name of the device's vendor
        /// </summary>
        [ForeignKey("VendorId")]
        public DeviceVendor Vendor { get; set; }
    }
}
