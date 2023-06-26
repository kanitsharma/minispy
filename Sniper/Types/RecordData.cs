using System.Runtime.InteropServices;
using System.Security.Principal;

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

        public ulong sid;
    }
}
