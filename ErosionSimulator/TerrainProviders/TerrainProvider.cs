namespace ErosionSimulator
{
    public abstract class TerrainProvider
    {
        // child class must never raise an Exception, must return a value for every input
        public abstract double GetHeightAt(double x, double y);

        public double InterpolateSmooth(double a, double b, double weight)
        {
            return a + (3 * weight * weight - 2 * weight * weight * weight) * (b - a);
        }

        public double InterpolateLinear(double a, double b, double weight)
        {
            return a + weight * (b - a);
        }

        public Vector2 InterpolateLinear(Vector2 a, Vector2 b, double weight)
        {
            return a + weight * (b - a);
        }

        protected int KnuthHash(int i)
        {
            return (int)(i * 2654435761 % (2L * int.MaxValue));
        }
    }
}