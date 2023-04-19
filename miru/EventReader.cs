using CenterDevice.MiniFSWatcher.Events;
using CenterDevice.MiniFSWatcher.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Principal;

namespace CenterDevice.MiniFSWatcher
{
    class EventReader
    {
        public static List<FileSystemEvent> ReadFromBuffer(IntPtr buffer, long bufferSize)
        {
            var events = new List<FileSystemEvent>();
            int offset = 0;
            while (offset + Marshal.SizeOf(typeof(LogRecord)) < bufferSize)
            {
                var recordAddress = IntPtr.Add(buffer, offset);
                LogRecord record = ReadRecordFromBuffer(recordAddress);
                ValidateRecordLength(record, bufferSize);
                var stringBytes = record.Length - Marshal.SizeOf(typeof(LogRecord));
                string[] strings = ReadEventStringsFromBuffer(recordAddress, stringBytes);
                if (record.Data.EventType == EventType.Move)
                {
                    events.Add(CreateRenameOrMoveEvent(record, strings));
                }
                else
                {
                    events.Add(CreateFileSystemEvent(record, strings));
                }

                offset += record.Length;
            }

            return events;
        }

        private static void ValidateRecordLength(LogRecord record, long bufferSize)
        {
            if (record.Length <= 0 || record.Length > bufferSize)
            {
                throw new Exception("Invalid record length");
            }
        }

        private static LogRecord ReadRecordFromBuffer(IntPtr recordAddress)
        {
            return Marshal.PtrToStructure<LogRecord>(recordAddress);
        }

        private static string[] ReadEventStringsFromBuffer(IntPtr recordAddress, int stringBytes)
        {
            var stringOffset = IntPtr.Add(recordAddress, Marshal.SizeOf(typeof(LogRecord)));
            var data = new byte[stringBytes];
            Marshal.Copy(stringOffset, data, 0, stringBytes);
            return Encoding.Unicode.GetString(data).Split('\0');
        }

        private static FileSystemEvent CreateFileSystemEvent(LogRecord record, string[] strings)
        {
            var fileSystemEvent = new FileSystemEvent()
            {
                Filename = PathConverter.ReplaceDevicePath(strings[0]),
                ProcessId = record.Data.ProcessId,
                Type = record.Data.EventType,
                Account = GetProcessOwnerByID((int)record.Data.ProcessId)
            };
            return fileSystemEvent;
        }

        private static FileSystemEvent CreateRenameOrMoveEvent(LogRecord record, string[] strings)
        {
            var fileSystemEvent = new RenameOrMoveEvent()
            {
                Filename = PathConverter.ReplaceDevicePath(strings[1]),
                OldFilename = PathConverter.ReplaceDevicePath(strings[0]),
                ProcessId = record.Data.ProcessId,
                Type = record.Data.EventType,
                Account = GetProcessOwnerByID((int)record.Data.ProcessId)
            };
            return fileSystemEvent;
        }

        public static string GetProcessOwnerByID(int processId)
        {
            IntPtr processHandle = IntPtr.Zero;
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                processHandle = OpenProcess(PROCESS_QUERY_INFORMATION, false, processId);
                if (processHandle == IntPtr.Zero) return "NO ACCESS";
                OpenProcessToken(processHandle, TOKEN_QUERY, out tokenHandle);
                using (WindowsIdentity wi = new WindowsIdentity(tokenHandle))
                {
                    string user = wi.Name;
                    return user;
                }
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero) CloseHandle(tokenHandle);
                if (processHandle != IntPtr.Zero) CloseHandle(processHandle);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const UInt32 SYNCHRONIZE = 0x00100000;
        private const UInt32 PROCESS_TERMINATE = 0x0001;
        private const UInt32 PROCESS_CREATE_THREAD = 0x0002;
        private const UInt32 PROCESS_SET_SESSIONID = 0x0004;
        private const UInt32 PROCESS_VM_OPERATION = 0x0008;
        private const UInt32 PROCESS_VM_READ = 0x0010;
        private const UInt32 PROCESS_VM_WRITE = 0x0020;
        private const UInt32 PROCESS_DUP_HANDLE = 0x0040;
        private const UInt32 PROCESS_CREATE_PROCESS = 0x0080;
        private const UInt32 PROCESS_SET_QUOTA = 0x0100;
        private const UInt32 PROCESS_SET_INFORMATION = 0x0200;
        private const UInt32 PROCESS_QUERY_INFORMATION = 0x0400;
        private const UInt32 PROCESS_SUSPEND_RESUME = 0x0800;
        private const UInt32 PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;
        private const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        private const UInt32 TOKEN_DUPLICATE = 0x0002;
        private const UInt32 TOKEN_IMPERSONATE = 0x0004;
        private const UInt32 TOKEN_QUERY = 0x0008;
        private const UInt32 TOKEN_QUERY_SOURCE = 0x0010;
        private const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;
        private const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;
        private const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;
    }
}