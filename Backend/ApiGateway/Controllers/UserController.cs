using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UserManagementService;

namespace ApiGateway.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            // Ocelot sẽ định tuyến request này dựa trên cấu hình ocelot.json
            var response = await client.PostAsJsonAsync("/users", request); // Đường dẫn tương đối
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode); // Trả về mã trạng thái từ UserManagementService
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Ok(registerResponse);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var client = _httpClientFactory.CreateClient();
            // Ocelot sẽ định tuyến request này dựa trên cấu hình ocelot.json
            var response = await client.PostAsJsonAsync("/users/login", request); // Đường dẫn tương đối
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Ok(loginResponse);
        }
    }
}