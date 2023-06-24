using System.Runtime.InteropServices;

namespace Sniper.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct CommandMessage
    {
        public MinispyCommand Command;
        public int Reserved;
    }
}
