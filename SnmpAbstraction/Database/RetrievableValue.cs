using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnmpAbstraction
{
    /// <summary>
    /// Model for the values table
    /// </summary>
    [Table("Values")]
    internal class RetrievableValue
    {
        /// <summary>
        /// A protected default c'tor is needed for Entity Framework
        /// </summary>
        protected RetrievableValue() { }

        /// <summary>
        /// Prevent constructions for something other than the corresponding Enum.
        /// </summary>
        /// <param name="enum">The enum value to represent.</param>
        private RetrievableValue(RetrievableValuesEnum @enum)
        {
            this.Id = (int)@enum;
            this.ValueMeaning = @enum.ToString();
        }

        /// <summary>
        /// Gets the ID (unique key)
        /// </summary>
        [Column("ID")]
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets the readable meaning of the value.
        /// </summary>
        [Column("ValueMeaning")]
        public string ValueMeaning { get; set; }

        /// <summary>
        /// Implicit convertion from enum to model container.
        /// </summary>
        /// <param name="enum">The enum to convert.</param>
        public static implicit operator RetrievableValue(RetrievableValuesEnum @enum) => new RetrievableValue(@enum);

        /// <summary>
        /// Implicit conversion from model to enum.
        /// </summary>
        /// <param name="retrievalValue">The model to container to convert.</param>
        public static implicit operator RetrievableValuesEnum(RetrievableValue retrievalValue) => (RetrievableValuesEnum)retrievalValue.Id;
    }
}
