using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.Entities.Player;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class SlotWidget: Widget {

    private readonly IItemContainer? _container;
    private readonly int _slot;
    private readonly uint _invIconsTexture;
    private readonly bool _hotbar;
    
    public SlotWidget(int x, int y, float z, Gui parent, IItemContainer? container, int slot, uint invIconsTexture) : base(x, y, z, 19, 19, parent){
        _container = container;
        _slot = slot;
        _invIconsTexture = invIconsTexture;
        _hotbar = container is PlayerInventory && slot < 9;
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        ItemStack? stack = _container?.GetItem(_slot);
        bool hasStack = false;
        if (!ItemStack.IsNull(stack)) {
            if (IsHeldStack()) {
                DrawDashedLine(mouseX, mouseY);
                GameClient.Instance.ItemRenderer.RenderItemInGui(guiVb, stack, mouseX, mouseY, 0.1f, 40, 40);
                if (stack.Count > 1) {
                    Parent.Font.RenderString(mouseX - 9, mouseY - 9, 0.1f - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
                }
            }
            else {
                GameClient.Instance.ItemRenderer.RenderItemInGui(guiVb, stack, X + 9, Y+ 9, Z - 0.01f, 40, 40);
                if (stack.Count > 1) {
                    Parent.Font.RenderString(X, Y, Z - 0.03f, "" + stack.Count, ColorUtil.FromFloats(1f, 0.65f, 0f));
                }
            }
            hasStack = true;
        }
        Parent.GuiRenderer.DrawTexture(guiVb, X, Y, Z, hasStack ? 0 : 18, 0, 18, 18, 256, _invIconsTexture);
        if (_hotbar && ((_container as PlayerInventory)?.HotbarSlot ?? -1) == _slot) {
            Parent.GuiRenderer.DrawTexture(guiVb, X, Y, Z - 0.02f, 36, 0, 18, 18, 256, _invIconsTexture);
        }
    }

    public override bool OnMouseDown(MouseButton btn, int relX, int relY){
        if (_container != null) {
            Parent.OnSlotClicked(_slot, _container);
        }
        return true;
    }

    private bool IsHeldStack(){
        return _container != null && Parent.HeldStackContainer == _container && Parent.HeldStackIndex == _slot;
    }

    private void DrawDashedLine(int mouseX, int mouseY){
        Parent.GuiRenderer.DrawLine(X + Width/2f, Y + Height/2f, mouseX, mouseY, 0.11f, 5, 0xff_ff_ff_ff);
    }
}