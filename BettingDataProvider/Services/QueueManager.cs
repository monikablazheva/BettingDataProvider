namespace BettingDataProvider.Services
{
    public static class QueueManager
    {
        private static Queue<string> _myQueue = new Queue<string>();

        public static Queue<string> MyQueue
        {
            get { return _myQueue; }
        }

        public static void AddItem(string item)
        {
            _myQueue.Enqueue(item);
        }
    }
}
