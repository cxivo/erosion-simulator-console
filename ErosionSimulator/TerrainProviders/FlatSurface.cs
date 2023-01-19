namespace ErosionSimulator
{
    public class FlatSurface : TerrainProvider
    {
        public override double GetHeightAt(double x, double y)
        {
            return 0d;
        }
    }
}
