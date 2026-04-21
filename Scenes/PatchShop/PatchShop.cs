using System;
using Godot;
using Patchwork.Domain;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchShop;

public partial class PatchShop : Panel
{
    public event Action<int, Vector2, Vector2> PatchSelected;

    private const string OriginalTextureDirectory = "res://Assets/Patches/original";

    private RootService _rootService;
    private TextureButton[] _patchButtons;
    private int _hiddenPatchOffset = -1;

    private static readonly Vector2 PatchButtonSize = new(250.0f, 250.0f);
    private static readonly Vector2[] PatchButtonPositions =
    [
        new Vector2(0.0f, 0.0f),
        new Vector2(255.0f, 0.0f),
        new Vector2(510.0f, 0.0f)
    ];

    public override void _Ready()
    {
        _patchButtons =
        [
            GetNode<TextureButton>("PatchButton0"),
            GetNode<TextureButton>("PatchButton1"),
            GetNode<TextureButton>("PatchButton2")
        ];

        for (var i = 0; i < _patchButtons.Length; i++)
        {
            var patchOffset = i;
            _patchButtons[i].Position = PatchButtonPositions[i];
            _patchButtons[i].Size = PatchButtonSize;
            _patchButtons[i].ButtonDown += () => OnPatchButtonDown(patchOffset);
        }
    }

    public void Initialize(RootService rootService)
    {
        _rootService = rootService;
        Refresh();
    }

    public void Refresh()
    {
        if (_rootService?.CurrentGame == null)
        {
            return;
        }

        var selectablePatches = _rootService.CurrentGame.PatchShop.GetSelectablePatches();
        var buyablePatchOffsets = _rootService.PlayerActionService.GetBuyablePatchOffsets();

        for (var i = 0; i < _patchButtons.Length; i++)
        {
            if (i < selectablePatches.Count)
            {
                _patchButtons[i].TextureNormal = LoadPatchTexture(selectablePatches[i].Id);
                _patchButtons[i].Visible = i != _hiddenPatchOffset;
                _patchButtons[i].Modulate = buyablePatchOffsets.Contains(i)
                    ? Colors.White
                    : new Color(1.0f, 1.0f, 1.0f, 0.35f);
                _patchButtons[i].Disabled = !buyablePatchOffsets.Contains(i);
            }
            else
            {
                _patchButtons[i].TextureNormal = null;
                _patchButtons[i].Visible = false;
                _patchButtons[i].Disabled = true;
            }
        }
    }

    public void HidePatchAtOffset(int patchOffset)
    {
        _hiddenPatchOffset = patchOffset;
        Refresh();
    }

    public void RestoreHiddenPatch()
    {
        _hiddenPatchOffset = -1;
        Refresh();
    }

    private void OnPatchButtonDown(int patchOffset)
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

        var button = _patchButtons[patchOffset];
        var sourceCenterGlobal = button.GetGlobalRect().GetCenter();
        var dragOffsetFromCenter = GetGlobalMousePosition() - sourceCenterGlobal;

        PatchSelected?.Invoke(patchOffset, sourceCenterGlobal, dragOffsetFromCenter);
    }

    private static Texture2D LoadPatchTexture(int patchId)
    {
        return ResourceLoader.Load<Texture2D>($"{OriginalTextureDirectory}/{patchId}.png");
    }
}
