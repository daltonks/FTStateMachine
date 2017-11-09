using System;

namespace FTStateMachineExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var storeExample = new StoreExample();
            storeExample.Run();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
