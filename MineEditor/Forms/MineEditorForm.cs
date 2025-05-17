using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using MineEditor.Models;
using MineEditor.Services;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace MineEditor.Forms
{
  public partial class MineEditorForm : Form
  {
    private ListBox automataListBox, componentsListBox;
    private Button createButton, editButton, deleteButton, activateButton, editComponentsButton;
    private List<AutomataModel> configurations;
    private string mineName;

    /// <summary> Создание окна для управления автоматами и компонентами конфигурации </summary>
    public MineEditorForm(string mineName)
    {
      this.Text = $"{mineName}";
      this.Size = new System.Drawing.Size(530, 600);
      this.StartPosition = FormStartPosition.CenterScreen;

      this.mineName = mineName;

      //Управление компонентами
      componentsListBox = new ListBox
      {
        Location = new System.Drawing.Point(20, 20),
        Size = new System.Drawing.Size(470, 200)
      };
      this.Controls.Add(componentsListBox);

      editComponentsButton = new Button { Text = "Редактировать", Location = new System.Drawing.Point(210, 250), Size = new System.Drawing.Size(110, 30), Enabled = false };

      editComponentsButton.Click += EditComponentsButton_Click;
      componentsListBox.SelectedIndexChanged += ComponentsListBox_SelectedIndexChanged;

      this.Controls.Add(editComponentsButton);

      //Управление автоматами
      automataListBox = new ListBox
      {
        Location = new System.Drawing.Point(20, 320),
        Size = new System.Drawing.Size(470, 150)
      };
      this.Controls.Add(automataListBox);

      createButton = new Button { Text = "Создать", Location = new System.Drawing.Point(20, 500), Size = new System.Drawing.Size(110, 30) };
      editButton = new Button { Text = "Редактировать", Location = new System.Drawing.Point(140, 500), Size = new System.Drawing.Size(110, 30), Enabled = false };
      deleteButton = new Button { Text = "Удалить", Location = new System.Drawing.Point(260, 500), Size = new System.Drawing.Size(110, 30), Enabled = false };
      activateButton = new Button { Text = "Активировать / Деактивировать", Location = new System.Drawing.Point(380, 490), Size = new System.Drawing.Size(110, 50), Enabled = false };
    
      createButton.Click += CreateButton_Click;
      editButton.Click += EditButton_Click;
      deleteButton.Click += DeleteButton_Click;
      activateButton.Click += ActivateButton_Click;
      automataListBox.SelectedIndexChanged += AutomataListBox_SelectedIndexChanged;

      this.Controls.Add(createButton);
      this.Controls.Add(editButton);
      this.Controls.Add(deleteButton);
      this.Controls.Add(activateButton);
     
      //Загрузка списков типов компонентов и автоматов
      LoadComponentTypes();
      LoadAutomata();
    }

    /// <summary> Отображение актуального списка автоматов </summary>
    private void LoadAutomata()
    {
      configurations = ConfigManager.LoadConfigs(mineName);
      automataListBox.Items.Clear();
      foreach (var config in configurations)
      {
        bool isActive = config?.IsActive ?? false;
        string displayText = $"{(isActive ? "[Активный]" : "[Неактивный]")} {config.Token.Name} (UID: {config.Token.UID})";
        automataListBox.Items.Add(displayText);
      }
    }

    /// <summary> Отображение 11 из 12 типов компонентов, без initialState (заданы по умолчанию в enum ComponentType) </summary>
    private void LoadComponentTypes()
    {
      componentsListBox.Items.Clear();
      foreach (ComponentType type in Enum.GetValues(typeof(ComponentType)))
      {
        if (type != ComponentType.initialState)
        {
          componentsListBox.Items.Add(type.ToString());
        }
      }
    }

    /// <summary> Создание нового автомата </summary>
    private void CreateButton_Click(object sender, EventArgs e)
    {
      string name = Interaction.InputBox("Введите имя автомата:", "Новый автомат", "Automata1");
      if (!string.IsNullOrWhiteSpace(name) & name != "config")
      {
        var token = new Token(Guid.NewGuid().ToString(), name);
        var newConfig = new AutomataModel(token);
        ConfigManager.SaveConfig(newConfig, mineName);
        new AutomataEditorForm(newConfig, mineName).ShowDialog();
        LoadAutomata();
      }
      else
      {
        MessageBox.Show("Попробуйте ввести другое имя.");
      }
    }

    /// <summary> Переход к редактированию компонентов выбранного типа для конкретной конфигурации шахты </summary>
    private void EditComponentsButton_Click(object sender, EventArgs e)
    {
      if (componentsListBox.SelectedItem != null)
      {
        string selectedType = componentsListBox.SelectedItem.ToString();
        new ComponentTypeEditorForm(selectedType, mineName).ShowDialog();
      }
    }

    /// <summary> Редактирование существующего автомата </summary>
    private void EditButton_Click(object sender, EventArgs e)
    {
      if (automataListBox.SelectedIndex >= 0)
      {
        var selectedConfig = configurations[automataListBox.SelectedIndex];
        new AutomataEditorForm(selectedConfig, mineName).ShowDialog();
        ConfigManager.SaveConfig(selectedConfig, mineName);
        LoadAutomata();
      }
    }

    /// <summary> Удаление существующего автомата </summary>
    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (automataListBox.SelectedIndex >= 0)
      {
        string configName = configurations[automataListBox.SelectedIndex].Token.Name;
        if (MessageBox.Show($"Удалить {configName}?", "Удаление", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
          ConfigManager.DeleteConfig(configName, mineName);
          LoadAutomata();
        }
      }
    }

    /// <summary> Активация/деактивация существующего автомата (по умолчанию автомат деактивирован) </summary>
    private void ActivateButton_Click(object sender, EventArgs e)
    {
      if (automataListBox.SelectedIndex >= 0)
      {
        string configName = configurations[automataListBox.SelectedIndex].Token.Name;
        string filePath = Path.Combine(ConfigManager.ConfigDirectory, mineName, configName + ".json");

        var automata = JsonConvert.DeserializeObject<AutomataModel>(File.ReadAllText(filePath));
        automata.IsActive = !automata.IsActive;
        File.WriteAllText(filePath, JsonConvert.SerializeObject(automata, Formatting.Indented));

        LoadAutomata();
      }
    }

    /// <summary> Управление доступностью кнопки редактирования для списка типов компонентов </summary>
    private void ComponentsListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      editComponentsButton.Enabled = componentsListBox.SelectedIndex >= 0;
    }

    /// <summary> Управлене доступностью кнопок для списка автоматов </summary>
    private void AutomataListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool hasSelection = automataListBox.SelectedIndex >= 0;
      editButton.Enabled = hasSelection;
      deleteButton.Enabled = hasSelection;
      activateButton.Enabled = hasSelection;
    }
  }
}

