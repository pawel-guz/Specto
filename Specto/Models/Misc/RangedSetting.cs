using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specto
{
    public class RangedSetting<T> where T : IComparable
    {
        public T MinValue { get; set; }
        public T MaxValue { get; set; }
        public T Value { get; set; }

        public RangedSetting(T value, T minValue, T maxValue)
        {
            if (value.CompareTo(minValue) < 0)
                Value = minValue;
            else if (value.CompareTo(maxValue) > 0)
                Value = maxValue;
            else Value = value;
        }
    }

    public class RangedDoubleSetting
    {
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public double Value { get; set; }

        public RangedDoubleSetting(double value, double minValue, double maxValue)
        {
            if (value < minValue)
                Value = minValue;
            else if (value > maxValue)
                Value = maxValue;
            else Value = value;
        }
    }
}
