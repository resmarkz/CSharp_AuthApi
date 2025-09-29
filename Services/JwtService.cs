using AuthApi.Data;
using AuthApi.DTOs;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.Services
{
    public class JwtService
    {
        private readonly AuthAPIContext _dbContext;
        private readonly IConfiguration _configuration;

        public JwtService(AuthAPIContext dbContext, IConfiguration configuration) 
        { 
            _dbContext = dbContext;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> Authenticate(LoginDto request)
        {
            var userAccount = await _dbContext.Users.FirstOrDefaultAsync(x => x.Username == request.Username);

            if (userAccount == null || !BCrypt.Net.BCrypt.Verify(request.Password, userAccount.Password))
            {
                return null;
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, userAccount.Id.ToString()),
        new Claim(ClaimTypes.Name, userAccount.Username),
        new Claim(ClaimTypes.Email, userAccount.Email ?? "")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["JwtConfig:Issuer"],
                Audience = _configuration["JwtConfig:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new AuthResponseDto
            {
                Username = userAccount.Username,
                Email = userAccount.Email,
                Token = tokenString
            };
        }

        public async Task<AuthResponseDto?> Register(RegisterDto newUser)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Username == newUser.Username))
            {
                throw new Exception("Username already taken.");
            }

            var user = new User
            {
                Username = newUser.Username,
                Email = newUser.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                Role = string.IsNullOrEmpty(newUser.Role) ? "user" : newUser.Role
            };

            try
            {
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }

            return new AuthResponseDto
            {
                Username = user.Username,
                Email = user.Email,
                Token = ""
            };
        }


    }
}
