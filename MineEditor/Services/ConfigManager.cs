using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using MineEditor.Models;
using System.Linq;

namespace MineEditor.Services
{
  public static class ConfigManager
  {
    public static readonly string ConfigDirectory = @"..\..\Configs";

    /// <summary> Загрузка конфигураций шахты из основной директории </summary>
    public static List<string> LoadMines()
    {
      if (!Directory.Exists(ConfigDirectory)) Directory.CreateDirectory(ConfigDirectory);
      var mines = new List<string>();

      foreach (string dir in Directory.GetDirectories(ConfigDirectory))
      {
        if (Path.GetFileName(dir).Equals("Components", StringComparison.OrdinalIgnoreCase))
          continue;
        mines.Add(new DirectoryInfo(dir).Name);
      }
      return mines;
    }

    /// <summary> Загрузка конфигураций автоматов из директории шахты </summary>
    public static List<AutomataModel> LoadConfigs(string mineName)
    {
      var configs = new List<AutomataModel>();
      var minePath = Path.Combine(ConfigDirectory, mineName);
      if (!Directory.Exists(minePath))
      {
        return configs;
      }

      var mineConfig = LoadMineConfig(mineName);
      foreach (var token in mineConfig.AutomataTokens)
      {
        var files = Directory.GetFiles(minePath, "*.json");
        foreach (var file in files)
        {
          if (Path.GetFileName(file).Equals("config.json", StringComparison.OrdinalIgnoreCase))
            continue;

          var config = JsonConvert.DeserializeObject<AutomataModel>(File.ReadAllText(file));
          if (config.Token.UID == token.UID)
          {
            configs.Add(config);
            break;
          }
        }
      }

      return configs;
    }

    /// <summary> Сохранение конфигурации автомата в директорию шахты и обновление config-файла шахты </summary>
    public static void SaveConfig(AutomataModel config, string mineName)
    {
      var minePath = Path.Combine(ConfigDirectory, mineName);
      if (!Directory.Exists(minePath))
      {
        Directory.CreateDirectory(minePath);
      }

      // Сохраняем конфигурацию автомата
      var configPath = Path.Combine(minePath, config.Token.Name + ".json");
      var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
      File.WriteAllText(configPath, jsonString);

      // Обновляем конфигурацию шахты
      var mineConfig = LoadMineConfig(mineName);
      if (!string.IsNullOrEmpty(config.Token.UID) && !mineConfig.AutomataTokens.Any(t => t.UID == config.Token.UID))
      {
        mineConfig.AutomataTokens.Add(config.Token);
        SaveMineConfig(mineName, mineConfig);
      }
    }

    /// <summary> Удаление конфигурации автомата из директории шахты и config-файла шахты </summary>
    public static void DeleteConfig(string configName, string mineName)
    {
      var configPath = Path.Combine(ConfigDirectory, mineName, configName + ".json");
      if (File.Exists(configPath))
      {
        var config = JsonConvert.DeserializeObject<AutomataModel>(File.ReadAllText(configPath));
        var mineConfig = LoadMineConfig(mineName);
        mineConfig.AutomataTokens.RemoveAll(t => t.UID == config.Token.UID);
        SaveMineConfig(mineName, mineConfig);
        File.Delete(configPath);
      }
    }

    /// <summary> Загрузка конфигурации шахты </summary>
    private static MineConfiguration LoadMineConfig(string mineName)
    {
      var configPath = Path.Combine(ConfigDirectory, mineName, "config.json");
      var token = new Token(Guid.NewGuid().ToString(), mineName);
      if (!File.Exists(configPath))
      {
        return new MineConfiguration(token);
      }

      var jsonString = File.ReadAllText(configPath);

      return JsonConvert.DeserializeObject<MineConfiguration>(jsonString) ?? new MineConfiguration(token);
    }

    /// <summary> Сохранение конфигурации шахты </summary>
    private static void SaveMineConfig(string mineName, MineConfiguration config)
    {
      var configPath = Path.Combine(ConfigDirectory, mineName, "config.json");
      var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
      File.WriteAllText(configPath, jsonString);
    }

    /// <summary> Загрузка компонентов из директории типа компонента </summary>
    public static List<Component> LoadComponents(string componentType, string mineName)
    {
      var components = new List<Component>();
      var minePath = Path.Combine(ConfigDirectory, mineName);
      var componentsPath = Path.Combine(minePath, "Components", componentType);
      
      if (!Directory.Exists(componentsPath))
        return components;

      var mineConfig = LoadMineConfig(mineName);
      var componentTypeEnum = (ComponentType)Enum.Parse(typeof(ComponentType), componentType);
      if (!mineConfig.Components.ContainsKey(componentTypeEnum))
        return components;

      var componentTokens = mineConfig.Components[componentTypeEnum];
      foreach (var file in Directory.GetFiles(componentsPath, "*.json"))
      {
        var json = File.ReadAllText(file);
        var component = JsonConvert.DeserializeObject<Component>(json);
        if (component != null && componentTokens.Any(t => t.UID == component.Token.UID))
        {
          components.Add(component);
        }
      }

      return components;
    }

    /// <summary> Сохранение компонента в директорию типа компонента и в config-файл шахты </summary>
    public static void SaveComponent(Component component, string mineName)
    {
      var minePath = Path.Combine(ConfigDirectory, mineName);
      var componentsPath = Path.Combine(minePath, "Components", component.Type.ToString());
      
      if (!Directory.Exists(componentsPath))
        Directory.CreateDirectory(componentsPath);

      var filePath = Path.Combine(componentsPath, component.Token.Name + ".json");
      var jsonString = JsonConvert.SerializeObject(component, Formatting.Indented);
      File.WriteAllText(filePath, jsonString);

      // Обновляем конфигурацию шахты
      var mineConfig = LoadMineConfig(mineName);
      if (!mineConfig.Components.ContainsKey(component.Type))
      {
        mineConfig.Components[component.Type] = new List<Token>();
      }

      if (!mineConfig.Components[component.Type].Any(t => t.UID == component.Token.UID))
      {
        mineConfig.Components[component.Type].Add(component.Token);
        SaveMineConfig(mineName, mineConfig);
      }
    }

    /// <summary> Удаление компонента из директории типа компонента и config-файла шахты </summary>
    public static void DeleteComponent(Component component, string mineName)
    {
      var minePath = Path.Combine(ConfigDirectory, mineName);
      var componentsPath = Path.Combine(minePath, "Components", component.Type.ToString());
      var filePath = Path.Combine(componentsPath, component.Token.Name + ".json");
      
      if (File.Exists(filePath))
      {
        File.Delete(filePath);

        // Обновляем конфигурацию шахты
        var mineConfig = LoadMineConfig(mineName);
        if (mineConfig.Components.ContainsKey(component.Type))
        {
          mineConfig.Components[component.Type].RemoveAll(t => t.UID == component.Token.UID);
          SaveMineConfig(mineName, mineConfig);
        }
      }
    }

    /// <summary> Получение списка автоматов, использующих конкретный компонент </summary>
    public static List<AutomataModel> GetAutomataUsingComponent(Component component, string mineName)
    {
      var automata = new List<AutomataModel>();
      var allConfigs = LoadConfigs(mineName);

      foreach (var config in allConfigs)
      {
        if (config.Components.ContainsKey(component.Type) && 
            config.Components[component.Type].Token.UID == component.Token.UID)
        {
          automata.Add(config);
        }
      }
      return automata;
    }
  }
}
