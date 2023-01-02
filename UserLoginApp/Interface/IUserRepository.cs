using UserLoginApp.Models;

namespace UserLoginApp.Interface
{
    public interface IUserRepository
    {
        ICollection<User> GetUsers();

        User GetUser(int userid);

       int GetPoint(int userid);

        Boolean UserExists(int userid);

        Boolean CreateUser(User user);

        Boolean UserNameExists(string username);

        User GetUserByName(string username);

        Boolean UpdateUserPoint(User UserUpdate);
        Boolean Save();

        Boolean DeleteUser(User user);
    }
}
