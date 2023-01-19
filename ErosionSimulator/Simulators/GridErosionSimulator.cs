using ErosionSimulator.Geology;
using System;

namespace ErosionSimulator.Simulators
{
    public class GridErosionSimulator : Simulator
    {
        private const double INITIAL_RAIN = 0.1d;
        private const double INITIAL_SEDIMENT = 0d;
        private const double FRICTION = 0.3d;
        private const double DELTA_T = 1d;
        private const double WATER_SPREAD = 0.5d;
        private const double SEDIMENT_CAPACITY = 1d;
        private const double DEPOSITION = 0.5d;
        private const double DISSOLUTION = 0.3d;
        private const double GRAVITY = 1d;
        private const int RAIN_FREQUENCY = 50;
        private const int DIFFUSION_FREQUENCY = 1;
        private readonly double INVERSE_SQRT2 = 1d / Math.Sqrt(2);

        public int SizeX { get; }
        public int SizeY { get; }
        private double[,] terrain;
        private WaterColumn[,] water;
        private System.Random random;
        private int sinceLastRain = 0;
        private int sinceLastDiffusion = 0;


        public GridErosionSimulator(int sizeX, int sizeY, TerrainProvider terrainProvider)
        {
            this.SizeX = sizeX;
            this.SizeY = sizeY;
            this.terrain = new double[SizeX, SizeY];
            this.water = new WaterColumn[SizeX, SizeY];
            this.random = new System.Random();

            // initialize terrain data
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    this.terrain[i, j] = terrainProvider.GetHeightAt(i, j);
                    this.water[i, j] = new WaterColumn(INITIAL_RAIN, INITIAL_SEDIMENT);          
                }
            }
        }

        public void Rain()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    water[i, j].Height += INITIAL_RAIN;
                }
            }
        }

        private void Diffuse()
        {
            WaterColumn[,] water2 = new WaterColumn[SizeX, SizeY];

            // clone 
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    water2[x, y] = new WaterColumn(water[x, y].Height, 0d);
                }
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    // apply Gaussian blur to the amount of sediment in the water column
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (x + i >= 0 && y + j >= 0 && x + i < SizeX && y + j < SizeY)
                            {
                                if (i == 0 && j == 0)
                                {
                                    water2[x + i, y + j].Sediment += water[x, y].Sediment / 4d;
                                } else if (i == 0 || j == 0)
                                {
                                    water2[x + i, y + j].Sediment += water[x, y].Sediment / 8d;
                                } else 
                                {
                                    water2[x + i, y + j].Sediment += water[x, y].Sediment / 16d;
                                }
                            }
                        }
                    }
                }
            }

            this.water = water2;
        }

        public override void Step()
        {
            if (sinceLastRain >= RAIN_FREQUENCY)
            {
                Rain();
                sinceLastRain = 1;
            } else
            {
                sinceLastRain++;
            }

            if (sinceLastDiffusion >= DIFFUSION_FREQUENCY)
            {
                Diffuse();
                sinceLastRain = 1;
            }
            else
            {
                sinceLastRain++;
            }

            // clone the water field - this makes sure that the erosion is not affected by the order of traversing cells
            // TODO - do it without cloning
            // only water2 is modified
            WaterColumn[,] water2 = new WaterColumn[SizeX, SizeY];
            double[,] terrain2 = new double[SizeX, SizeY];
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    water2[x, y] = new WaterColumn(water[x, y].Height, water[x, y].Sediment);
                    terrain2[x, y] = terrain[x, y];
                }
            }

            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    if (water[x, y].Height > 0)
                    {
                        double heightHere = terrain[x, y] + water[x, y].Height;

                        double[,] neighbors = new double[3, 3];
                        double[,] heightDifferences = new double[3, 3];
                        double totalWater = 0d;
                        double totalVelocityDistribution = 0d;
                        double minWater = double.MaxValue;

                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                if (!(i == 0 && j == 0))
                                {
                                    if (x + i < 0 || y + j < 0 || x + i > SizeX - 1 || y + j > SizeY - 1)
                                    {
                                        heightDifferences[1 + i, 1 + j] = 0d;
                                    }
                                    else
                                    {
                                        heightDifferences[1 + i, 1 + j] = Math.Max(heightHere - (terrain[x + i, y + j] + water[x + i, y + j].Height), 0d);
                                        totalWater += heightDifferences[1 + i, 1 + j];

                                        if (heightDifferences[1 + i, 1 + j] != 0 && heightDifferences[1 + i, 1 + j] < minWater)
                                        {
                                            minWater = heightDifferences[1 + i, 1 + j];
                                        }

                                        // a very weird calculation - it calculates how the water will spread according to its velocity vector
                                        neighbors[1 + i, 1 + j] = Math.Exp(-WATER_SPREAD * (new Vector2(i, j) - water[x, y].Velocity.NormalizedOrSmaller()).SquareMagnitude());
                                        totalVelocityDistribution += neighbors[1 + i, 1 + j];
                                    }
                                }
                            }
                        }

                        // calculate height differences of water levels between this cell and the 4 neighbors
                        /*
                        double top = y >= SizeY - 1 ? 0d : Math.Max(heightHere - (terrain[x, y + 1] + water[x, y + 1].Height), 0d);
                        double bottom = y <= 0 ? 0d : Math.Max(heightHere - (terrain[x, y - 1] + water[x, y - 1].Height), 0d);
                        double right = x >= SizeX - 1 ? 0d : Math.Max(heightHere - (terrain[x + 1, y] + water[x + 1, y].Height), 0d);
                        double left = x <= 0 ? 0d : Math.Max(heightHere - (terrain[x - 1, y] + water[x - 1, y].Height), 0d);
                        */

                        // we never want to give away so much water that this cell will end up lower than all its neigbors


                        // this will make sure that this column never gets lower than the lowest of its neighbors
                        // don't try to imagine what it does, this is just what fell out when I tried calculating it
                        totalWater = Math.Max(totalWater, 0d);
                        double outputWater = Math.Min(totalWater, water[x, y].Height) / 2d;
                        // double outputWater = (minWater * totalWater) / (minWater + totalWater);

                        double capacity = (SEDIMENT_CAPACITY / DELTA_T) * outputWater * water[x, y].Velocity.Magnitude();
                        double sedimentPart = water[x, y].Sediment * outputWater / water[x, y].Height;

                        // deposit or erode, the equations are the same
                        terrain[x, y] += (DEPOSITION / DELTA_T) * (sedimentPart - capacity);
                        water[x, y].Sediment -= (DEPOSITION / DELTA_T) * (sedimentPart - capacity);
                        water[x, y].Height -= (DEPOSITION / DELTA_T) * (sedimentPart - capacity);

                        double outputSediment = 0d;

                        if (outputWater > 0)
                        {
                            for (int i = -1; i <= 1; i++)
                            {
                                for (int j = -1; j <= 1; j++)
                                {
                                    if (!(i == 0 && j == 0))
                                    {
                                        if (!(x + i < 0 || y + j < 0 || x + i > SizeX - 1 || y + j > SizeY - 1))
                                        {
                                            double part = (heightDifferences[1 + i, 1 + j] / totalWater) * outputWater;
                                            water[x + i, y + j].Height += part;
                                            double sediment = water[x, y].Sediment * part / water[x, y].Height;
                                            water[x + i, y + j].Sediment += sediment;
                                            outputSediment += sediment;

                                            // velocity update
                                            double k = part / water[x + i, y + j].Height;
                                            Vector2 partVelocity = water[x, y].Velocity
                                                + DELTA_T * (new Vector2(i, j) * heightDifferences[1 + i, 1 + j] * GRAVITY)
                                                - DELTA_T * water[x, y].Velocity * FRICTION;
                                            water[x + i, y + j].Velocity = InterpolateLinear(partVelocity, water[x + i, y + j].Velocity, k);
                                        }
                                    }
                                }
                            }

                            water[x, y].Height -= outputWater;
                            water[x, y].Sediment -= outputSediment;
                        }
                      
                    }
                }
            }

            //this.water = water2;
        }

        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // interpolate
            return InterpolateLinear(
                InterpolateLinear(terrain[lowX, lowY], terrain[lowX, highY], y - lowY),
                InterpolateLinear(terrain[highX, lowY], terrain[highX, highY], y - lowY),
                x - lowX);
        }
    }
}
