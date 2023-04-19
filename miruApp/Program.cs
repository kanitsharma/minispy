using CenterDevice.MiniFSWatcher;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CenterDevice.MiniFSWatcherApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = (args.Length > 0) ? args[0] : AppDomain.CurrentDomain.BaseDirectory;

            Console.WriteLine("FileSystemEventFilter Demo Application");
            Console.WriteLine("======================================\n");
            Console.WriteLine("Listing events in \"" + path + "\"");
            Console.WriteLine("To watch a different path, run: " + AppDomain.CurrentDomain.FriendlyName + " <pattern>\n");

            //CaptureEventsUsingDefaultWatcher(path);
            CaptureEventsUsingFilter(path);

            Console.WriteLine("Press <ESC> to stop exit...");
            do
            {
                while (!Console.KeyAvailable)
                {
                    Task.Delay(200).Wait();
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            Console.WriteLine("Done");
        }

        private static void CaptureEventsUsingDefaultWatcher(string path)
        {
            Console.WriteLine("Capturing events using default file system watcher...");

            var watcher = new FileSystemWatcher(path);
            watcher.Changed += (sender, e) =>
            {
                Console.WriteLine("Changed: " + e.FullPath);
            };
            watcher.Created += (sender, e) =>
            {
                Console.WriteLine("Created: " + e.FullPath);
            };
            watcher.Renamed += (sender, e) =>
            {
                Console.WriteLine("Renamed: " + e.OldFullPath + " -> " + e.FullPath);
            };
            watcher.Deleted += (sender, e) =>
            {
                Console.WriteLine("Deleted: " + e.FullPath);
            };

            watcher.EnableRaisingEvents = true;
        }

        private static void CaptureEventsUsingFilter(string path)
        {
            var filter = new EventWatcher();


            filter.Connect();
            filter.AggregateEvents = true;
            filter.NotWatchProcess(EventWatcher.GetCurrentProcessId());
        
            filter.WatchPath(path + "*");

            filter.OnChange += (name, process, account) =>
            {
                Console.WriteLine("Changed: " + name + " Account: " + account);
            };
            filter.OnCreate += (name, process, account) =>
            {
                Console.WriteLine("Created: " + name + " Account: " + account);
            };
            filter.OnDelete += (name, process, account) =>
            {
                Console.WriteLine("Deleted: " + name + " Account: " + account);
            };
            filter.OnRenameOrMove += (name, oldName, process, account) =>
            {
                Console.WriteLine("Moved: " + oldName + " -> " + name + " Account: " + account);
            };
        }
    }
}
