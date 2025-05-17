using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineEditor.Models
{
  public class Option
  {
    /// <summary> Уникальный токен опции </summary>
    public Token Token { get; }

    /// <summary> Дополнительные данные для компонента </summary>
    public string AdditionalData { get; set; }

    /// <summary> Конструктор класса Option </summary>
    public Option(Token token)
    {
      Token = token;
    }
  }
}