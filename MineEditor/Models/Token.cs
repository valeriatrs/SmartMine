using System;
using System.Collections.Generic;

namespace MineEditor.Models
{
  public class Token
  {
    /// <summary> Уникальный идентификатор сущности </summary>
    public string UID { get; set; }

    /// <summary> Имя сущности </summary>
    public string Name { get; set; }

    /// <summary> Сет для хранения созданных uid'ов </summary>
    private static readonly HashSet<string> ValidationSet = new HashSet<string>();

    /// <summary> Конструктор класса Token </summary>
    public Token(string uid, string name)
    {
      ValidationSet.Add(uid);
      UID = uid;
      Name = name;
    }
  }
} 