namespace Entities.Abilities.Observer
{
    public interface IObserver
    {
        public bool OnNotify(MCEvent mcEvent);
    }
}
