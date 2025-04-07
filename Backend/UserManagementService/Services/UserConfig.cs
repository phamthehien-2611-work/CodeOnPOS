using Grpc.Core;
using UserManagementService.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace UserManagementService
{
    public class UserConfig : UserService.UserServiceBase
    {
        private readonly UserManagementContext _context;
        private readonly IConfiguration _config;

        public UserConfig(UserManagementContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public override async Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

                tbl_User newUser = new tbl_User
                {
                    UserId = Guid.NewGuid(),
                    UserName = request.Username,
                    Password = hashedPassword,
                };

                _context.tbl_Users.Add(newUser);
                await _context.SaveChangesAsync();

                return new RegisterResponse { Success = true, Message = "Đăng ký thành công" };
            }
            catch (Exception ex)
            {
                return new RegisterResponse { Success = false, Message = ex.Message };
            }
        }

        public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            tbl_User user = await _context.tbl_Users.FirstOrDefaultAsync(u => u.UserName == request.Username);

            if (user == null)
            {
                return new LoginResponse { Success = false, Message = "Tên đăng nhập hoặc mật khẩu không đúng" };
            }

            bool verified = BCrypt.Net.BCrypt.Verify(request.Password, user.Password); // Xác minh password

            if (!verified)
            {
                return new LoginResponse { Success = false, Message = "Tên đăng nhập hoặc mật khẩu không đúng" };
            }

            // Nếu xác minh thành công, tạo JWT token
            var token = GenerateJwtToken(user);
            return new LoginResponse { Success = true, Message = "Đăng nhập thành công", Token = token };
        }

        private string GenerateJwtToken(tbl_User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}