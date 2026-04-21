using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.PatchShop;

public partial class PatchShop : Panel
{
    private RootService _rootService;
    private Patchwork.Scenes.PatchView.PatchView[] _patchViews;

    private static readonly Vector2 PatchViewSize = new(250.0f, 250.0f);
    private static readonly Vector2[] PatchViewPositions =
    [
        new Vector2(0.0f, 0.0f),
        new Vector2(255.0f, 0.0f),
        new Vector2(510.0f, 0.0f)
    ];

    public override void _Ready()
    {
        _patchViews =
        [
            GetNode<Patchwork.Scenes.PatchView.PatchView>("PatchView0"),
            GetNode<Patchwork.Scenes.PatchView.PatchView>("PatchView1"),
            GetNode<Patchwork.Scenes.PatchView.PatchView>("PatchView2")
        ];

        for (var i = 0; i < _patchViews.Length; i++)
        {
            _patchViews[i].Position = PatchViewPositions[i];
            _patchViews[i].Size = PatchViewSize;
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

        for (var i = 0; i < _patchViews.Length; i++)
        {
            if (i < selectablePatches.Count)
            {
                _patchViews[i].DisplayPatch(selectablePatches[i]);
            }
            else
            {
                _patchViews[i].Clear();
            }
        }
    }
}
