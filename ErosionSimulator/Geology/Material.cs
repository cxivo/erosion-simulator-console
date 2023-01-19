namespace ErosionSimulator.Geology
{
    public class Material
    {
        // warning - the constants are made up
        public static readonly Material WATER = new Material(0d, 1d);
        public static readonly Material SOIL = new Material(1d, 1.2d);
        public static readonly Material ROCK = new Material(4d, 2d);

        public double Hardness { get; }
        public double Density { get; }  // in g/cm^3

        Material(double hardness, double density)
        {
            this.Hardness = hardness;
            this.Density = density;
        }
    }
}
