using Godot;
using Patchwork.Domain;
using Patchwork.Service;
using System;
using System.Linq;

namespace Patchwork.Scenes.StartScene;

public partial class StartScene : Control
{
	private RootService _rootService;
	
	public override void _Ready()
	{
	}
	
	public void Initialize(RootService rootService)
	{
		_rootService = rootService;
	}

	
}
