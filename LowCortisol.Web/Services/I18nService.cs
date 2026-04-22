using System.Text.Json;

namespace LowCortisol.Web.Services;

public class I18nService
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private Dictionary<string, string> _strings = new();

    public string CurrentLanguage { get; private set; } = "es";

    public event Action? OnChange;

    public I18nService(IWebHostEnvironment env)
    {
        _translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["es"] = LoadLanguage(env, "es"),
            ["en"] = LoadLanguage(env, "en"),
            ["pt"] = LoadLanguage(env, "pt")
        };

        _strings = _translations["es"];
        CurrentLanguage = "es";
    }

    public Task InitializeAsync(string language = "es")
    {
        SetLanguage(language);
        return Task.CompletedTask;
    }

    public Task SetLanguageAsync(string language)
    {
        SetLanguage(language);
        return Task.CompletedTask;
    }

    private void SetLanguage(string language)
    {
        language = Normalize(language);

        if (_translations.TryGetValue(language, out var dict) && dict.Count > 0)
        {
            _strings = dict;
            CurrentLanguage = language;
        }
        else
        {
            _strings = _translations["es"];
            CurrentLanguage = "es";
        }

        OnChange?.Invoke();
    }

    private static Dictionary<string, string> LoadLanguage(IWebHostEnvironment env, string language)
    {
        var filePath = Path.Combine(env.WebRootPath, "i18n", $"{language}.json");

        if (!File.Exists(filePath))
            return new Dictionary<string, string>();

        var json = File.ReadAllText(filePath);

        return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
               ?? new Dictionary<string, string>();
    }

    public string this[string key]
    {
        get
        {
            if (_strings.TryGetValue(key, out var value))
                return value;

            return key;
        }
    }

    private static string Normalize(string? language)
    {
        return language?.ToLowerInvariant() switch
        {
            "en" => "en",
            "pt" => "pt",
            _ => "es"
        };
    }
}