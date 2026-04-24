using Patchwork.Service;
using Godot;

namespace Patchwork.Scenes.ControlPanel;

public partial class ControlPanelView : Panel
{
    private RootService _rootService;
    private VBoxContainer _placementButtons;
    private Button[] _placementButtonsList;
    private Button _skipButton;

    public override void _Ready()
    {
        _placementButtons = GetNode<VBoxContainer>("PlacementButtons");
        _placementButtonsList =
        [
            GetNode<Button>("PlacementButtons/RotateCounterClockwiseButton"),
            GetNode<Button>("PlacementButtons/RotateClockwiseButton"),
            GetNode<Button>("PlacementButtons/MirrorButton"),
            GetNode<Button>("PlacementButtons/ConfirmPlacementButton")
        ];
        _skipButton = GetNode<Button>("SkipButton");
    }

    public void Initialize(RootService rootService)
    {
        _rootService = rootService;
    }
}
