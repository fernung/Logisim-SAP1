using System;

namespace SapCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Instructions.Compile();
            Instructions.Assemble("RAM");
        }
    }
}
