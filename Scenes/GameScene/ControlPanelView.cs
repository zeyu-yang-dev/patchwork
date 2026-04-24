using Patchwork.Service;
using Godot;

namespace Patchwork.Scenes.GameScene;

public partial class ControlPanelView : Panel
{
    private RootService _rootService;
    private VBoxContainer _placementButtons;
    private TextureButton[] _placementButtonsList;
    private TextureButton _skipButton;

    public override void _Ready()
    {
        _placementButtons = GetNode<VBoxContainer>("PlacementButtons");
        _placementButtonsList =
        [
            GetNode<TextureButton>("PlacementButtons/RotateCounterClockwiseButton"),
            GetNode<TextureButton>("PlacementButtons/RotateClockwiseButton"),
            GetNode<TextureButton>("PlacementButtons/MirrorButton"),
            GetNode<TextureButton>("PlacementButtons/ConfirmPlacementButton")
        ];
        _skipButton = GetNode<TextureButton>("SkipButton");
    }

    public void Initialize(RootService rootService)
    {
        _rootService = rootService;
    }
}
