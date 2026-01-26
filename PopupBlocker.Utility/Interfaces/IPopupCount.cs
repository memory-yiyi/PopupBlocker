namespace PopupBlocker.Utility.Interfaces
{
    public interface IPopupCount
    {
        public long BlockedCount { get; }

        public void ResetBlockCount();
        public void AddBlockCount();
    }
}
