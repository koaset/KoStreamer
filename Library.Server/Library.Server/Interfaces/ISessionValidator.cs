namespace Library.Server.Interfaces
{
    public interface ISessionValidator
    {
        bool IsValid(string session);
    }
}
