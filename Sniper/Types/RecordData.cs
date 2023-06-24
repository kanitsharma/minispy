using System.Runtime.InteropServices;

namespace Sniper.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct RecordData
    {
        public ulong OriginatingTime;
        public ulong CompletionTime;

        public EventType EventType;

        public int Flags;
        public ulong ProcessId;
    }

}
