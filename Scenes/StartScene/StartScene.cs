using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System;
using System.Linq;

namespace Patchwork.Scenes.StartScene;

public partial class StartScene : Control
{
	private RootService _rootService;
	public TextureButton _startButton;
	public TextureButton _exitButton;
	
	public override void _Ready()
	{
		var buttonsNode = GetNode<Node>("Window/MarginContainer/VBoxContainer/Buttons");
		_startButton = buttonsNode.GetNode<TextureButton>("StartButton");
		_exitButton = buttonsNode.GetNode<TextureButton>("ExitButton");
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
	}

	
}
