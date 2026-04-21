using Godot;
using Patchwork.Service;

namespace Patchwork.Scenes.GameScene;

public partial class GameScene : Control
{
    public RootService RootService { get; private set; }

    public void Initialize(RootService rootService)
    {
        RootService = rootService;
        GetNode<Patchwork.Scenes.PatchShop.PatchShop>("PatchShop").Initialize(rootService);
    }
}
