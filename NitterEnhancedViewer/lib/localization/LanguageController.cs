using System.Globalization;
using System.Reflection;
using NitterEnhancedViewer.lib.fs;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace NitterEnhancedViewer.lib.localization;

public class LanguageController
{
    public static T GetLocalizedValue<T>(string key)
    {
        return LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Language:" + key);
    }
    
    public static void UpdateLanguage(string language)
    {
        if (string.IsNullOrEmpty(language) || language == "en-US" || language == "Default")
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.InvariantCulture;
        }
        else
        {
            LocalizeDictionary.Instance.Culture = new CultureInfo(language);
        }
    }

    public static async ValueTask Initialize()
    {
        var json = await JsonController.LoadSettings();
        if (json.Language != null) UpdateLanguage(json.Language);
    }
}