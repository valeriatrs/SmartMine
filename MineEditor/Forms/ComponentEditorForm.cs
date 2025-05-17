using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MineEditor.Models;
using Microsoft.VisualBasic;
using Component = MineEditor.Models.Component;

namespace MineEditor.Forms
{
  public class OptionInputForm : Form
  {
    private TextBox nameTextBox;
    private TextBox additionalDataTextBox;
    private Button okButton;
    private Button cancelButton;

    public string OptionName => nameTextBox.Text;
    public string AdditionalData => additionalDataTextBox.Text;

    /// <summary> Создание вспомогательной формы для ввода имени опции и дополнительных данных </summary>
    public OptionInputForm(string title, string defaultName = "", string defaultAdditionalData = "")
    {
      this.Text = title;
      this.Size = new System.Drawing.Size(400, 150);
      this.StartPosition = FormStartPosition.CenterParent;
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;

      var nameLabel = new Label { Text = "Имя опции:", Location = new System.Drawing.Point(10, 15), AutoSize = true };
      nameTextBox = new TextBox { Location = new System.Drawing.Point(150, 12), Size = new System.Drawing.Size(220, 20), Text = defaultName };

      var additionalDataLabel = new Label { Text = "Дополнительные данные:", Location = new System.Drawing.Point(10, 45), AutoSize = true };
      additionalDataTextBox = new TextBox { Location = new System.Drawing.Point(150, 42), Size = new System.Drawing.Size(220, 20), Text = defaultAdditionalData };

      okButton = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new System.Drawing.Point(120, 80), Size = new System.Drawing.Size(75, 23) };
      cancelButton = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new System.Drawing.Point(295, 80), Size = new System.Drawing.Size(75, 23) };

      this.Controls.AddRange(new Control[] { nameLabel, nameTextBox, additionalDataLabel, additionalDataTextBox, okButton, cancelButton });
      this.AcceptButton = okButton;
      this.CancelButton = cancelButton;
    }
  }

  /// <summary> Создание окна для управления опциями конкретного компонента </summary>
  public partial class ComponentEditorForm : Form
  {
    private ListView optionsListView;
    private Button createButton, editButton, deleteButton;
    private Component component;

    public ComponentEditorForm(Component component)
    {
      this.Text = $"{component.Token.Name}";
      this.Size = new System.Drawing.Size(530, 330);
      this.StartPosition = FormStartPosition.CenterScreen;

      this.component = component;

      optionsListView = new ListView
      {
        Location = new System.Drawing.Point(20, 20),
        Size = new System.Drawing.Size(470, 200),
        View = View.Details,
        FullRowSelect = true,
        GridLines = true
      };

      optionsListView.Columns.Add("Имя опции", 100);
      optionsListView.Columns.Add("UID", 250);
      optionsListView.Columns.Add("Дополнительные данные", 200);

      this.Controls.Add(optionsListView);

      createButton = new Button { Text = "Создать", Location = new System.Drawing.Point(20, 240), Size = new System.Drawing.Size(110, 30) };
      editButton = new Button { Text = "Редактировать", Location = new System.Drawing.Point(200, 240), Size = new System.Drawing.Size(110, 30), Enabled = false };
      deleteButton = new Button { Text = "Удалить", Location = new System.Drawing.Point(380, 240), Size = new System.Drawing.Size(110, 30), Enabled = false };

      optionsListView.SelectedIndexChanged += OptionsListView_SelectedIndexChanged;

      createButton.Click += CreateButton_Click;
      editButton.Click += EditButton_Click;
      deleteButton.Click += DeleteButton_Click;
      
      this.Controls.Add(createButton);
      this.Controls.Add(editButton);
      this.Controls.Add(deleteButton);

      LoadOptions();
    }

    /// <summary> Отображение актуального списка опций </summary>
    private void LoadOptions()
    {
      optionsListView.Items.Clear();
      foreach (var option in component.Options)
      {
        var item = new ListViewItem(option.Token.Name);
        item.SubItems.Add(option.Token.UID);
        item.SubItems.Add(option.AdditionalData ?? "");
        optionsListView.Items.Add(item);
      }
    }

    /// <summary> Создание новой опции </summary>
    private void CreateButton_Click(object sender, EventArgs e)
    {
      using (var inputForm = new OptionInputForm("Новая опция"))
      {
        if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputForm.OptionName))
        {
          var token = new Token(Guid.NewGuid().ToString(), inputForm.OptionName);
          var option = new Option(token);
          option.AdditionalData = inputForm.AdditionalData;
          component.Options.Add(option);
          LoadOptions();
        }
      }
    }

    /// <summary> Редактирование существующей опции </summary>
    private void EditButton_Click(object sender, EventArgs e)
    {
      if (optionsListView.SelectedItems.Count > 0)
      {
        var selectedOption = component.Options[optionsListView.SelectedIndices[0]];
        using (var inputForm = new OptionInputForm("Редактирование опции", selectedOption.Token.Name, selectedOption.AdditionalData))
        {
          if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputForm.OptionName))
          {
            selectedOption.Token.Name = inputForm.OptionName;
            selectedOption.AdditionalData = inputForm.AdditionalData;
            LoadOptions();
          }
        }
      }
    }

    /// <summary> Удаление существующей опции </summary>
    private void DeleteButton_Click(object sender, EventArgs e)
    {
      if (optionsListView.SelectedItems.Count > 0)
      {
        var selectedOption = component.Options[optionsListView.SelectedIndices[0]];
        component.Options.Remove(selectedOption);
        LoadOptions();
      }
    }

    /// <summary> Управление доступностью кнопок формы </summary>
    private void OptionsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
      bool hasSelection = optionsListView.SelectedItems.Count > 0;
      editButton.Enabled = hasSelection;
      deleteButton.Enabled = hasSelection;
    }
  }
}

