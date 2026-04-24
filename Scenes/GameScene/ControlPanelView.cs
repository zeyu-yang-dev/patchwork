using Patchwork.Service;
using Godot;

namespace Patchwork.Scenes.GameScene;

public partial class ControlPanelView : Panel
{
    private RootService _rootService;
    
    private TextureButton _rotateLeftButton;
    private TextureButton _rotateRightButton;
    private TextureButton _mirrorButton;
    private TextureButton _confirmButton;
    private TextureButton _skipButton;

    // =================================================================================================================
    
    public override void _Ready()
    {
        _rotateLeftButton = GetNode<TextureButton>("PlacementButtons/RotateCounterClockwiseButton");
        _rotateRightButton = GetNode<TextureButton>("PlacementButtons/RotateClockwiseButton");
        _mirrorButton = GetNode<TextureButton>("PlacementButtons/MirrorButton");
        _confirmButton = GetNode<TextureButton>("PlacementButtons/ConfirmPlacementButton");
        _skipButton = GetNode<TextureButton>("SkipButton");
        
        _rotateLeftButton.ButtonDown += OnRotateLeftPressed;
        _rotateRightButton.ButtonDown += OnRotateRightPressed;
        _mirrorButton.ButtonDown += OnMirrorPressed;
        _confirmButton.ButtonDown += OnConfirmPressed;
        _skipButton.ButtonDown += OnSkipPressed;
    }

    public void Initialize(RootService rootService)
    {
        _rootService = rootService;
        _rootService.StateChanged += OnGameStateChanged;
    }
    
    // =================================================================================================================

    private void OnRotateLeftPressed()
    {
        _rootService.PatchService.RotateCounterClockwise();
    }

    private void OnRotateRightPressed()
    {
        _rootService.PatchService.RotateClockwise();
    }

    private void OnMirrorPressed()
    {
        _rootService.PatchService.Mirror();
    }

    private void OnConfirmPressed()
    {
        _rootService.PlayerActionService.PlacePatch();
    }

    private void OnSkipPressed()
    {
        _rootService.PlayerActionService.Skip();
    }

    // =================================================================================================================
    
    
    
    
    
    private void RefreshVisual()
    {
        
    }
    
    // 由Service层驱动的刷新函数
    private void OnGameStateChanged()
    {
        RefreshVisual();
    }
}
