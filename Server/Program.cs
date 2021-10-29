using Server.Core;

namespace Solution
{
    public sealed class Program
    {
        public static void Main()
        {
            var core = new Core(2050, 0xFF);
            core.Run(1);
            core.Shutdown();
        }
    }
}