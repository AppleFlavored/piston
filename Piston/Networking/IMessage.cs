namespace Piston.Networking;

public interface IMessage<out T>
{
    static abstract T Read();
    void Write();
}