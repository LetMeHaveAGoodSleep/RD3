using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RD3.Shared
{
    public class RandomNumberUtil
    {
        public static double GetRandomDouble(double min = 0, double max = 100)
        {
            Random random = new Random();
            double range = max - min;
            double randomValue = random.NextDouble() * range + min;
            return randomValue;
        }

        public static float GetRandomSingle(float min = 0, float max = 100)
        {
            Random random = new Random();
            float range = max - min;
            float randomValue = random.NextSingle() * range + min;
            return randomValue;
        }

        public static float GetRandomInt(int min = 0, int max = 100)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
