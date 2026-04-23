using System;
using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchShop;

public partial class PatchShopView : Panel
{
    // 1. 按钮的index 2. 按钮的中心点的全局坐标 3. 从按钮中心指向鼠标位置的矢量
    public event Action<int, Vector2, Vector2> PatchSelected;

    private const string OriginalTextureDirectory = "res://Assets/Patches/original";

    private RootService _rootService;
    private TextureButton[] _Buttons;
    private int _hiddenButtonIndex = -1; // 需要隐藏的button的index

    private static readonly Vector2 ButtonSize = new(250.0f, 250.0f);
    private static readonly Vector2[] ButtonPositions =
    [
        // 每个button之间的间隔为5
        new Vector2(0.0f, 0.0f),
        new Vector2(255.0f, 0.0f),
        new Vector2(510.0f, 0.0f)
    ];

    // =================================================================================================================
    
    // _Ready处理场景树内部的事情：1. 子节点获取 2. 节点属性初始化 3. 场景内节点之间的事件连接
    public override void _Ready()
    {
        _Buttons =
        [
            GetNode<TextureButton>("PatchButton0"),
            GetNode<TextureButton>("PatchButton1"),
            GetNode<TextureButton>("PatchButton2")
        ];

        for (var i = 0; i < _Buttons.Length; i++)
        {
            var patchOffset = i;
            // 指定3个TextureButton的位置和大小
            _Buttons[i].Position = ButtonPositions[i];
            _Buttons[i].Size = ButtonSize;
            // 订阅ButtonDown事件
            _Buttons[i].ButtonDown += () => OnButtonDown(patchOffset);
        }
    }
    
    // Initialize处理场景树外部的依赖注入
    public void Initialize(RootService rootService)
    {
        _rootService = rootService;
        Refresh();
    }

    // =================================================================================================================
    
    public void Refresh()
    {
        if (_rootService?.CurrentGame == null)
        {
            return;
        }

        var selectablePatches = _rootService.CurrentGame.PatchShop.GetSelectablePatches();
        var buyablePatchOffsets = _rootService.PlayerActionService.GetBuyablePatchOffsets();

        // 对每一个button的外观进行刷新
        for (var i = 0; i < _Buttons.Length; i++)
        {
            if (i < selectablePatches.Count)
            {
                _Buttons[i].TextureNormal = LoadButtonTexture(selectablePatches[i].Id);
                _Buttons[i].Visible = i != _hiddenButtonIndex;
                _Buttons[i].Modulate = buyablePatchOffsets.Contains(i)
                    ? Colors.White
                    : new Color(1.0f, 1.0f, 1.0f, 0.35f);
                _Buttons[i].Disabled = !buyablePatchOffsets.Contains(i);
            }
            else
            {
                _Buttons[i].TextureNormal = null;
                _Buttons[i].Visible = false;
                _Buttons[i].Disabled = true;
            }
        }
    }

    public void HideButtonAtIndex(int patchOffset)
    {
        _hiddenButtonIndex = patchOffset;
        Refresh();
    }

    public void RestoreHiddenButton()
    {
        _hiddenButtonIndex = -1;
        Refresh();
    }

    // =================================================================================================================
    
    // 在_Ready()中订阅了ButtonDown事件
    // 先检查被按下的按钮对应的patchOffset是否buyable，
    // 如果是buyable的，发出PatchSelected事件。
    private void OnButtonDown(int patchOffset)
    {
        if (_rootService == null)
        {
            return;
        }

        var buyablePatchOffsets = _rootService.PlayerActionService.GetBuyablePatchOffsets();

        if (!buyablePatchOffsets.Contains(patchOffset))
        {
            return;
        }

        var button = _Buttons[patchOffset];
        var buttonCenterGlobal = button.GetGlobalRect().GetCenter(); // 这个按钮的中心点在屏幕上的坐标
        var centerToCursorOffset = GetGlobalMousePosition() - buttonCenterGlobal;

        // 1. 按钮的index 2. 按钮的中心点的全局坐标 3. 从按钮中心指向鼠标位置的矢量
        PatchSelected?.Invoke(patchOffset, buttonCenterGlobal, centerToCursorOffset);
    }

    private static Texture2D LoadButtonTexture(int patchId)
    {
        return ResourceLoader.Load<Texture2D>($"{OriginalTextureDirectory}/{patchId}.png");
    }
}
