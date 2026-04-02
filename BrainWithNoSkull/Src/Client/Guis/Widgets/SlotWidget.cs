using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class SlotWidget: Widget {

    private readonly IItemContainer? _container;
    private readonly int _slot;
    private readonly uint _invIconsTexture;
    private readonly bool _hotbar;
    
    public SlotWidget(int x, int y, float z, IItemContainer? container, int slot, uint invIconsTexture) : base(x, y, z, 19, 19){
        _container = container;
        _slot = slot;
        _invIconsTexture = invIconsTexture;
        _hotbar = container is PlayerInventory && slot < 9;
    }

    public override void Render(VertexBuilder guiVb, Gui parent, int mouseX, int mouseY){
        ItemStack? stack = _container?.GetItem(_slot);
        bool hasStack = false;
        if (!ItemStack.IsNull(stack)) {
            GameClient.Instance.ItemRenderer.RenderItemInGui(guiVb, stack, X + 9, Y+ 9, Z - 0.01f, 40, 40);
            if (stack.Count > 1) {
                parent.Font.RenderString(X, Y, Z - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
            }
            hasStack = true;
        }
        parent.DrawTexture(guiVb, X, Y, Z, hasStack ? 0 : 18, 0, 18, 18, 256, _invIconsTexture);
        if (_hotbar && ((_container as PlayerInventory)?.HotbarSlot ?? -1) == _slot) {
            parent.DrawTexture(guiVb, X, Y, Z - 0.02f, 36, 0, 18, 18, 256, _invIconsTexture);
        }
    }
}