using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace FitTrack.Api.Auth;

/// <summary>
/// Run with: dotnet run --project FitTrack.Api --launch-profile https -- --setup-auth
/// Asks for a demo user name and password, then writes a signing key and a PASSWORD HASH
/// (not the password) into User Secrets. Nothing here is ever committed to Git.
/// </summary>
public static class AuthSetup
{
    private const int Iterations = 100_000;

    public static void Run()
    {
        var secretsId = typeof(AuthSetup).Assembly.GetCustomAttribute<UserSecretsIdAttribute>()?.UserSecretsId;
        if (string.IsNullOrEmpty(secretsId))
        {
            Console.WriteLine("The project has no UserSecretsId. Run: dotnet user-secrets init --project FitTrack.Api");
            return;
        }

        Console.WriteLine("FitTrack.Api authentication setup");

        Console.Write("User name: ");
        var username = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("A user name is required. Nothing was saved.");
            return;
        }

        var password = ReadPassword("Password (8 or more characters): ");
        if (password.Length < 8)
        {
            Console.WriteLine("The password must be at least 8 characters. Nothing was saved.");
            return;
        }

        if (password != ReadPassword("Confirm password: "))
        {
            Console.WriteLine("The passwords did not match. Nothing was saved.");
            return;
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        var passwordHash = $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

        var path = GetSecretsPath(secretsId);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var secrets = File.Exists(path) && !string.IsNullOrWhiteSpace(File.ReadAllText(path))
            ? JsonNode.Parse(File.ReadAllText(path))!.AsObject()
            : new JsonObject();

        // Keep an existing signing key so tokens already issued stay valid.
        if (string.IsNullOrEmpty(secrets["Authentication:Jwt:SigningKey"]?.GetValue<string>()))
            secrets["Authentication:Jwt:SigningKey"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        secrets["Authentication:User:Username"] = username;
        secrets["Authentication:User:PasswordHash"] = passwordHash;

        File.WriteAllText(path, secrets.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine();
        Console.WriteLine("Saved to User Secrets. Next: set \"Enabled\": true in the Authentication section of appsettings.Development.json.");
    }

    private static string GetSecretsPath(string secretsId)
    {
        var root = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetEnvironmentVariable("APPDATA") ?? string.Empty, "Microsoft", "UserSecrets")
            : Path.Combine(Environment.GetEnvironmentVariable("HOME") ?? string.Empty, ".microsoft", "usersecrets");

        return Path.Combine(root, secretsId, "secrets.json");
    }

    private static string ReadPassword(string prompt)
    {
        Console.Write(prompt);

        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? string.Empty;

        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(chars.ToArray());
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (chars.Count > 0)
                    chars.RemoveAt(chars.Count - 1);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                chars.Add(key.KeyChar);
            }
        }
    }
}
