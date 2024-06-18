namespace Observer
{
    public interface IObserver
    {
        public bool OnNotify(EventType mcEvent);
    }
}
