using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for a device specific OID.
    /// </summary>
    [Table("DeviceSpecificOids")]
    internal class DeviceSpecificOid
    {
        /// <summary>
        /// Gets the schema info's name (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Name { get; set; }

        /// <summary>
        /// Gets the ID that maps this entry to a list of DeviceVersions.
        /// </summary>
        [Column("OidMappingId")]
        public int OidMappingId { get; set; }

        /// <summary>
        /// Gets the ID of the meaning that this entry represents.
        /// </summary>
        [Column("ValueId")]
        public int RetrievableValueId { get; set; }

        /// <summary>
        /// Gets the retrievable value dataset that this entry belongs to.
        /// </summary>
        [ForeignKey("RetrievableValueId")]
        public RetrievableValue RetrievableValue { get; set; }

        /// <summary>
        /// Gets the ID of the data type that the value of this entry is of.
        /// </summary>
        [Column("DataType")]
        public int DataTypeId { get; set; }

        /// <summary>
        /// Gets the data tyüe that the value of this entry belongs to.
        /// </summary>
        [ForeignKey("DataTypeId")]
        public DataType DataType { get; set; }

        /// <summary>
        /// Gets the OID for retrieving tha data that this entry represents.
        /// </summary>
        [Column("OID")]
        public Oid Oid { get; set; }
    }
}
