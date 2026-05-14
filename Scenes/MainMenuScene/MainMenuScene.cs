using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System;
using System.Linq;

namespace Patchwork.Scenes.MainMenuScene;

public partial class MainMenuScene : Control
{
	private RootService _rootService;
	public LineEdit[] _textFields;
	public TextureButton _startButton;
	public TextureButton _exitButton;
	
	public override void _Ready()
	{
		var textFieldsNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/InputArea/TextFields");
		_textFields = textFieldsNode.GetChildren().OfType<LineEdit>().ToArray();
		
		var buttonsNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/Buttons");
		_startButton = buttonsNode.GetNode<TextureButton>("StartButton");
		_exitButton = buttonsNode.GetNode<TextureButton>("ExitButton");
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
	}

	
}
