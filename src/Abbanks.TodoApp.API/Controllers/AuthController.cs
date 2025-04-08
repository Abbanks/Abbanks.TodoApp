using Abbanks.TodoApp.Application.DTOs;
using Abbanks.TodoApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Abbanks.TodoApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="registerUserDto">User registration details</param>
        /// <returns>Authentication result with JWT token</returns>
        /// <response code="200">Returns the authentication result with token</response>
        /// <response code="400">If the registration data is invalid</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerUserDto)
        {
            try
            {
                _logger.LogInformation("Processing registration request for username: {Username}", registerUserDto.Username);

                var result = await _authService.RegisterAsync(registerUserDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Registration failed for username: {Username}, Errors: {@Errors}",
                        registerUserDto.Username, result.Errors);
                    return BadRequest(result);
                }

                _logger.LogInformation("Successfully registered user: {Username}, UserId: {UserId}",
                    registerUserDto.Username, result.User?.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for username: {Username}", registerUserDto.Username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Success = false, Errors = new[] { "An unexpected error occurred during registration" } });
            }
        }

        /// <summary>
        /// Authenticates a user
        /// </summary>
        /// <param name="loginUserDto">User login credentials</param>
        /// <returns>Authentication result with JWT token</returns>
        /// <response code="200">Returns the authentication result with token</response>
        /// <response code="400">If the login credentials are invalid</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                _logger.LogInformation("Processing login request for username: {Username}", loginUserDto.Username);

                var result = await _authService.LoginAsync(loginUserDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Login failed for username: {Username}, Errors: {@Errors}",
                        loginUserDto.Username, result.Errors);
                    return BadRequest(result);
                }

                _logger.LogInformation("Successful login for username: {Username}, UserId: {UserId}",
                    loginUserDto.Username, result.User?.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login for username: {Username}", loginUserDto.Username);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Success = false, Errors = new[] { "An unexpected error occurred during login" } });
            }
        }

        /// <summary>
        /// Gets the current authenticated user's profile
        /// </summary>
        /// <returns>User profile information</returns>
        /// <response code="200">Returns the user profile</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the user profile is not found</response>
        /// <response code="500">If an unexpected error occurs</response>
        [HttpGet("current")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("User ID claim missing from token");
                    return Unauthorized(new { message = "User identity not found" });
                }

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    _logger.LogWarning("Invalid user ID format in token: {UserIdClaim}", userIdClaim);
                    return Unauthorized(new { message = "Invalid user identity format" });
                }

                _logger.LogInformation("Retrieving profile for user ID: {UserId}", userId);

                var user = await _authService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning("User profile not found for ID: {UserId}", userId);
                    return NotFound(new { message = "User profile not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An unexpected error occurred while retrieving user profile" });
            }
        }
    }
}
