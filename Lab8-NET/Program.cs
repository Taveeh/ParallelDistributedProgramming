using MPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// mpiexec -n 3 netcoreapp3.1\Lab8-NET.exe
namespace Lab8_NET
{
    class Program
    {
        static void Main(string[] args)
        {
            using (new MPI.Environment(ref args))
            {

                int processRank = Communicator.world.Rank;
                int size = Communicator.world.Size;

                if (processRank == 0)
                {
                    mainWorker(size);
                }
                else
                {
                    childWorker(processRank);
                }
            }
        }

        private static void mainWorker(int mpiSize)
        {
            Console.WriteLine("MAIN");

            //generate subscriptions
            bool[] subscriptions = getRandomBooleanArray(mpiSize);
            bool[] subscriptions_2 = getRandomBooleanArray(mpiSize);
            int i;

            Console.WriteLine("MAIN: subs= ");
            for (i = 0; i < subscriptions.Count(); i++)
            {
                Console.Write(subscriptions[i]);
                Console.Write(" ");
            }
            Console.WriteLine("\nMAIN: subs2= ");
            for (i = 0; i < subscriptions_2.Count(); i++)
            {
                Console.Write(subscriptions_2[i]);
                Console.Write(" ");
            }
            Console.Write("\n");
            //process changes
            while (true)
            {

                Message changes;
                
                Communicator.world.Receive<Message>(Communicator.anySource, Communicator.anyTag, out changes);
                Console.WriteLine(changes);
                
                long mpiChild = changes.mpiId;
                long change = changes.change;
                bool first = changes.first;
                long value = changes.value;
                long time = changes.time;

                Console.WriteLine("Main: received from " + mpiChild + " (change " + change + "), " + "var" + (first ? "" : "2") + "= " + value + " and time= " + time);

                for (int processRank = 1; processRank < mpiSize; processRank++)
                {
                    if (processRank != mpiChild && (first ? subscriptions[processRank] : subscriptions_2[processRank]))
                    {
                        Console.WriteLine("Main: sent to " + processRank + ", var" + (first ? "" : "2") + "= " + value + " and time= " + time);
 
                        Communicator.world.Send(changes, processRank, 0);
                    }
                }
            }
        }

        private static void childWorker(int mpiMe)
        {
            //make sure main process is set up
            Thread.Sleep(1000);
            
            Console.WriteLine("CHILD: " + mpiMe);

            long var = 0L;
            long var2 = 0L;
            long time = DateTime.Now.ToFileTime();

            Thread thread = new Thread(() => childChangePerformer(mpiMe, var, var2, time));
            thread.Start();

            while (true)
            {
                Message data;
                Communicator.world.Receive<Message>(0, MPI.Communicator.anyTag, out data);

                bool first = data.first;
                long value = data.value;
                long current = data.time;

                Console.WriteLine("Child: " + mpiMe + ": received var" + (first ? "" : "2") + "= " + value + " and time= " + current);

                if (first)
                    Interlocked.Exchange(ref var, value);
                else
                    Interlocked.Exchange(ref var2, value);

                if (Interlocked.Read(ref time) < current)
                {
                    Console.WriteLine("Child: " + mpiMe + ": updated time= " + current + " (before " + time + ")");
                    Interlocked.Exchange(ref time, current);
                }
            }
        }

        private static void childChangePerformer(int mpiMe, long var, long var2, long time)
        {
            //initialise
            int CHANGES = 3;
            
            
            //perform changes
            for (int change = 0; change < CHANGES; change++)
            {
                long value = new Random().Next() % 100;
                bool first = new Random().Next(10) > 5;
                long current = DateTime.Now.ToFileTime();
                Interlocked.Exchange(ref time, current);

                if (first)
                    Interlocked.Exchange(ref var, value);
                else
                    Interlocked.Exchange(ref var2, value);

                Message data = new Message(mpiMe, change, first, value, time);
                
                Console.WriteLine(mpiMe + ": sent var" + (first ? "" : "2") + "= " + value + " (change " + change + ")");
                
                Communicator.world.Send(data, 0, 0);
            }
        }

        private static bool[] getRandomBooleanArray(int length)
        {
            bool[] array = new bool[length];

            for (int index = 0; index < length; index++)
            {
                array[index] = (new Random().Next(8) > 5);
            }

            return array;
        }
    }
}
