using System;
using System.Text;

namespace Lab7_NET
{
    [Serializable]
    public class Polynomial
    {
        public int degree { get; set; }
        public int[] coefficients { get; set; }
        public int size = 0;

        public Polynomial(int s)
        {
            degree = s;
            size = s + 1;
            coefficients = new int[size];
        }

        public void GenerateRandom()
        {
            Random rnd = new Random();

            for (int i = 0; i < size; i++)
            {
                coefficients[i] = rnd.Next(-10, 10);
                if (i == size - 1)
                {
                    while (coefficients[i] == 0)
                    {
                        coefficients[i] = rnd.Next(-10, 10);
                    }
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = size - 1; i >= 0; i--)
            {
                if (coefficients[i] != 0)
                {
                    if (coefficients[i] < 0)
                    {
                        sb.Append(coefficients[i]);
                    }
                    else if (coefficients[i] > 0)
                    {
                        if (i < size - 1)
                        {
                            sb.Append("+");
                        }
                        sb.Append(coefficients[i]);
                    }


                    if (i == 1)
                    {
                        sb.Append("*");
                        sb.Append("X");
                    }
                    else if (i != 0)
                    {
                        sb.Append("*");
                        sb.Append("X^");
                        sb.Append(i);
                    }

                }
            }

            return sb.ToString();
        }

        #region coefficientHelpers
        public static int[] GetLow(int m, int[] coefficients)
        {
            int[] coefficientsLow = new int[m];

            for (int i = 0; i < m; i++)
            {
                coefficientsLow[i] = coefficients[i];
            }

            return coefficientsLow;

        }

        public static int[] GetHigh(int m, int[] coefficients)
        {
            int[] coefficientsHigh = new int[m];

            for (int i = 0; i < m; i++)
            {
                coefficientsHigh[i] = coefficients[i + m];
            }

            return coefficientsHigh;

        }

        public static int[] GetLowHigh(int m, int[] coefficientsLow, int[] coefficientsHigh)
        {
            int[] coefficientsLowHigh = new int[m];

            for (int i = 0; i < m; i++)
            {
                coefficientsLowHigh[i] = coefficientsLow[i] + coefficientsHigh[i];
            }

            return coefficientsLowHigh;
        }
        #endregion

    }
}
