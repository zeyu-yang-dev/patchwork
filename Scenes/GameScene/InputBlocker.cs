using Godot;

namespace Patchwork.Scenes.GameScene;

public partial class InputBlocker : Control
{
	public override void _Ready()
	{
		Hide();
		MouseFilter = MouseFilterEnum.Stop;
		FocusMode = FocusModeEnum.All;
	}

	public override void _GuiInput(InputEvent @event)
	{
		AcceptEvent();
	}

	public void Enable()
	{
		MoveToFront();
		Show();
		GrabFocus();
	}

	public void Disable()
	{
		ReleaseFocus();
		Hide();
	}
}
