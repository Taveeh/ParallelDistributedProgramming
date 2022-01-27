using MPI;
using System;
using System.Linq;
// mpiexec -n 3 netcoreapp3.1\Lab7-NET.exe
namespace Lab7_NET
{
    class Program
    {
        public static Polynomial MergeMultiplicationResult(Polynomial[] results)
        {
            Polynomial result = new Polynomial(results[0].degree);

            for (int i = 0; i < result.size; i++)
                for (int j = 0; j < results.Length; j++)
                    result.coefficients[i] += results[j].coefficients[i];

            return result;
        }

        public static void MPIMultiplicationMaster(Polynomial polynomial1, Polynomial polynomial2)
        {

            int n = Communicator.world.Size;
            Polynomial result = null;

            #region multipleProcess
            if (n > 1)
            {
                int degreeResult = polynomial1.degree + polynomial2.degree;

                int begin = 0;
                int end = 0;
                int step = degreeResult / (n - 1);

                if (step == 0)
                {
                    step = 1;
                }

                for (int i = 1; i < n; i++)
                {
                    begin = end;
                    end = end + step;

                    if (i == n - 1)
                        end = degreeResult + 1;

                    Communicator.world.Send(polynomial1, i, 0);
                    Communicator.world.Send(polynomial2, i, 0);
                    Communicator.world.Send(begin, i, 0);
                    Communicator.world.Send(end, i, 0);
                }

                Polynomial[] results = new Polynomial[n - 1];

                for (int i = 1; i < n; i++)
                    results[i - 1] = Communicator.world.Receive<Polynomial>(i, 0);
                result = MergeMultiplicationResult(results);
            }
            #endregion

            #region singleProcess
            else if (n == 1)
            {
                int degreeResult = polynomial1.degree + polynomial2.degree;
                result = MultiplicationHelper.ClassicMultiplication(polynomial1, polynomial2, 0, degreeResult + 1);
            }
            #endregion

            Console.WriteLine("MPI Multiplication: " + result);
        }

        public static void MPIMultiplicationWorker()
        {

            Polynomial polynomial1 = Communicator.world.Receive<Polynomial>(0, 0);
            Polynomial polynomial2 = Communicator.world.Receive<Polynomial>(0, 0);

            int begin = Communicator.world.Receive<int>(0, 0);
            int end = Communicator.world.Receive<int>(0, 0);

            Polynomial result = MultiplicationHelper.ClassicMultiplication(polynomial1, polynomial2, begin, end);

            Communicator.world.Send(result, 0, 0);
        }

        public static void MPIKaratsubaMaster(Polynomial polynomial1, Polynomial polynomial2)
        {

            Polynomial result = new Polynomial(polynomial1.degree * 2);
            if (Communicator.world.Size == 1)
            {
                result = MultiplicationHelper.KaratsubaMultiplicationWrapper(polynomial1, polynomial2);
            }
            else
            {
                Communicator.world.Send<int>(0, 1, 0);
                Communicator.world.Send<int[]>(polynomial1.coefficients, 1, 0);
                Communicator.world.Send<int[]>(polynomial2.coefficients, 1, 0);
                if (Communicator.world.Size == 2)
                    Communicator.world.Send<int[]>(new int[0], 1, 0);
                else
                    Communicator.world.Send<int[]>(Enumerable.Range(2, Communicator.world.Size - 2).ToArray(), 1, 0);

                int[] coefs = Communicator.world.Receive<int[]>(1, 0);
                result.coefficients = coefs;
            }

            Console.WriteLine("MPI  Karatsuba: " + result.ToString() + "\n");
        }

