using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the database data types table.
    /// </summary>
    [Table("DataTypes")]
    internal class DataType
    {
        /// <summary>
        /// A protected default c'tor is needed for Entity Framework
        /// </summary>
        protected DataType() { }

        /// <summary>
        /// Prevent constructions for something other than the corresponding Enum.
        /// </summary>
        /// <param name="enum">The enum value to represent.</param>
        private DataType(DataTypesEnum @enum)
        {
            this.Id = (int)@enum;
            this.TypeName = @enum.ToString();
        }

        /// <summary>
        /// Gets the ID (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the readable name of the type.
        /// </summary>
        [Column("TypeName")]
        public string TypeName { get; set; }

        /// <summary>
        /// Implicit convertion from enum to model container.
        /// </summary>
        /// <param name="enum">The enum to convert.</param>
        public static implicit operator DataType(DataTypesEnum @enum) => new DataType(@enum);

        /// <summary>
        /// Implicit conversion from model to enum.
        /// </summary>
        /// <param name="retrievalValue">The model to container to convert.</param>
        public static implicit operator DataTypesEnum(DataType retrievalValue) => (DataTypesEnum)retrievalValue.Id;
    }
}
