using System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a combination of a (collected) exception and an additional info text.
    /// </summary>
    internal class InfoAndException : IInfoAndException
        {
            /// <inheritdoc />
            public string Info { get; set; }

            /// <inheritdoc />
            public Exception Exception { get; set; }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"{this.Info}: {this.Exception?.GetType().Name}: {this.Exception?.Message}";
            }
        }
}
