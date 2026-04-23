using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;

namespace LowCortisol.Web.Auth;

public class AuthService
{
    private const string SessionKey = "lowcortisol.currentUserId";

    private static readonly HashSet<string> AllowedAccountTypes = new()
    {
        "Homeowner",
        "Admin",
        "Technician"
    };

    private readonly AppDbContext _db;
    private readonly ProtectedLocalStorage _storage;

    public AuthService(AppDbContext db, ProtectedLocalStorage storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<(bool Ok, string Error)> RegisterAsync(
        string name,
        string email,
        string phone,
        string password,
        string accountType)
    {
        name = name.Trim();
        email = email.Trim().ToLowerInvariant();
        phone = phone.Trim();
        password = password.Trim();
        accountType = accountType.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return (false, "El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(email))
            return (false, "El correo es obligatorio.");

        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña es obligatoria.");

        if (!AllowedAccountTypes.Contains(accountType))
            return (false, "Tipo de cuenta inválido.");

        var exists = await _db.Users.AnyAsync(x => x.Email == email);
        if (exists)
            return (false, "Ese correo ya está registrado.");

        var user = new AppUser
        {
            Name = name,
            Email = email,
            Phone = phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            AccountType = accountType
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _storage.SetAsync(SessionKey, user.Id);

        return (true, "");
    }

    public async Task<(bool Ok, string Error)> LoginAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();
        password = password.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return (false, "Correo o contraseña inválidos.");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null)
            return (false, "Correo o contraseña inválidos.");

        var valid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!valid)
            return (false, "Correo o contraseña inválidos.");

        await _storage.SetAsync(SessionKey, user.Id);
        return (true, "");
    }

    public async Task LogoutAsync()
    {
        await _storage.DeleteAsync(SessionKey);
    }

    public async Task<AppUser?> GetCurrentUserAsync()
    {
        var result = await _storage.GetAsync<int>(SessionKey);
        if (!result.Success)
            return null;

        var userId = result.Value;
        return await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var user = await GetCurrentUserAsync();
        return user is not null;
    }
}