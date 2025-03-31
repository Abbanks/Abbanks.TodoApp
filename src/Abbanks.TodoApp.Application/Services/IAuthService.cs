using Abbanks.TodoApp.Application.DTOs;

namespace Abbanks.TodoApp.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterUserDto registerUserDto);
        Task<AuthResultDto> LoginAsync(LoginUserDto loginUserDto);
        Task<UserDto?> GetUserByIdAsync(Guid userId);
    }
}
