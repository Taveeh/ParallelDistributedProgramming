using System.Threading.Tasks;

namespace Lab7_NET
{
    public class MultiplicationHelper
    {

        public static Polynomial ClassicMultiplication(Polynomial p1, Polynomial p2, int start, int end)
        {
            int degreeResult = p1.degree + p2.degree;
            Polynomial result = new Polynomial(degreeResult);

            for (int i = start; i < end; i++)
            {
                if (i > result.size)
                {
                    break;
                }
                for (int j = 0; j <= i; j++)
                {
                    if (j < p1.size && (i - j) < p2.size)
                    {
                        int value = p1.coefficients[j] * p2.coefficients[i - j];
                        result.coefficients[i] += value;
                    }
                }
            } 

            return result;
        }

        public static Polynomial KaratsubaMultiplicationWrapper(Polynomial p1, Polynomial p2)
        {
            Polynomial result = new Polynomial(p1.degree + p2.degree);
            result.coefficients = KaratsubaMultiplication(p1.coefficients, p2.coefficients);

            return result;
        }

        public static int[] KaratsubaMultiplication(int[] coefficients1, int[] coefficients2)
        {
            int coefficientsLength = coefficients1.Length;
            int[] productCoefficients = new int[2 * coefficientsLength];
            
            // Stopping condition for recursion
            if (coefficientsLength == 1)
            {
                productCoefficients[0] = coefficients1[0] * coefficients2[0];
                return productCoefficients;
            }

            int newSize = coefficientsLength / 2;

            // Divide the polynomials' coefficients
            int[] coefficients1Low = Polynomial.GetLow(newSize, coefficients1);
            int[] coefficients1High = Polynomial.GetHigh(newSize, coefficients1);
            int[] coefficients2Low = Polynomial.GetLow(newSize, coefficients2);
            int[] coefficients2High = Polynomial.GetHigh(newSize, coefficients2);

            int[] coefficients1LowHigh = Polynomial.GetLowHigh(newSize, coefficients1Low, coefficients1High);
            int[] coefficients2LowHigh = Polynomial.GetLowHigh(newSize, coefficients2Low, coefficients2High);

            // Recursively call method on smaller arrays and construct the low and high parts of the product
            Task<int[]> t1 = Task<int[]>.Factory.StartNew(() =>
            {
                return KaratsubaMultiplication(coefficients1Low, coefficients2Low);
            });

            Task<int[]> t2 = Task<int[]>.Factory.StartNew(() =>
            {
                return KaratsubaMultiplication(coefficients1High, coefficients2High);
            });

            Task<int[]> t3 = Task<int[]>.Factory.StartNew(() =>
            {
                return KaratsubaMultiplication(coefficients1LowHigh, coefficients2LowHigh);
            });

            // Retrieve computations
            int[] productLowCoefficients = t1.Result;
            int[] productHighCoefficients = t2.Result;
            int[] productLowHighCoefficients = t3.Result;

            // Construct the middle portion of the product
            int[] productMiddle = new int[coefficientsLength];
            for (int halfSizeIndex = 0; halfSizeIndex < coefficientsLength; halfSizeIndex++)
                productMiddle[halfSizeIndex] = productLowHighCoefficients[halfSizeIndex] - productLowCoefficients[halfSizeIndex] - productHighCoefficients[halfSizeIndex];

            // Assemble the product from the low, middle and high parts. Start with the low and high parts of the product.
            for (int index = 0, middleOffset = coefficientsLength / 2; index < coefficientsLength; ++ index)
            {
                productCoefficients[index] += productLowCoefficients[index];
                productCoefficients[index + coefficientsLength] += productHighCoefficients[index];
                productCoefficients[index + middleOffset] += productMiddle[index];
            }

            return productCoefficients;

        }
    }
}
