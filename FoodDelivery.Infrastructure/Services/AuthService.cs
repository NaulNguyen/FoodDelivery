using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FoodDelivery.Application.Common;
using FoodDelivery.Application.DTOs;
using FoodDelivery.Application.Interfaces;
using FoodDelivery.Domain.Constants;
using FoodDelivery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FoodDelivery.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager, 
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // 1. Danh sách các Role được phép đăng ký công khai
        var publicRoles = new List<string> { AppRoles.Customer, AppRoles.Driver, AppRoles.RestaurantOwner };

        // 2. Kiểm tra xem Role gửi lên có hợp lệ không
        if (!publicRoles.Contains(request.Role))
        {
            return Result<AuthResponse>.Failure($"Đăng ký vai trò '{request.Role}' không được phép.");
        }

        // 3. Tạo đối tượng User mới từ model
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Username,
            Role = request.Role
        };

        // 4. Dùng UserManager để tạo User và băm mật khẩu
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description));
        }

        // 5. Kiểm tra xem vai trò đã tồn tại chưa
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            // Nếu chưa có, hãy tạo mới vai trò đó
            await _roleManager.CreateAsync(new IdentityRole<Guid>(request.Role));
        }

        // 6. Sau đó mới an tâm gán vai trò cho User
        await _userManager.AddToRoleAsync(user, request.Role);

        var token = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Username = user.UserName!,
            Role = user.Role
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Tìm user theo Email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null) 
            return Result<AuthResponse>.Failure("Email không tồn tại trong hệ thống.");

        // 2. Kiểm tra mật khẩu
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Result<AuthResponse>.Failure("Mật khẩu không chính xác.");

        var token = await GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            Username = user.UserName!,
            Role = user.Role
        });
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
            return Result<AuthResponse>.Failure("Token không hợp lệ.");

        var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return Result<AuthResponse>.Failure("Token không hợp lệ.");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Refresh token không hợp lệ hoặc đã hết hạn.");

        var newAccessToken = await GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Username = user.UserName!,
            Role = user.Role
        });
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false // Khác biệt quan trọng: không kiểm tra lifetime vì ta đang muốn làm mới token đã hết hạn
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        // 1. Lấy danh sách Roles của User từ DB
        var userRoles = await _userManager.GetRolesAsync(user);

        // 2. Tạo danh sách Claims
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 3. Đưa từng Role vào Claims
        foreach (var role in userRoles)
        {
            authClaims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 4. Ký tên và tạo Token (sử dụng Secret Key)
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddMinutes(15), // Access token hết hạn nhanh (ví dụ 15 phút)
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
