using System;
using System.Collections.Generic;

namespace MineEditor.Models
{
  public class AutomataModel
  {
    /// <summary> Уникальный токен автомата </summary>
    public Token Token { get; set; }

    /// <summary> Статус автомата - активен/неактивен </summary>
    public bool IsActive { get; set; }

    /// <summary> У автомата для каждого типа компонента определен этот компонент со списком опций </summary>
    public Dictionary<ComponentType, Component> Components { get; set; }

    /// <summary> Конструктор класса AutomataModel </summary>
    public AutomataModel(Token token)
    {
      Token = token;
      IsActive = false;
      Components = new Dictionary<ComponentType, Component>();
    }
  }
}