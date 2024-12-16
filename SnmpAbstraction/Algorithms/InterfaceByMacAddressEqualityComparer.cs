using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Equality comparer that compares <see cref="IInterfaceDetail" /> by MAC address.
    /// </summary>
    internal class InterfaceByMacAddressEqualityComparer : IEqualityComparer<IInterfaceDetail>
    {
        /// <inheritdoc />
        public bool Equals(IInterfaceDetail x, IInterfaceDetail y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            return x.MacAddressString?.ToUpperInvariant() == y.MacAddressString?.ToUpperInvariant();
        }

        /// <inheritdoc />
        public int GetHashCode(IInterfaceDetail obj)
        {
            if (obj is null)
            {
                return 0;
            }

            return obj.MacAddressString?.GetHashCode() ?? 0;
        }
    }
}