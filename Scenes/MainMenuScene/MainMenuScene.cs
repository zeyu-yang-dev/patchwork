using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System;
using System.Linq;

namespace Patchwork.Scenes.MainMenuScene;

public partial class MainMenuScene : Control
{
	private RootService _rootService;
	private LineEdit[] _textFields;
	private TextureButton _startButton;
	private TextureButton _exitButton;
	
	public override void _Ready()
	{
		var textFieldsNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/InputArea/TextFields");
		_textFields = textFieldsNode.GetChildren().OfType<LineEdit>().ToArray();
		
		var buttonsNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/Buttons");
		_startButton = buttonsNode.GetNode<TextureButton>("StartButton");
		_exitButton = buttonsNode.GetNode<TextureButton>("ExitButton");

		_startButton.Pressed += OnStartButtonPressed;
		_exitButton.Pressed += OnExitButtonPressed;
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
	}
	
	private void OnStartButtonPressed()
	{
		if (_textFields[0].Text == "" || _textFields[1].Text == "") return;
		
		_rootService.GameService.StartNewGame(_textFields[0].Text, _textFields[1].Text);
		
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0f, 1.0f);
		
		tween.Finished += () =>
		{
			Visible = false;
		};
	}

	private void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}
