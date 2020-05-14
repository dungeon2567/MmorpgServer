namespace MmorpgServer
{
    public delegate void PropertyChangedHandler<TOwner, T>(TOwner owner, T from, T to) where TOwner: GameObject;
    public delegate void PropertyChangedHandlerFast<TOwner, T>(TOwner owner, in T from, in T to) where TOwner: GameObject;
}