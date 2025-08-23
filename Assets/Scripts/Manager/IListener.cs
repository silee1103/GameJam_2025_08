public interface IListener
{
    void OnEvent(GameEvent evt, object payload);
}