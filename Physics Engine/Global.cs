using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

namespace Physics_Engine
{
    internal static class Global
    {
        // General constants
        public const float Gravity = -9.81f;
        public const double FramerateCap = 0f;
        public static bool DebugMode = false;

        // Timing properties
        public static Stopwatch ElapsedTimer = new();
        public static Stopwatch Deltatimer = new();
        public static float Deltatime = 0f;

        // Mouse properties
        public static Vector2 MouseDelta = Vector2.Zero;
        public static float MouseDeltaScroll = 0f;

        public static Dictionary<MouseButton, bool> MouseButtonStates = new()
        {
            { MouseButton.Left,   false },
            { MouseButton.Right,  false },
            { MouseButton.Middle, false },
        };

        public static void StartTimers()
        {
            ElapsedTimer.Start();
            Deltatimer.Start();
        }
        public static void Update()
        {
            Deltatime = (float)Deltatimer.Elapsed.TotalSeconds;
            Deltatimer.Restart();
            MouseDeltaScroll = 0f;
            MouseDelta = Vector2.Zero;
        }
        public static float Elapsedtime => (float)ElapsedTimer.Elapsed.TotalSeconds;
    }
}
