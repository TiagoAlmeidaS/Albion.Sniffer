using System.Numerics;
using System.Reflection;

namespace Albion.Sniffer.Core.Utility
{
    [Obfuscation(Feature = "mutation", Exclude = false)]
    public static class Additions
    {
        private const double angle = -45 * Math.PI / 180;

        public static Vector2 fromValues(float x, float y)
        {
            return new Vector2(y, x);
        }

        public static Vector2 fromFArray(float[] array)
        {
            if (array.Length == 2)
            {
                return new Vector2(array[1], array[0]);
            }
            else
            {
                return new Vector2();
            }
        }

        public static Vector2 Rotate(this Vector2 v)
        {
            return new Vector2(
                (float)(v.X * Math.Cos(angle) - v.Y * Math.Sin(angle)),
                (float)(v.X * Math.Sin(angle) + v.Y * Math.Cos(angle))
            );
        }

        public static float Magnitude(this Vector2 vector)
        {
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        private static float CornerColorValue(float value, float minus)
        {
            if (value == 0 || value - minus < 0)
            {
                return 0;
            }
            else
            {
                return value - minus;
            }
        }
    }
}
