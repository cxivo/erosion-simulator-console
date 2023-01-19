using System;
namespace ErosionSimulator
{
    public struct Vector2
    {
        public double x, y;
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public bool IsZero()
        {
            return x == 0 && y == 0;
        }

        public double Magnitude()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double SquareMagnitude()
        {
            return x * x + y * y;
        }

        public Vector2 Normalized()
        {
            double m = Magnitude();
            return new Vector2(x / m, y / m);
        }

        public Vector2 NormalizedOrSmaller()
        {
            double m = Magnitude();
            if (m <= 1)
            {
                return this;
            }
            else
            {
                return new Vector2(x / m, y / m);
            }
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(double a, Vector2 b)
        {
            return new Vector2(a * b.x, a * b.y);
        }

        public static Vector2 operator *(Vector2 a, double b)
        {
            return new Vector2(a.x * b, a.y * b);
        }
    }
}