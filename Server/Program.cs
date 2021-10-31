using Server.Core;

namespace Solution
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var core = new Core(2050, 0xFF);
            core.Run();
            core.Shutdown();
        }
    }
}