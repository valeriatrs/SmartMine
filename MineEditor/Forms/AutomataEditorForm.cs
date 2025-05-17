using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MineEditor.Models;
using MineEditor.Services;

namespace MineEditor.Forms
{
  public partial class AutomataEditorForm : Form
  {
    private AutomataModel config;
    private int currentStep = 0;
    private string mineName;

    private Label titleLabel;
    private Panel componentsPanel;
    private Button nextButton, backButton, finishButton;

    private Dictionary<ComponentType, List<Component>> availableComponents = new Dictionary<ComponentType, List<Component>>();
    private List<RadioButton> radioButtons = new List<RadioButton>();

    /// <summary> Создание шаблона окна wizard-формы редактирования автомата </summary>
    public AutomataEditorForm(AutomataModel config, string mineName)
    {
      this.config = config;
      this.mineName = mineName;

      this.Text = $"{config.Token.Name}";
      this.Size = new Size(415, 290);
      this.StartPosition = FormStartPosition.CenterScreen;

      foreach (ComponentType type in Enum.GetValues(typeof(ComponentType)))
      {
        availableComponents[type] = ConfigManager.LoadComponents(type.ToString(), mineName);
      }

      titleLabel = new Label
      {
        Location = new Point(20, 20),
        Size = new Size(360, 20),
        Font = new Font("Arial", 12, FontStyle.Bold)
      };
      this.Controls.Add(titleLabel);

      componentsPanel = new Panel
      {
        Location = new Point(20, 50),
        Size = new Size(340, 120),
        AutoScroll = true
      };
      this.Controls.Add(componentsPanel);

      backButton = new Button { Text = "Назад", Location = new Point(20, 200), Size = new Size(80, 30) };
      nextButton = new Button { Text = "Далее", Location = new Point(110, 200), Size = new Size(80, 30) };
      finishButton = new Button { Text = "Готово", Location = new Point(300, 200), Size = new Size(80, 30), Enabled = false };

      backButton.Click += BackButton_Click;
      nextButton.Click += NextButton_Click;
      finishButton.Click += FinishButton_Click;

      this.Controls.Add(backButton);
      this.Controls.Add(nextButton);
      this.Controls.Add(finishButton);

      LoadStep();
    }

    /// <summary> Загрузка редактора текущего компонента </summary>
    private void LoadStep()
    {
      var currentType = (ComponentType)currentStep;
      titleLabel.Text = $"{currentType}";

      componentsPanel.Controls.Clear();
      radioButtons.Clear();

      int yPos = 0;

      if (currentType == ComponentType.initialState)
      {
        // Для initialState используем только состояния из компонента типа states
        if (config.Components.TryGetValue(ComponentType.states, out var statesComponent))
        {
          foreach (var state in statesComponent.Options)
          {
            var radioButton = new RadioButton
            {
              Text = state.Token.Name,
              Location = new Point(10, yPos),
              AutoSize = true,
              Tag = state
            };

            componentsPanel.Controls.Add(radioButton);
            radioButtons.Add(radioButton);
            yPos += 25;
          }
        }
        else
        {
          var label = new Label
          {
            Text = "Сначала необходимо выбрать компонент типа states",
            Location = new Point(10, yPos),
            AutoSize = true
          };
          componentsPanel.Controls.Add(label);
        }
      }
      else
      {
        // Для остальных компонентов используем стандартную логику
        foreach (var component in availableComponents[currentType])
        {
          var radioButton = new RadioButton
          {
            Text = component.Token.Name,
            Location = new Point(10, yPos),
            AutoSize = true,
            Tag = component
          };

          componentsPanel.Controls.Add(radioButton);
          radioButtons.Add(radioButton);
          yPos += 25;
        }
      }

      if (config.Components.ContainsKey(currentType))
      {
        var currentComponent = config.Components[currentType];
        if (currentType == ComponentType.initialState)
        {
          var radioButton = radioButtons.FirstOrDefault(rb => ((Option)rb.Tag).Token.UID == currentComponent.Options[0].Token.UID);
          if (radioButton != null)
          {
            radioButton.Checked = true;
          }
        }
        else
        {
          var radioButton = radioButtons.FirstOrDefault(rb => ((Component)rb.Tag).Token.UID == currentComponent.Token.UID);
          if (radioButton != null)
          {
            radioButton.Checked = true;
          }
        }
      }
      else if (radioButtons.Count > 0)
      {
        radioButtons[0].Checked = true;
      }

      //Управление доступностью кнопок
      backButton.Enabled = currentStep > 0;
      nextButton.Enabled = currentStep < 11;
      finishButton.Enabled = currentStep == 11;
    }

    /// <summary> Сохранение текущего выбора </summary>
    private void SaveStep()
    {
      var currentType = (ComponentType)currentStep;
      
      var selectedRadioButton = radioButtons.FirstOrDefault(rb => rb.Checked);
      if (selectedRadioButton != null)
      {
        if (currentType == ComponentType.initialState)
        {
          // Для initialState создаем новый компонент с выбранным состоянием
          var selectedState = (Option)selectedRadioButton.Tag;
          var initialStateComponent = new Component(
            new Token(Guid.NewGuid().ToString(), "InitialState"),
            ComponentType.initialState
          );
          initialStateComponent.Options.Add(selectedState);

          if (config.Components.ContainsKey(currentType))
          {
            config.Components[currentType] = initialStateComponent;
          }
          else
          {
            config.Components.Add(currentType, initialStateComponent);
          }
        }
        else
        {
          var selectedComponent = (Component)selectedRadioButton.Tag;
          if (config.Components.ContainsKey(currentType))
          {
            config.Components[currentType] = selectedComponent;
          }
          else
          {
            config.Components.Add(currentType, selectedComponent);
          }
        }
      }
    }

    /// <summary> Переход на предыдущий шаг (кнопка доступна для компонентов со 2го по 12ый включительно) </summary>
    private void BackButton_Click(object sender, EventArgs e)
    {
      SaveStep();
      currentStep--;
      LoadStep();
    }

    /// <summary> Переход на следующий шаг (кнопка доступна для компонентов с 1го по 11ый включительно) </summary>
    private void NextButton_Click(object sender, EventArgs e)
    {
      SaveStep();
      currentStep++;
      LoadStep();
    }

    /// <summary> Завершение редактирования автомата (кнопка доступна только для 12го компонента) </summary>
    private void FinishButton_Click(object sender, EventArgs e)
    {
      SaveStep();
      ConfigManager.SaveConfig(config, mineName);
      MessageBox.Show("Конфигурация автомата успешно сохранена!", "Успешное сохранение", MessageBoxButtons.OK, MessageBoxIcon.Information);
      this.Close();
    }
  }
}
