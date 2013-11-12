using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DataFlow
{
     class Program
    {
        static void Main(string[] args)
        {
            var printBlock = new ActionBlock<string[]>(names =>
            {
                foreach (string name in names)
                {
                    Console.WriteLine("The name is: {0}", name);    
                }
            });

            var batcher = new BatchBlock<string>(2);


            batcher.LinkTo(printBlock);

            batcher.Post("adam");
            Console.ReadKey();

            batcher.Post("kathy");
            Console.ReadKey();

            batcher.Post("bud");
            Console.ReadKey();

            batcher.Post("jacky");
            Console.ReadKey();

            // DataFlow ********************************
            //ThrowErrorPlain();
            //ThrowErrorContinueWith();
            //RunBufferBlock();
            //BroadcastBlock();
            //WriteOnceBlock();
            //ActionBlock();
            //TransformBlock();
            //TransformManuBlock();
            //BatchBlock();
            //JoinBlock();
            //BatchedJoinBlock();


            Console.ReadKey();
        }


        #region Dataflow

        //*****************************************************************
        // Dataflow (Task Parallel Library)
        // 
        // http://msdn.microsoft.com/en-us/library/hh228603(v=vs.110).aspx
        //
        //*****************************************************************

        private static void BatchedJoinBlock()
        {
            Func<int, int> DoWork = n =>
            {
                if (n < 0)
                    throw new ArgumentOutOfRangeException();
                return n;
            };

            var bjb = new BatchedJoinBlock<int, Exception>(7);

            foreach (int i in new int[] { 5, 6, -7, -22, 13, 55, 0 })
            {
                try
                {
                    bjb.Target1.Post(DoWork(i));
                }
                catch (ArgumentOutOfRangeException e)
                {
                    bjb.Target2.Post(e);
                }
            }

            var results = bjb.Receive();

            foreach (int n in results.Item1)
            {
                Console.WriteLine(n);
            }

            foreach (Exception e in results.Item2)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void JoinBlock()
        {
            var jb = new JoinBlock<int, int, char>();

            jb.Target1.Post(3);
            jb.Target1.Post(6);

            jb.Target2.Post(5);
            jb.Target2.Post(4);

            jb.Target3.Post('+');
            jb.Target3.Post('-');

            for (int i = 0; i < 2; i++)
            {
                var data = jb.Receive();
                switch (data.Item3)
                {
                    case '+':
                        Console.WriteLine("{0} + {1} = {2}",
                           data.Item1, data.Item2, data.Item1 + data.Item2);
                        break;
                    case '-':
                        Console.WriteLine("{0} - {1} = {2}",
                           data.Item1, data.Item2, data.Item1 - data.Item2);
                        break;
                    default:
                        Console.WriteLine("Unknown operator '{0}'.", data.Item3);
                        break;
                }
            }

        }

        private static void BatchBlock()
        {
            var batchBlock = new BatchBlock<int>(10);

            for (int i = 0; i < 13; i++)
            {
                batchBlock.Post(i);
            }

            batchBlock.Complete();

            Console.WriteLine("the sum of the elements in batch 1 is {0}",
                batchBlock.Receive().Sum());

            Console.WriteLine("the sum of the elements in batch 2 is {0}",
                batchBlock.Receive().Sum());
        }

        private static void TransformManuBlock()
        {
            var transformManyBlock = new TransformManyBlock<string, char>(s => s.ToCharArray());

            transformManyBlock.Post("hello");
            transformManyBlock.Post("world");

            for (int i = 0; i < ("hello" + "world").Length; i++)
            {
                Console.WriteLine(transformManyBlock.Receive());
            }

        }

        private static void TransformBlock()
        {
            var transformBlock = new TransformBlock<int, double>(n => Math.Sqrt(n));

            transformBlock.Post(10);
            transformBlock.Post(20);
            transformBlock.Post(30);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(transformBlock.Receive());
            }
        }

        private static void ActionBlock()
        {
            var actionBlock = new ActionBlock<int>(n => Console.WriteLine(n));

            for (int i = 0; i < 3; i++)
            {
                actionBlock.Post(i * 10);
            }

            actionBlock.Complete();
            actionBlock.Completion.Wait();
        }

        private static void WriteOnceBlock()
        {
            var writeOnceBlock = new WriteOnceBlock<string>(null);

            Parallel.Invoke(
                () => writeOnceBlock.Post("Message 1"),
                () => writeOnceBlock.Post("Message 2"),
                () => writeOnceBlock.Post("Message 3"));

            Console.WriteLine(writeOnceBlock.Receive());
        }

        private static void BroadcastBlock()
        {
            var broadcastBlock = new BroadcastBlock<double>(null);

            broadcastBlock.Post(Math.PI);

            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(broadcastBlock.Receive());
            }
        }

        private static void RunBufferBlock()
        {
            // Create a BufferBlock<int> object. 
            var bufferBlock = new BufferBlock<int>();

            // Post several messages to the block. 
            for (int i = 0; i < 3; i++)
            {
                bufferBlock.Post(i);
            }

            // Receive the messages back from the block. 
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine(bufferBlock.Receive());
            }

        }

        static void ThrowErrorContinueWith()
        {
            var throwIfNegative = new ActionBlock<int>(n =>
            {
                Console.WriteLine("n = {0}", n);
                if (n < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
            });

            throwIfNegative.Completion.ContinueWith(task =>
            {
                Console.WriteLine("The status of the completion task is '{0}'.", task.Status);
            });

            throwIfNegative.Post(0);
            throwIfNegative.Post(-1);
            throwIfNegative.Post(1);
            throwIfNegative.Post(-2);
            throwIfNegative.Complete();


            try
            {
                throwIfNegative.Completion.Wait();
                Console.ReadKey();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered {0}: {1}",
                        e.GetType().Name, e.Message);
                    return true;
                });
            }
        }

        static void ThrowErrorPlain()
        {
            var throwIfNegative = new ActionBlock<int>(n =>
            {
                Console.WriteLine("n = {0}", n);
                if (n < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
            });

            throwIfNegative.Post(0);
            throwIfNegative.Post(-1);
            throwIfNegative.Post(1);
            throwIfNegative.Post(-2);
            throwIfNegative.Complete();

            try
            {
                throwIfNegative.Completion.Wait();
            }
            catch (AggregateException ae)
            {
                ae.Handle(e =>
                {
                    Console.WriteLine("Encountered {0}: {1}",
                        e.GetType().Name, e.Message);
                    return true;
                });
            }
        }

        #endregion
    }
}
