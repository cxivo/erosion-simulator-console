using System;
namespace ErosionSimulator
{
    public class CompositeNoise : TerrainProvider
    {
        public double Scale { get; }
        public double Height { get; }
        private readonly int levels;
        private readonly TerrainProvider[] noises;

        public CompositeNoise(double scale, double height, params TerrainProvider[] noises)
        {
            this.Scale = scale;
            this.Height = height;
            this.noises = noises;
            this.levels = noises.Length;
        }

        public override double GetHeightAt(double x, double y)
        {
            double value = 0d;
            double power = 1d;
            for (int i = 0; i < levels; i++)
            {
                value += Height * noises[i].GetHeightAt(Scale * x * power + 0.5d, Scale * y * power + 0.5d) / power;
                power *= 2d;
            }
            return value;
        }
    }
}