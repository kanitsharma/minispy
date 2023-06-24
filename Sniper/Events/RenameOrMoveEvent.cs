namespace Sniper.Events
{
    public class RenameOrMoveEvent: FileSystemEvent
    {
        public string OldFilename { get; internal set; }
    }
}
