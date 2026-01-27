using System.Reflection;
using Echoes;

namespace HASS.Agent.Translations;

public static class TranslationHelper
{
    public const string UnknownTranslationUnit = "UNKNOWN TRANSLATION UNIT";
    
    public static string Get(string key, string prefix, string suffix)
    {
        var translationUnit = typeof(Translations.Strings)
            .GetProperty($"{prefix}{key}{suffix}", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null) as TranslationUnit;
        
        return translationUnit == null ? UnknownTranslationUnit : translationUnit.CurrentValue;
    }
}