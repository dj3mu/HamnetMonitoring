using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the generic database schema info.
    /// </summary>
    [Table("SchemaInfo")]
    internal class SchemaInfo
    {
        /// <summary>
        /// Gets the schema info's name (unique key)
        /// </summary>
        [Column("InfoName")]
        [Key]
        public int Name { get; set; }

        /// <summary>
        /// Gets the schema info's value.
        /// </summary>
        [Column("InfoValue")]
        public int Value { get; set; }
    }
}