        public static void MPIKaratsubaWorker()
        {
            int from = Communicator.world.Receive<int>(Communicator.anySource, 0);
            int[] coefficients1 = Communicator.world.Receive<int[]>(Communicator.anySource, 0);
            int[] coefficients2 = Communicator.world.Receive<int[]>(Communicator.anySource, 0);
            int[] availableProcesses = Communicator.world.Receive<int[]>(Communicator.anySource, 0);

            int coefficientsLength = coefficients1.Length;

            int[] productCoefficients = new int[2 * coefficientsLength];

            //Handle the base case where the polynomial has only one coefficient
            if (coefficients1.Length == 1)
            {
                productCoefficients[0] = coefficients1[0] * coefficients2[0];

                Communicator.world.Send<int[]>(productCoefficients, from, 0);
                return;
            }

            int newSize = coefficients1.Length / 2;

            // Divide the polynomials' coefficients
            int[] coefficients1Low = Polynomial.GetLow(newSize, coefficients1);
            int[] coefficients1High = Polynomial.GetHigh(newSize, coefficients1);
            int[] coefficients2Low = Polynomial.GetLow(newSize, coefficients2);
            int[] coefficients2High = Polynomial.GetHigh(newSize, coefficients2);

            int[] coefficients1LowHigh = Polynomial.GetLowHigh(newSize, coefficients1Low, coefficients1High);
            int[] coefficients2LowHigh = Polynomial.GetLowHigh(newSize, coefficients2Low, coefficients2High);

            //Recursively call method on smaller arrays and construct the low and high parts of the product
            int[] productLowCoefficients, productHighCoefficients, productLowHighCoefficients;

            if (availableProcesses.Length == 0)
            {
                // No process available for distributing the workload
                productLowCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1Low, coefficients2Low);
                productHighCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1High, coefficients2High);
                productLowHighCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1LowHigh, coefficients2LowHigh);
            }

            else if (availableProcesses.Length == 1)
            {
                // 1 process available for distributing the workload
                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients1Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients2Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[0], 0);

                // The rest is computed normally
                productHighCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1High, coefficients2High);
                productLowHighCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1LowHigh, coefficients2LowHigh);

                productLowCoefficients = Communicator.world.Receive<int[]>(availableProcesses[0], 0);
            }
            else if (availableProcesses.Length == 2)
            {
                // 2 processes available for distributing the workload
                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients1Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients2Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[0], 0);

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients1High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients2High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[1], 0);

                // The rest is computed normally
                productLowHighCoefficients = MultiplicationHelper.KaratsubaMultiplication(coefficients1LowHigh, coefficients2LowHigh);

                productLowCoefficients = Communicator.world.Receive<int[]>(availableProcesses[0], 0);
                productHighCoefficients = Communicator.world.Receive<int[]>(availableProcesses[1], 0);
            }
            else if (availableProcesses.Length == 3)
            {
                // 3 processes available for distributing the workload
                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients1Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients2Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[0], 0);

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients1High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients2High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[1], 0);

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(coefficients1LowHigh, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(coefficients2LowHigh, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(new int[0], availableProcesses[2], 0);

                productLowCoefficients = Communicator.world.Receive<int[]>(availableProcesses[0], 0);
                productHighCoefficients = Communicator.world.Receive<int[]>(availableProcesses[1], 0);
                productLowHighCoefficients = Communicator.world.Receive<int[]>(availableProcesses[2], 0);
            }

            else
            {
                // More than 3 processes available for distributing the workload
                int[] tempAvailableProcesses = availableProcesses.Skip(3).ToArray();
                int auxLength = tempAvailableProcesses.Length / 3;

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients1Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(coefficients2Low, availableProcesses[0], 0);
                Communicator.world.Send<int[]>(tempAvailableProcesses.Take(auxLength).ToArray(), availableProcesses[0], 0);

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients1High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(coefficients2High, availableProcesses[1], 0);
                Communicator.world.Send<int[]>(tempAvailableProcesses.Skip(auxLength).Take(auxLength).ToArray(), availableProcesses[1], 0);

                Communicator.world.Send<int>(Communicator.world.Rank, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(coefficients1LowHigh, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(coefficients2LowHigh, availableProcesses[2], 0);
                Communicator.world.Send<int[]>(tempAvailableProcesses.Skip(2 * auxLength).ToArray(), availableProcesses[2], 0);

                productLowCoefficients = Communicator.world.Receive<int[]>(availableProcesses[0], 0);
                productHighCoefficients = Communicator.world.Receive<int[]>(availableProcesses[1], 0);
                productLowHighCoefficients = Communicator.world.Receive<int[]>(availableProcesses[2], 0);
            }

            //Construct the middle portion of the product
            int[] productMiddle = new int[coefficientsLength];
            for (int index = 0; index < coefficientsLength; index++)
            {
                productMiddle[index] = productLowHighCoefficients[index] - productLowCoefficients[index] - productHighCoefficients[index];
            }

            //Assemble the product from the low, middle and high parts. Start with the low and high parts of the product.
            for (int index = 0, middleOffset = coefficientsLength / 2; index < coefficientsLength; ++index)
            {
                productCoefficients[index] += productLowCoefficients[index];
                productCoefficients[index + coefficientsLength] += productHighCoefficients[index];
                productCoefficients[index + middleOffset] += productMiddle[index];
            }

            Communicator.world.Send<int[]>(productCoefficients, from, 0);

        }

        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {
                Console.WriteLine("Hello from process with rank " + Communicator.world.Rank + " running on " + MPI.Environment.ProcessorName);
                if (Communicator.world.Rank == 0)
                {
                    // Master Process

                    int degree = 3;
                    Polynomial polynomial1 = new Polynomial(degree);
                    Polynomial polynomial2 = new Polynomial(degree);

                    polynomial1.GenerateRandom();
                    polynomial2.GenerateRandom();

                    Console.WriteLine("P1 = " +  polynomial1 + "\n");
                    Console.WriteLine("P2 = " +  polynomial2 + "\n");

                    MPIMultiplicationMaster(polynomial1, polynomial2);
                    
                    Console.WriteLine("\n");
                    
                    MPIKaratsubaMaster(polynomial1, polynomial2);
                }

                else
                {
                    // Worker Process
                    MPIMultiplicationWorker();
                    MPIKaratsubaWorker();
                }
            }
        }
    }
}