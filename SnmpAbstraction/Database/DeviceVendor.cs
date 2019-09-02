using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the device vendors table
    /// </summary>
    [Table("DeviceVendors")]
    internal class DeviceVendor
    {
        /// <summary>
        /// Gets the ID (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the readable name of the vendor.
        /// </summary>
        [Column("Name")]
        public string VendorName { get; set; }

        /// <summary>
        /// Gets the list of devices that are made by this vendor.
        /// </summary>
        [InverseProperty("Vendor")]
        public List<Device> Devices { get; set; }
    }
}
