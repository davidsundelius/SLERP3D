using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;

//This is code is not written by the RacingGame project group, it was found at http://forums.xna.com/forums/p/16667/86975.aspx
//and is used for debugging only.

namespace PIXTools
{
    public enum PIXOption
    {
        DisallowProfiling = 1
    }

    public class PIXTools
    {
        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern int D3DPERF_BeginEvent(uint col, String wszName);

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern int D3DPERF_EndEvent();

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern int D3DPERF_SetMarker(uint col, String wszName);

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Auto)]
        private static extern int D3DPERF_SetRegion(uint col, String wszName);

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern int D3DPERF_QueryRepeatFrame();

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern void D3DPERF_SetOptions(uint dwOptions);

        [DllImport("d3d9.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern uint D3DPERF_GetStatus();

        private static int NestedBegins = 0;
        private static bool enabled = true;

        public static bool IsCurrentlyProfiling
        {
            get { return D3DPERF_GetStatus() != 0; }
        }

        public static bool Enabled
        {
            get { return enabled; }
            set { enabled = true; }
        }

        public static int BeginEvent(Color color, String eventName)
        {
            if (!enabled)
                return -1;

            NestedBegins++;

            return D3DPERF_BeginEvent(color.PackedValue, eventName);
        }

        public static int BeginEvent(String eventName)
        {
            if (!enabled)
                return -1;

            return BeginEvent(Color.Black, eventName);
        }

        public static int EndEvent()
        {
            if (!enabled)
                return -1;

            if (NestedBegins == 0)
                throw new PIXToolsException("BeginEvent must be called prior to a EndEvent call.");

            NestedBegins--;

            return D3DPERF_EndEvent();
        }

        public static void SetMarker(Color color, String eventName)
        {
            if (!enabled)
                return;

            D3DPERF_SetMarker(color.PackedValue, eventName);
        }

        public static void SetRegion(Color color, String eventName)
        {
            if (!enabled)
                return;

            D3DPERF_SetRegion(color.PackedValue, eventName);
        }

        public static bool QueryRepeatFrame()
        {
            if (!enabled)
                return false;

            return D3DPERF_QueryRepeatFrame() == 0 ? false : true;
        }

        public static void SetOptions(PIXOption options)
        {
            enabled = !((options & PIXOption.DisallowProfiling) == PIXOption.DisallowProfiling);

            if (!enabled)
                return;

            D3DPERF_SetOptions((uint)options);
        }

        public static uint GetStatus()
        {
            if (!enabled)
                return 0;

            return D3DPERF_GetStatus();
        }
    }

    public class PIXToolsException : Exception
    {
        public PIXToolsException()
        {
        }

        public PIXToolsException(string message)
            : base(message)
        {
        }

        public PIXToolsException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PIXToolsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}