using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Application.Settings;
using Abbanks.TodoApp.Core.Entities;
using Abbanks.TodoApp.Core.Repositories;
using AutoMapper;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IOptions<JwtSettings> jwtSettings,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResultDto> RegisterAsync(RegisterUserDto registerUserDto)
        {
            _logger.LogInformation("Attempting to register user with username: {Username}", registerUserDto.Username);

            // Check if username exists
            if (await _userRepository.UsernameExistsAsync(registerUserDto.Username))
            {
                _logger.LogWarning("Registration failed: Username {Username} already exists", registerUserDto.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Username is already taken" }
                };
            }

            // Check if email exists
            if (await _userRepository.EmailExistsAsync(registerUserDto.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", registerUserDto.Email);
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Email is already registered" }
                };
            }

            // Create new user
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDto.Password);

            var user = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.AddAsync(user);
            _logger.LogInformation("User registered successfully: {UserId}", createdUser.Id);

            // Generate JWT token
            var token = GenerateJwtToken(createdUser);
            _logger.LogInformation("JWT token generated for user: {UserId}", createdUser.Id);

            return new AuthResultDto
            {
                Success = true,
                Token = token,
                User = _mapper.Map<UserDto>(createdUser)
            };
        }

        public async Task<AuthResultDto> LoginAsync(LoginUserDto loginUserDto)
        {
            _logger.LogInformation("Login attempt for username: {Username}", loginUserDto.Username);

            var user = await _userRepository.GetByUsernameAsync(loginUserDto.Username);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Username}", loginUserDto.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid username or password" }
                };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginUserDto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed - invalid password for user: {Username}", loginUserDto.Username);
                return new AuthResultDto
                {
                    Success = false,
                    Errors = new List<string> { "Invalid username or password" }
                };
            }

            _logger.LogInformation("Login successful for user: {UserId}", user.Id);
            var token = GenerateJwtToken(user);
            _logger.LogDebug("JWT token generated for user: {UserId}", user.Id);

            return new AuthResultDto
            {
                Success = true,
                Token = token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving user details for ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                return null;
            }

            _logger.LogInformation("User details retrieved successfully for: {UserId}", userId);
            return _mapper.Map<UserDto>(user);
        }

        private string GenerateJwtToken(User user)
        {
            _logger.LogDebug("Generating JWT token for user: {UserId}", user.Id);

            if (string.IsNullOrEmpty(_jwtSettings.Secret))
            {
                _logger.LogError("JWT token generation failed: JWT Secret is empty or null");
                throw new InvalidOperationException("JWT Secret is not configured");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                IssuedAt = now,
                Expires = now.AddDays(_jwtSettings.ExpirationDays),
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