using System;

namespace ErosionSimulator
{
    // it even rhymes!
    public class ArrayTerrain : TerrainProvider
    {
        private double[,] heights;

        public ArrayTerrain(double[,] heights)
        {
            this.heights = heights;
        }


        // if the input point is outside the array, just return a point in the array closest to the wanted one
        public override double GetHeightAt(double x, double y)
        {
            int lowX = Math.Max((int)Math.Floor(x), 0);
            int highX = Math.Min((int)Math.Ceiling(x), heights.GetLength(0) - 1);
            int lowY = Math.Max((int)Math.Floor(y), 0);
            int highY = Math.Min((int)Math.Ceiling(y), heights.GetLength(1) - 1);

            return InterpolateSmooth(
               InterpolateSmooth(heights[lowX, lowY], heights[lowX, highY], y - lowY),
               InterpolateSmooth(heights[highX, lowY], heights[highX, highY], y - lowY), x - lowX);
        }
    }
}