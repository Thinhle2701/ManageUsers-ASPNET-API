using UserLoginApp.Interface;
using Microsoft.AspNetCore.Mvc;
using UserLoginApp.Models;
using UserLoginApp.Data;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace UserLoginApp.Controllers
{
    public class BodyUpdatePoint
    {
        public int point { get; set; }
    }

    public class BodyLogin
    {
        public string username { get; set; }
        public string Password { get; set; }
    }


    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly DataContext context;
        private readonly IConfiguration _congfiguration;

        public UserController(IUserRepository userRespository,DataContext context,IConfiguration configuration) 
        {
            _userRepository = userRespository;
            this.context = context;
            _congfiguration = configuration;
        }

        [HttpGet]
        [ProducesResponseType(200,Type = typeof(IEnumerable<User>))]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }
            return Ok(users);
        }

        [HttpGet("{userid}")]
        [ProducesResponseType(200, Type= typeof(User))]
        [ProducesResponseType(400)]
        public IActionResult GetUser(int userid) 
        {
            if (!_userRepository.UserExists(userid))
            {
                return NotFound();
            }

            var user = _userRepository.GetUser(userid);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(user);

        }

        [HttpGet("{userid}/point")]
        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400)]
        public IActionResult GetPoint(int userid)
        {
            if (!_userRepository.UserExists(userid))
            {
                var message = string.Format("User with id = {0} not found", userid);
                return NotFound(new { message = message,status = "404",success="fail" });
            }

            var user = _userRepository.GetPoint(userid);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(new {Point = user});

        }


        [HttpPost,AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateUser([FromBody] User userCreate) 
        {
           if (userCreate == null) 
            {
                return BadRequest(ModelState);
            }

            if (_userRepository.UserNameExists(userCreate.UserName))
            {
                return NotFound(new { message = "username already exists", status = "404", success = "fail" });
            }

            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            int count = context.Users.Count();
            if (count == 0) 
            {
                userCreate.UserID= 1;
            }
            else
            {
                int max = context.Users.Max(u => u.UserID);
                userCreate.UserID = max + 1;
            }

            userCreate.Point = 0;
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userCreate.Password);
            userCreate.Password = passwordHash;
            userCreate.UserRole = "user";

            if (!_userRepository.CreateUser(userCreate))
            {
                ModelState.AddModelError("", "Can not save data");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully");
        }

        [HttpPost("/createadmin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult CreateAdmin([FromBody] User userCreate)
        {
            var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(_bearer_token);
            var tokenS = jsonToken as JwtSecurityToken;
            var role = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            if (role == "user")
            {
                return BadRequest("You do not have privilleges");
            }
            if (userCreate == null)
            {
                return BadRequest(ModelState);
            }

            if (_userRepository.UserNameExists(userCreate.UserName))
            {
                return NotFound(new { message = "username already exists", status = "404", success = "fail" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int count = context.Users.Count();
            if (count == 0)
            {
                userCreate.UserID = 1;
            }
            else
            {
                int max = context.Users.Max(u => u.UserID);
                userCreate.UserID = max + 1;
            }

            userCreate.Point = 0;
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userCreate.Password);
            userCreate.Password = passwordHash;
            userCreate.UserRole = "admin";

            if (!_userRepository.CreateUser(userCreate))
            {
                ModelState.AddModelError("", "Can not save data");
                return StatusCode(500, ModelState);
            }

            return Ok("Successfully");
        }


        [HttpPost("login"), AllowAnonymous]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public IActionResult Login([FromBody] BodyLogin account)
        {
            if (account.username == null)
            {
                return BadRequest(ModelState);
            }

            if (account.Password == null)
            {
                return BadRequest(ModelState);
            }

            if (!_userRepository.UserNameExists(account.username))
            {
                return NotFound(new { message = "Invalid Username or Password", status = "404", success = "fail" });
            }

            var user = _userRepository.GetUserByName(account.username);
            bool verified = BCrypt.Net.BCrypt.Verify(account.Password, user.Password);
            if (!verified)
            {
                return NotFound(new { message = "Invalid Username or Password", status = "404", success = "fail" });
            }

            string token = CreateToken(user);

            return Ok(new { UserData = user,Token = token });
        }

        private string CreateToken(User user)
        {
            var claims = new[] {
          new Claim(JwtRegisteredClaimNames.Name, user.UserName),
          new Claim(JwtRegisteredClaimNames.Sub, user.UserRole)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_congfiguration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

        [HttpPut("{UserID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult UpdatePoint(int UserID, [FromBody] BodyUpdatePoint UpdateData)
        {
            //Authorization Admin
            var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(_bearer_token);
            var tokenS = jsonToken as JwtSecurityToken;
            var role = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            if (role == "user")
            {
                return BadRequest("You do not have privilleges");
            }
            if (UpdateData == null)
            {
                return BadRequest();
            }

            if (!_userRepository.UserExists(UserID))
            {
                return NotFound("Your Account does not exist");
            }

            var User = _userRepository.GetUser(UserID);
            User.Point = UpdateData.point;

            if (!_userRepository.UpdateUserPoint(User))
            {
                ModelState.AddModelError("", "Can not save data");
                return StatusCode(500, ModelState);
            }
            return Ok(User);
        }

        [HttpDelete("{UserID}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult DeleteUser(int UserID)
        {
            //Authorization Admin
            var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(_bearer_token);
            var tokenS = jsonToken as JwtSecurityToken;
            var role = tokenS.Claims.First(claim => claim.Type == "sub").Value;

            if (role == "user")
            {
                return BadRequest("You do not have privilleges");
            }

            if (!_userRepository.UserExists(UserID))
            {
                return NotFound("Your UserID does not exist");
            }

            var user = _userRepository.GetUser(UserID);
            if (!_userRepository.DeleteUser(user))
            {
                ModelState.AddModelError("", "Something went wrong");
                return StatusCode(500,ModelState);
            }
            var message = "Delete Successfully user have UserID: " + UserID;

            return Ok(message);
        }

    }
}
