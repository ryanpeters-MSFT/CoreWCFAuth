namespace WcfService.Repositories
{
    public interface IAuthenticationRepository
    {
        bool Authenticate(string username, string password);
    }
}
