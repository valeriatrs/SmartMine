using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using MineEditor.Models;
using MineEditor.Services;
using Newtonsoft.Json;

namespace MineEditor.Forms
{
  public partial class MainForm : Form
  {
    private ListBox listBoxMines;
    private Button createButton, editButton, deleteButton;

    /// <summary> Создание стартового окна приложения </summary>
    public MainForm()
    {
      this.Text = "Конфигурации шахты";
      this.Size = new System.Drawing.Size(400, 300);
      this.StartPosition = FormStartPosition.CenterScreen;

      listBoxMines = new ListBox
      {
        Location = new System.Drawing.Point(20, 20),
        Size = new System.Drawing.Size(340, 150)
      };
      this.Controls.Add(listBoxMines);

      createButton = new Button { Text = "Создать", Location = new System.Drawing.Point(20, 200), Size = new System.Drawing.Size(100, 30) };
      editButton = new Button { Text = "Редактировать", Location = new System.Drawing.Point(140, 200), Size = new System.Drawing.Size(100, 30), Enabled = false };
      deleteButton = new Button { Text = "Удалить", Location = new System.Drawing.Point(260, 200), Size = new System.Drawing.Size(100, 30), Enabled = false };

      createButton.Click += CreateButton_Click;
      editButton.Click += EditButton_Click;
      deleteButton.Click += DeleteButton_Click;
      listBoxMines.SelectedIndexChanged += listBoxMines_SelectedIndexChanged;

      this.Controls.Add(createButton);
      this.Controls.Add(editButton);
      this.Controls.Add(deleteButton);

      LoadMines();
    }

    /// <summary> Отображение актуального списка конфигураций шахты </summary>
    private void LoadMines()
    {
      listBoxMines.Items.Clear();
      var mines = ConfigManager.LoadMines();

      foreach (string mine in mines)
      {
        listBoxMines.Items.Add(mine);
      }
    }

    /// <summary> Создание новой конфигурации шахты </summary>
    private void CreateButton_Click(object sender, EventArgs e)
    {
      string mineName = Interaction.InputBox("Введите имя конфигурации:", "Новая конфигурация", "Mine1");

      if (!string.IsNullOrWhiteSpace(mineName))
      {
        string minePath = Path.Combine(ConfigManager.ConfigDirectory, mineName);

        if (!Directory.Exists(minePath))
        {
          Directory.CreateDirectory(minePath);
          var token = new Token(Guid.NewGuid().ToString(), mineName);
          var config = new MineConfiguration(token);
          var jsonString = JsonConvert.SerializeObject(config, Formatting.Indented);
          File.WriteAllText(Path.Combine(minePath, "config.json"), jsonString);
          new MineEditorForm(mineName).ShowDialog();
          LoadMines();
        }
      }
    }

    /// <summary> Редактирование существующей конфигурации шахты </summary>
    private void EditButton_Click(object sender, EventArgs e)
    {
      if (listBoxMines.SelectedItem != null)
      {
        string mineName = listBoxMines.SelectedItem.ToString();
        new MineEditorForm(mineName).ShowDialog();
      }
      else
      {
        MessageBox.Show("Выберите конфигурацию для редактирования");
      }
    }

    /// <summary> Удаление существующей конфигурации шахты </summary>
    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (listBoxMines.SelectedItem != null)
      {
        string mineName = listBoxMines.SelectedItem.ToString();
        string minePath = Path.Combine(ConfigManager.ConfigDirectory, mineName);

        if (MessageBox.Show($"Удалить конфигурацию {mineName}?", $"Удаление {mineName}", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
          Directory.Delete(minePath, true);
          LoadMines();
        }
      }
      else
      {
        MessageBox.Show("Выберите конфигурацию для удаления");
      }
    }

    /// <summary> Управление доступностью кнопок формы </summary>
    private void listBoxMines_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool hasSelection = listBoxMines.SelectedIndex >= 0;
      editButton.Enabled = hasSelection;
      deleteButton.Enabled = hasSelection;
    }
  }
}
