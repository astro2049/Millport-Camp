namespace Observer
{
    public interface IObserver
    {
        public bool OnNotify(MCEvent mcEvent);
    }
}