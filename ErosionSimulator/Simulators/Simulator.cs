using System;
using System.Collections.Generic;
using System.Text;

namespace ErosionSimulator
{
    public abstract class Simulator : TerrainProvider
    {
        public abstract void Step();

        // simulate n steps at once
        public void Step(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Step();
            }
        }
    }
}
