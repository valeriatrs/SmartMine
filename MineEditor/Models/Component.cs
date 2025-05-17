using System;
using System.Collections.Generic;

namespace MineEditor.Models
{
  public class Component
  {
    /// <summary> Уникальный токен компонента </summary>
    public Token Token { get; }

    /// <summary> Тип компонента (например: externalIncomingEvents, externalOutgoingEvents) </summary>
    public ComponentType Type { get; set; }

    /// <summary> Список опций для данного компонента (например: states = Loading, Dumping) </summary>
    public List<Option> Options { get; }

    /// <summary> Конструктор класса Component </summary>
    public Component(Token token, ComponentType type)
    {
      Token = token;
      Type = type;
      Options = new List<Option>();
    }
  }
}