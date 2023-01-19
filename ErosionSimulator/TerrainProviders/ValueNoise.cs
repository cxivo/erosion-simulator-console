using System;

namespace ErosionSimulator
{
    public class ValueNoise : TerrainProvider
    {
        private int seed;

        public ValueNoise(int seed)
        {
            this.seed = seed;
        }

        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // the seed is added last, otherwise it would just act like an offset
            double heightLowLow = KnuthHash(seed + KnuthHash(lowX + KnuthHash(lowY))) % 1d;
            double heightHighLow = KnuthHash(seed + KnuthHash(highX + KnuthHash(lowY))) % 1d;
            double heightLowHigh = KnuthHash(seed + KnuthHash(lowX + KnuthHash(highY))) % 1d;
            double heightHighHigh = KnuthHash(seed + KnuthHash(highX + KnuthHash(highY))) % 1d;

            // interpolate
            return InterpolateSmooth(
                InterpolateSmooth(heightLowLow, heightLowHigh, y - lowY),
                InterpolateSmooth(heightHighLow, heightHighHigh, y - lowY),
                x - lowX);
        }
    }
}