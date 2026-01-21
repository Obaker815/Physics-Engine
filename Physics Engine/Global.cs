using OpenTK.Mathematics;
using System.Diagnostics;

namespace Physics_Engine
{
    internal static class Global
    {
        public const float Gravity = -9.81f;
        public const double FramerateCap = 165f;

        public static bool DebugMode = false;
        public static Vector2 MousePosition = Vector2.Zero;
        public static Vector2 MouseDelta = Vector2.Zero;
        public static Stopwatch ElapsedTimer = new();
        public static Stopwatch Deltatimer = new();
        public static float Deltatime = 0f;

        public static void StartTimers()
        {
            ElapsedTimer.Start();
            Deltatimer.Start();
        }
        public static void UpdateDeltatime()
        {
            Deltatime = (float)Deltatimer.Elapsed.TotalSeconds;
            Deltatimer.Restart();
        }
        public static float Elapsedtime => (float)ElapsedTimer.Elapsed.TotalSeconds;
    }
}
