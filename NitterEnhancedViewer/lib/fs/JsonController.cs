using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;
using NitterEnhancedViewer.lib.gui;

namespace NitterEnhancedViewer.lib.fs;


public class SettingJson
{
   public string? Xusername { get; set; } = String.Empty;
   public string? Xpassword { get; set; } = String.Empty;
   public bool StartMinimized { get; set; } = true;
   
   public ThemeController.Theme Theme { get; set; } = ThemeController.Theme.Light;

   public bool MemoryTab { get; set; }
   [JsonConverter(typeof(ColorJsonConverter))]
   public Color DockColor { get; set; } = Colors.Gray;
   
   public string? Language { get; set; } = "en-US";
}

public class NitterData
{
   public string? Url { get; set; }
   public DateTime FavoriteTime { get; set; }
   public bool IsFavorite { get; set; }
}

public abstract class JsonController
{
   
   internal static async ValueTask <SettingJson>  LoadSettings()
   {
      if (!File.Exists("settings.json"))
      {
         await  SaveSettings(new SettingJson());
      }

      return JsonSerializer.Deserialize<SettingJson>(await File.ReadAllTextAsync("settings.json")) ?? throw new InvalidOperationException();
   }
   
   internal static async Task<List<NitterData>> LoadNitterData()
   {
      return JsonSerializer.Deserialize<List<NitterData>>(await File.ReadAllTextAsync("data/nitterdata.json")) ?? throw new InvalidOperationException();
   }
   
   public static async ValueTask CheckNullForSettingsData (SettingJson? settings)
   {
      if (settings == null)
      {
         await SaveSettings(new SettingJson());
         return;
      }
      
      var jsonString = await File.ReadAllTextAsync("settings.json");
      using var document = JsonDocument.Parse(jsonString);
      var root = document.RootElement;

      var defaultSettings = new SettingJson();
      var modified = false;

      if (!root.TryGetProperty("Xusername", out _))
      {
         settings.Xusername = defaultSettings.Xusername;
         modified = true;
      }

      if (!root.TryGetProperty("Xpassword", out _))
      {
         settings.Xpassword = defaultSettings.Xpassword;
         modified = true;
      }

      if (!root.TryGetProperty("StartMinimized", out _))
      {
         settings.StartMinimized = defaultSettings.StartMinimized;
         modified = true;
      }
    
      if (!root.TryGetProperty("Theme", out _))
      {
         settings.Theme = defaultSettings.Theme;
         modified = true;
      }

      if (!root.TryGetProperty("DockColor", out _))
      {
         settings.DockColor = defaultSettings.DockColor;
         modified = true;
      }
      
      if (!root.TryGetProperty("Language", out _))
      {
         settings.Language = defaultSettings.Language;
         modified = true;
      }
      
      if (!root.TryGetProperty("MemoryTab", out _))
      {
         settings.MemoryTab = defaultSettings.MemoryTab;
         modified = true;
      }

      if (modified)
      {
         await SaveSettings(settings);
      }
   }
   
   public static async ValueTask CheckNullForNitterData(List<NitterData> nitterData)
   {
      if (nitterData == null)
      {
         await SaveNitterData(new List<NitterData>());
         return;
      }
      
      var jsonString = await File.ReadAllTextAsync("data/nitterdata.json");
      using var document = JsonDocument.Parse(jsonString);
      var root = document.RootElement;

      var defaultNitterData = new List<NitterData>();
      var modified = false;

      if (!root.EnumerateArray().Any())
      {
         nitterData.AddRange(defaultNitterData);
         modified = true;
      }

      if (modified)
      {
         await SaveNitterData(nitterData);
      }
   }
   
   public static async ValueTask  SaveSettings(SettingJson settings)
   {
      var options = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
         WriteIndented = true
      };
      await File.WriteAllTextAsync("settings.json", JsonSerializer.Serialize(settings, options));
   }
   
   public static async ValueTask SaveNitterData(List<NitterData> nitterData)
   {
      var options = new JsonSerializerOptions()
      {
         DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
         WriteIndented = true
      };
      await File.WriteAllTextAsync("data/nitterdata.json", JsonSerializer.Serialize(nitterData, options));
   }
   
   public static async ValueTask<bool> AppendNitterData(NitterData nitterData)
   {
      var nitterDataList = await LoadNitterData();
      if (nitterDataList.Any(x => x.Url == nitterData.Url))
      {
         return false;
      }
      nitterDataList.Add(nitterData);
      await SaveNitterData(nitterDataList);
      return true;
   }
   
   public static async ValueTask<bool> RemoveNitterData(NitterData nitterData)
   {
      var nitterDataList = await LoadNitterData();
      if (!nitterDataList.Any(x => x.Url == nitterData.Url))
      {
         return false;
      }
      nitterDataList.Remove(nitterData);
      await SaveNitterData(nitterDataList);
      return true;
   }
   
   public static async ValueTask<bool> IsFavorite(string url)
   {
      var nitterDataList = await LoadNitterData();
      return nitterDataList.Any(x => x.Url == url);
   }
   
   
}

public class ColorJsonConverter : JsonConverter<Color>
{
   public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      var colorName = reader.GetString();
      return colorName != null ? (Color)ColorConverter.ConvertFromString(colorName) : Colors.Gray;
   }

   public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
   {
      writer.WriteStringValue(value.ToString());
   }
}

public class BrushJsonConverter : JsonConverter<Brush>
{
   public override Brush Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      var colorName = reader.GetString();
      return colorName != null ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName)) : Brushes.Gray;
   }

   public override void Write(Utf8JsonWriter writer, Brush value, JsonSerializerOptions options)
   {
      writer.WriteStringValue(value.ToString());
   }
}