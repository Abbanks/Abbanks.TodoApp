using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Application.Settings;
using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Repositories;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Abbanks.TodoApp.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterUserDto registerUserDto)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);

            var user = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);

            var token = GenerateJwtToken(createdUser);

            return new AuthResultDto
            {
                Success = true,
                Token = token,
                User = _mapper.Map<UserDto>(createdUser)
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginUserDto loginUserDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginUserDto.Username);

            if (user == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid username or password" }
                };
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid username or password" }
                };
            }

            var token = GenerateJwtToken(user);

            return new AuthResultDto
            {
                Success = true,
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
