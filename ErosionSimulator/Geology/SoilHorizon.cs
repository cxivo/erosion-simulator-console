using System.Collections.Generic;

namespace ErosionSimulator.Geology
{
    public class SoilHorizon
    {
        List<Material> layers = new List<Material>();
        List<double> heights = new List<double>();
        double dryHeight = 0d;  // height without water

        // builder
        public SoilHorizon AddLayer(Material material, double height)
        {
            if (material != Material.WATER)
            {
                this.dryHeight += height;
            }

            if (layers.Count > 0 && layers[layers.Count - 1] == material)
            {
                heights[layers.Count - 1] += height;
            }
            else
            {
                layers.Add(material);
                heights.Add(height);
            }

            return this;
        }

        public Material GetTopMaterial()
        {
            return layers[layers.Count - 1];
        }

        // input - how much at most you want to erode
        // output - how much was eroded (input if the Material layer was deeper than input or the height of the layer which is now removed)
        public double RemoveFromTopMaterial(double height)
        {
            if (layers.Count == 0)
            {
                return 0d;
            } 
            else if (heights[heights.Count - 1] > height)
            {
                // can remove how much I want
                heights[heights.Count - 1] -= height;

                if (GetTopMaterial() != Material.WATER) 
                {
                    dryHeight -= height; 
                }

                return height;
            }
            else
            {
                double previousHeight = heights[heights.Count - 1];
                if (GetTopMaterial() != Material.WATER)
                {
                    dryHeight -= height;
                }

                heights.RemoveAt(heights.Count - 1);
                layers.RemoveAt(layers.Count - 1);

                return previousHeight;
            }
        }

        public double GetHeight()
        {
            return dryHeight;
        }

        public double getWaterHeight()
        {
            if (layers.Count == 0 || layers[layers.Count - 1] != Material.WATER)
            {
                return 0d;
            }
            else
            {
                return heights[heights.Count - 1];
            }
        }
    }
}
