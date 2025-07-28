using BestStoreApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public AccountController(IConfiguration configuration)
        {
            this._Configuration = configuration;
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
