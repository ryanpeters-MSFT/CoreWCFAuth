namespace WcfService.Repositories
{
    public class MockAuthenticationRepository : IAuthenticationRepository
    {
        public bool Authenticate(string username, string password) => username == "ryan" && password == "123456";
    }
}
