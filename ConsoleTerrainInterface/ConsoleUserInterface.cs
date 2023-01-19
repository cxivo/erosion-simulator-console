using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using ErosionSimulator;

// for more info about the library, visit https://docs.sixlabors.com/articles/imagesharp/index.html
public class ConsoleUserInterface
{
    static void Main(string[] args)
    {
        Console.WriteLine("Please save your heightmap into a file called \"input.bmp\" and then press Enter");
        Console.ReadLine();

        Image<Rgba32> image;

        try
        {
            image = Image.Load<Rgba32>("input.bmp");
        }
        catch (Exception e)
        {
            Console.WriteLine("There was an error opening the file: " + e.Message + "\nThe program will now exit.");
            return;
        }

        int sizeX = image.Width;
        int sizeY = image.Height;
        double[,] heights = new double[sizeX, sizeY];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    // yes, my height requirements are all over the place
                    // but this should produce the best results - height from 0 to 4
                    heights[x, y] = (row[x].R) / 64d;
                }
            }
        });

        Simulator simulator = new BasicParticleErosionSimulator(sizeX, sizeY, new ArrayTerrain(heights));

        // ask the user for number of simulation steps
        int steps = -1;
        while (steps == -1)
        {
            Console.WriteLine("Input the number of erosion steps you want to simulate (around 10000 is recommended): ");
            try
            {
                steps = Int32.Parse(Console.ReadLine());
            }
            catch (FormatException)
            {
                Console.WriteLine("Please enter a number.");
            }
            catch (OverflowException)
            {
                Console.WriteLine("The number you have inputed is too large, please enter a smaller one");
            }
        }

        // simulation
        for (int i = 0; i < steps; i++)
        {
            simulator.Step();
        }

        // output
        Console.WriteLine("Simulation finished.");

        // get the height values
        double[,] outputHeights = new double[sizeX, sizeY];
        double min = double.PositiveInfinity, max = double.NegativeInfinity;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                outputHeights[x, y] = simulator.GetHeightAt(x, y);

                if (outputHeights[x, y] < min)
                {
                    min = outputHeights[x, y];
                }

                if (outputHeights[x, y] > max)
                {
                    max = outputHeights[x, y];
                }
            }
        }

        // write to image
        using (Image<Rgba32> output = new Image<Rgba32>(sizeX, sizeY))
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    float brightness = (float)((outputHeights[x, y] - min) / (max - min));
                    output[x, y] = new Rgba32(brightness, brightness, brightness);
                }
            }
            output.SaveAsBmp("output.bmp");
        }

        Console.WriteLine("Image has been saved as \"output.bmp\".");
    }
}
