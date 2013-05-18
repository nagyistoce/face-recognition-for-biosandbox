using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceRecognitionClient
{
    class Tools
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static string GetLogMessage(string message)
        {
            return string.Format("[{0}]Log: {1}\n", DateTime.Now, message);
        }

        public static string GetErrorMessage(string message)
        {
            return string.Format("[{0}]Error: {1}\n", DateTime.Now, message);
        }

        public static string CleanVectorString(string vector)
        {
            string tmp = vector.Replace("\n", "");
            tmp = tmp.Replace("\r", "");
            return tmp;
        }
    }
}
