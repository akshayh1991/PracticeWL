using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.BL
{
    public static class RandomStringGenerator
    {
        private static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly Random random = new Random();

        public static string Generate(int length)
        {
            var stringBuilder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }
            return stringBuilder.ToString();
        }
    }
}
