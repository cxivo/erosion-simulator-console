namespace ErosionSimulator.Geology
{
    public struct WaterColumn
    {
        public double Height { get; set; }
        public Vector2 Velocity { get; set; }
        public double Sediment { get; set; }

        public WaterColumn(double height, double sediment)
        {
            this.Height = height;
            this.Sediment = sediment;
            this.Velocity = new Vector2(0d, 0d);
        }
    }
}
