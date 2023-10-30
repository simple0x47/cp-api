using System.Configuration;

namespace Organization.Config;

public class ConfigurationReader
{
    public static readonly string DatabaseKey = "Database";

    private readonly IConfiguration _configuration;

    public ConfigurationReader(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public double GetDoubleOrDefault(string key, double defaultValue)
    {
        try
        {
            return double.Parse(_configuration[key]);
        }
        catch (Exception e)
        {
            return defaultValue;
        }
    }

    public string GetStringOrDefault(string key, string defaultValue)
    {
        string? value = _configuration[key];

        if (value is null) return defaultValue;

        return value;
    }

    public string GetStringOrThrowException(string key)
    {
        string? value = _configuration[key];

        if (value is null) throw new ConfigurationErrorsException($"missing '{key}' configuration");

        return value;
    }
}