using Restaurant.Models;

namespace Restaurant.Services
{
    public class SessionService
    {
        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser != null;
        public bool IsEmployee => CurrentUser?.Role == "Angajat";

        public void SignIn(User user) => CurrentUser = user;
        public void SignOut() => CurrentUser = null;
    }
}
