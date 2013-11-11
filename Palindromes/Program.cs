using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Palindromes
{
    class Program
    {
        static void Main(string[] args)
        {
            var downloadString = new TransformBlock<string, string>(Uri =>
            {
                Console.WriteLine("Downloading '{0}'...", Uri);

                return new WebClient().DownloadString(Uri);
            });

            var createWordList = new TransformBlock<string, string[]>(text =>
            {
                Console.WriteLine("Creating word list...");

                var tokens = text.ToArray()
                    .Select(x => char.IsLetter(x) ? x : ' ');

                text = new string(tokens.ToArray());

                return text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            });

            var filterWordList = new TransformBlock<string[], string[]>(words =>
            {
                Console.WriteLine("Filtering word list...");

                return words
                    .Where(word => word.Length > 3)
                    .OrderBy(word => word)
                    .Distinct()
                    .ToArray();
            });


            // TOutput is an IEnumerable<string>
            var findPalindromes = new TransformManyBlock<string[], string>(words =>
            {
                Console.WriteLine("Finding Palindrome...");

                var palindromes = new ConcurrentQueue<string>();

                Parallel.ForEach(words, word =>
                {

                    string reverse = new string(word.Reverse().ToArray());

                    if (Array.BinarySearch<string>(words, reverse) >= 0 &&
                        word != reverse)
                    {
                        palindromes.Enqueue(word);
                    }
                });

                return palindromes;
            });

            var printPalindrome = new ActionBlock<string>(palindrome =>
            {
                Console.WriteLine("Found palindrome {0}/{1}",
                    palindrome, new string(palindrome.Reverse().ToArray()));
            });

            downloadString.LinkTo(createWordList);
            createWordList.LinkTo(filterWordList);
            filterWordList.LinkTo(findPalindromes);
            findPalindromes.LinkTo(printPalindrome);

            downloadString.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)createWordList).Fault(t.Exception);
                else createWordList.Complete();
            });
            
            createWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)filterWordList).Fault(t.Exception);
                else filterWordList.Complete();
            });
            
            filterWordList.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)findPalindromes).Fault(t.Exception);
                else findPalindromes.Complete();
            });
            
            findPalindromes.Completion.ContinueWith(t =>
            {
                if (t.IsFaulted) ((IDataflowBlock)printPalindrome).Fault(t.Exception);
                else printPalindrome.Complete();
            });

            downloadString.Post("http://www.gutenberg.org/files/6130/6130-0.txt");

            downloadString.Complete();

            printPalindrome.Completion.Wait();

            Console.ReadKey();
        }
    }
}