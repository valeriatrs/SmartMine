using System;
using System.Collections.Generic;

namespace MineEditor.Models
{
  public class MineConfiguration
  {
    /// <summary> Уникальный токен шахты </summary>
    public Token Token { get; } 

    /// <summary> Уникальные номера автоматов, входящих в состав шахты </summary>
    public List<Token> AutomataTokens { get; set; }

    /// <summary> Для каждого типа компонента определен список возможных компонентов для каждого "типа автомата" </summary>
    public Dictionary<ComponentType, List<Token>> Components { get; set; }

    /// <summary> Конструктор класса MineConfiguration </summary>
    public MineConfiguration(Token token)
    {
      Token = token;
      Components = new Dictionary<ComponentType, List<Token>>();
      AutomataTokens = new List<Token>();
    }
  }
}