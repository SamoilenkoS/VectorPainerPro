using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MultitaskingAndMultithreading
{
    class Program
    {
        static void PrintNumbers()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            }
        }

        static List<int> Items = new List<int>();
        static object locker = new object();

        public static void Filter(object obj)
        {
            var data = obj as IEnumerable<int>;
            var filtered = data.Where(x => x % 2 == 0);
            lock (locker)
            {
                Items.AddRange(filtered);
            }
        }

        //static void Main(string[] args)
        //{
        //    int count = 20;
        //    int threadCount = 5;
        //    Thread[] threads = new Thread[threadCount];
        //    for (int i = 0; i < threadCount; i++)
        //    {
        //        threads[i] = new Thread(Filter);
        //        threads[i].Start(Enumerable.Range(i * count, count));
        //    }

        //    for (int i = 0; i < threadCount; i++)
        //    {
        //        threads[i].Join();
        //    }

        //    foreach (var item in Items)
        //    {
        //        Console.WriteLine(item);
        //    }
        //}

        //
        private static Semaphore _pool;

        // A padding interval to make the output more orderly.
        private static int _padding;

        public static void Main()
        {
            // Create a semaphore that can satisfy up to three
            // concurrent requests. Use an initial count of zero,
            // so that the entire semaphore count is initially
            // owned by the main program thread.
            //
            _pool = new Semaphore(0, 3);

            // Create and start five numbered threads. 
            //
            for (int i = 1; i <= 5; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(Worker));

                // Start the thread, passing the number.
                //
                t.Start(i);
            }

            // Wait for half a second, to allow all the
            // threads to start and to block on the semaphore.
            //
            Thread.Sleep(500);

            // The main thread starts out holding the entire
            // semaphore count. Calling Release(3) brings the 
            // semaphore count back to its maximum value, and
            // allows the waiting threads to enter the semaphore,
            // up to three at a time.
            //
            Console.WriteLine("Main thread calls Release(3).");
            _pool.Release(3);

            Console.WriteLine("Main thread exits.");
        }

        private static void Worker(object num)
        {
            // Each worker thread begins by requesting the
            // semaphore.
            Console.WriteLine("Thread {0} begins " +
                "and waits for the semaphore.", num);
            _pool.WaitOne();

            // A padding interval to make the output more orderly.
            int padding = Interlocked.Add(ref _padding, 100);

            Console.WriteLine("Thread {0} enters the semaphore.", num);

            // The thread's "work" consists of sleeping for 
            // about a second. Each thread "works" a little 
            // longer, just to make the output more orderly.
            //
            Thread.Sleep(1000 + padding);

            Console.WriteLine("Thread {0} releases the semaphore.", num);
            Console.WriteLine("Thread {0} previous semaphore count: {1}",
                num, _pool.Release());
        }
    }
}
