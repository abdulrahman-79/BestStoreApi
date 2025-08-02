using BestStoreApi.Models;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BestStoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _Configuration;
        private readonly ApplicationDbContext _applicationDbContext;

        public AccountController(IConfiguration configuration, ApplicationDbContext applicationDbContext)
        {
            this._Configuration = configuration;
            this._applicationDbContext = applicationDbContext;
        }

        //[HttpGet("TestToken")]
        //public IActionResult TestToken()
        //{
        //    User user = new User()
        //    {
        //        Id = 2,
        //        Role = "admin"
        //    };
        //    string jwt = CreateJWTToken(user);
        //    var response = new { JWToken = jwt };
        //    return Ok(response);
        //}

        [HttpPost("Register")]
        public IActionResult Register(UserDto userDto)
        {
            var emailCount = _applicationDbContext.Users.Count(u => u.Email == userDto.Email);
            if (emailCount > 0)
            {
                ModelState.AddModelError("Email", "This Email address is already used");
                return BadRequest(ModelState);
            }


            var passwordHasher = new PasswordHasher<User>();
            var encryptedPassword = passwordHasher.HashPassword(new User(), userDto.Password);

            User user = new User()
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Phone = userDto.Phone ?? "",
                Password = encryptedPassword,
                Role = "client",
                CreatedAt = DateTime.Now
            };
            _applicationDbContext.Users.Add(user);
            _applicationDbContext.SaveChanges();

            var jwt = CreateJWTToken(user);

            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);
        }


        [HttpPost("Login")]
        public IActionResult Login(string email, string password)
        {
            var user = _applicationDbContext.Users.FirstOrDefault(u => u.Email == email);
            
            if(user == null)
            {
                ModelState.AddModelError("Error", "Email or Password not Valid");
                return BadRequest(ModelState);
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(new User(), user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("Error", "Email or Password not Valid");
                return BadRequest(ModelState);
            }

            var jwt = CreateJWTToken(user);
            UserProfileDto userProfileDto = new UserProfileDto()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            var response = new
            {
                Token = jwt,
                User = userProfileDto
            };

            return Ok(response);

        }

        //[Authorize]
        //[HttpGet("GetTokenClaims")]
        //public IActionResult GetTokenClaims()
        //{
        //    var identity = User.Identity as ClaimsIdentity;
        //    if (identity != null)
        //    {
        //        Dictionary<string, string> claims = new Dictionary<string, string>();
        //        foreach (Claim claim in identity.Claims)
        //        {
        //            claims.Add(claim.Type, claim.Value);
        //        }
        //        return Ok(claims);
        //    }
        //    return Ok();
        //}

        //[Authorize] 
        //[HttpGet("AuthorizeAuthenticatedUsers")]
        //public IActionResult AuthorizeAuthenticatedUsers()
        //{
        //    return Ok("You are authorized");
        //}

        //[Authorize(Roles = "admin")]
        //[HttpGet("AuthorizeAdmin")]
        //public IActionResult AuthorizeAdmin()
        //{
        //    return Ok("You are authorized");
        //}

        //[Authorize(Roles = "admin, seller")]
        //[HttpGet("AuthorizeAdminAndSellers")]
        //public IActionResult AuthorizeAdminAndSellers()
        //{
        //    return Ok("You are authorized");
        //}

        private string CreateJWTToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim("id", "" + user.Id),
                new Claim("role", user.Role)
            };

            string strKey = _Configuration["JwtSettings:Key"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(strKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var token = new JwtSecurityToken(
                issuer: _Configuration["JwtSettings:Issuer"],
                audience: _Configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

    }
}
