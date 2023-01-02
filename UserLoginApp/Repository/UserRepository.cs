using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using UserLoginApp.Data;
using UserLoginApp.Interface;
using UserLoginApp.Models;

namespace UserLoginApp.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context) 
        {
            _context= context;
        }

        public ICollection<User> GetUsers() 
        {
            return _context.Users.OrderBy(p=>p.UserID).ToList();
        }

        public User GetUser(int userid) 
        {
            return _context.Users.Where(p => p.UserID == userid).FirstOrDefault();
        }

        public User GetUserByName(string username)
        {
            return _context.Users.Where(p => p.UserName == username).FirstOrDefault();
        }

        public Boolean UserExists(int userid)
        {
            return _context.Users.Any(p => p.UserID == userid);
        }

        public Boolean UserNameExists(string username)
        {
            return _context.Users.Any(p => p.UserName == username);
        }

        public int GetPoint(int userid)
        {
            var sqlResult = _context.Users.Where(p => p.UserID == userid).FirstOrDefault();
            Console.WriteLine(sqlResult);
            return sqlResult.Point;
        }

        public Boolean CreateUser(User user) 
        {
            //  int max = _context.Users.Select(p => p.UserID).DefaultIfEmpty(0).Max();
            // _context.Users.Add(new User() {UserID = 5,UserName = "beothin",Password = "123",Point = 0 });
            _context.Users.Add(user);
            return Save();
        }

        public Boolean Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public Boolean UpdateUserPoint(User UserUpdate)
        {
            _context.Users.Update(UserUpdate);
            return Save();
        }

        public Boolean DeleteUser(User UserDelete)
        {
            _context.Users.Remove(UserDelete);
            return Save();
        }

    }
}
