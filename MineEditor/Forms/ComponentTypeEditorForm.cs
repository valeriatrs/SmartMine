using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MineEditor.Models;
using MineEditor.Services;
using Microsoft.VisualBasic;
using Component = MineEditor.Models.Component;

namespace MineEditor.Forms
{
  public partial class ComponentTypeEditorForm : Form
  {
    private ListView componentsListView;
    private Button createButton, editButton, deleteButton;
    private string componentType;
    private string mineName;
    private List<Component> loadedComponents;

    /// <summary> Создание окна для управления компонентами конкретного типа </summary>
    public ComponentTypeEditorForm(string componentType, string mineName)
    {
      this.Text = $"{componentType}";
      this.Size = new System.Drawing.Size(530, 330);
      this.StartPosition = FormStartPosition.CenterScreen;

      this.componentType = componentType;
      this.mineName = mineName;
      this.loadedComponents = new List<Component>();

      componentsListView = new ListView
      {
        Location = new System.Drawing.Point(20, 20),
        Size = new System.Drawing.Size(470, 200),
        View = View.Details,
        FullRowSelect = true,
        GridLines = true
      };

      componentsListView.Columns.Add("Имя компонента", 200);
      componentsListView.Columns.Add("Опции", 250);

      this.Controls.Add(componentsListView);

      createButton = new Button { Text = "Создать", Location = new System.Drawing.Point(20, 240), Size = new System.Drawing.Size(110, 30) };
      editButton = new Button { Text = "Редактировать", Location = new System.Drawing.Point(200, 240), Size = new System.Drawing.Size(110, 30), Enabled = false };
      deleteButton = new Button { Text = "Удалить", Location = new System.Drawing.Point(380, 240), Size = new System.Drawing.Size(110, 30), Enabled = false };
      
      componentsListView.SelectedIndexChanged += ComponentsListView_SelectedIndexChanged;

      createButton.Click += CreateButton_Click;
      editButton.Click += EditButton_Click;
      deleteButton.Click += DeleteButton_Click;
      
      this.Controls.Add(createButton);
      this.Controls.Add(editButton);
      this.Controls.Add(deleteButton);

      LoadComponents();
    }

    /// <summary> Отображение актуального списка компонентов определенного типа в конфигурации конкретной шахты </summary>
    private void LoadComponents()
    {
      componentsListView.Items.Clear();
      loadedComponents = ConfigManager.LoadComponents(componentType, mineName);

      foreach (var component in loadedComponents)
      {
        var item = new ListViewItem(component.Token.Name);
        item.SubItems.Add(string.Join(", ", component.Options.Select(o => o.Token.Name)));
        componentsListView.Items.Add(item);
      }
    }

    /// <summary> Создание нового компонента </summary>
    private void CreateButton_Click(object sender, EventArgs e)
    {
      string name = Interaction.InputBox("Введите имя компонента:", "Новый компонент", "Component1");
      if (!string.IsNullOrWhiteSpace(name))
      {
        var token = new Token(Guid.NewGuid().ToString(), name);
        var component = new Component(token, (ComponentType)Enum.Parse(typeof(ComponentType), componentType));
        new ComponentEditorForm(component).ShowDialog();
        ConfigManager.SaveComponent(component, mineName);
        LoadComponents();
       }
    }

    /// <summary> Редактирование существующего компонента </summary>
    private void EditButton_Click(object sender, EventArgs e)
    {
      if (componentsListView.SelectedItems.Count > 0)
      {
        var selectedComponent = loadedComponents[componentsListView.SelectedIndices[0]];
        var editorForm = new ComponentEditorForm(selectedComponent);
        editorForm.ShowDialog();
        ConfigManager.SaveComponent(selectedComponent, mineName);
        LoadComponents();
      }
    }

    /// <summary> Удаление существующего компонента, если он не используется в существующем автомате </summary>
    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (componentsListView.SelectedItems.Count > 0)
      {
        var selectedComponent = loadedComponents[componentsListView.SelectedIndices[0]];
        var usedInAutomata = ConfigManager.GetAutomataUsingComponent(selectedComponent, mineName);

        if (usedInAutomata.Count > 0)
        {
          MessageBox.Show($"Компонент {selectedComponent.Token.Name} не может быть удален. Он используется в автоматах:\n{string.Join("\n", usedInAutomata.Select(a => a.Token.Name))}", 
            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
          ConfigManager.DeleteComponent(selectedComponent, mineName);
          LoadComponents();
        }
      }
    }

    /// <summary> Управление доступностью кнопок формы </summary>
    private void ComponentsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool hasSelection = componentsListView.SelectedItems.Count > 0;
      editButton.Enabled = hasSelection;
      deleteButton.Enabled = hasSelection;
    }
  }
}
