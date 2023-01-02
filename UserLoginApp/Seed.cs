using UserLoginApp.Data;
using UserLoginApp.Models;

namespace UserLoginApp
{
    public class Seed
    {
        private readonly DataContext dataContext;
        public Seed(DataContext context)
        {
            this.dataContext = context;
        }

        public void SeedDataContext()
        {
            /*  if (!dataContext.Users.Any()) 
              {
                  var account = new List<User>()
                  {
                      new User()
                      {
                          UserID= 1,
                          UserName="thinh",
                          Password="thinh123",
                          Point=0
                      },
                      new User()
                      {
                          UserID= 2,
                          UserName="beo",
                          Password="thinh123",
                          Point=0
                      }
                  };

                  dataContext.Users.AddRange(account);
                  dataContext.SaveChanges();
              }*/
            var account = new List<User>()
            {
                new User()
                {
                   UserID= 1,
                   UserName="thinh",
                   Password="thinh123",
                   Point=0
                },
                new User()
                {
                   UserID= 2,
                   UserName="beo",
                   Password="thinh123",
                   Point=0
                }
            };

            dataContext.Users.AddRange(account);
            dataContext.SaveChanges();
        }
    }
}
