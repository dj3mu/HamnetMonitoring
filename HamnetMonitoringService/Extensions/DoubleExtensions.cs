namespace HamnetMonitoringService
{
    internal static class DoubleExtensions
    {
        private const double InvalidDouble = -9999;

        public static double ToInfluxValidDouble(this double? orignal)
        {
            if ((orignal == null) || !orignal.HasValue)
            {
                return InvalidDouble;
            }

            if (double.IsInfinity(orignal.Value))
            {
                return double.MinValue;
            }

            if (double.IsNaN(orignal.Value))
            {
                return InvalidDouble;
            }

            return orignal.Value;
        }

        public static double ToInfluxValidDouble(this double orignal)
        {
            if (double.IsInfinity(orignal))
            {
                return InvalidDouble;
            }

            if (double.IsNaN(orignal))
            {
                return InvalidDouble;
            }

            return orignal;
        }
    }
}