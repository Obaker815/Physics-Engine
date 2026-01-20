namespace Physics_Engine
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Game();
            game.Run();
        }
    }
}