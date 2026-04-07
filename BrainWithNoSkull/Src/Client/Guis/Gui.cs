using BrainWithNoSkull.Client.Guis.Widgets;
using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Net.Pckt.Sb;
using BrainWithNoSkull.World;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Guis;

public abstract class Gui {

    public IRenderEngine RenderEngine = null!;
    protected int ScaledWidth, ScaledHeight;
    public FontRenderer Font = null!;
    public GuiRenderer GuiRenderer = null!;

    public IItemContainer? HeldStackContainer;
    public int HeldStackIndex;

    protected List<Widget> Widgets = [];

    public virtual void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        Widgets.Clear();
        RenderEngine = renderEngine;
        ScaledWidth = scaledWidth;
        ScaledHeight = scaledHeight;
        Font = font;
        GuiRenderer = new GuiRenderer(this, renderEngine);
    }

    public void ReInit(){
        Init(RenderEngine, Font, ScaledWidth, ScaledHeight);
    }

    public void RenderBase(VertexBuilder guiVb){
        GetScaledMousePos(out int mouseX, out int mouseY);

        foreach (Widget widget in Widgets) {
            widget.Render(guiVb, mouseX, mouseY);
        }

        Render(guiVb, mouseX, mouseY);
    }

    public abstract void Render(VertexBuilder guiVb, int mouseX, int mouseY);
    
    public virtual bool PausesGame(){
        return false;
    }

    public void HandleMouseButtonInput(MouseButton btn, bool down){
        GetScaledMousePos(out int mouseX, out int mouseY);
        OnMouseDown(btn, mouseX, mouseY);
        foreach (Widget widget in Widgets) {
            if (widget.IsWithin(mouseX, mouseY)) {
                widget.OnMouseDown(btn, mouseX - widget.X, mouseY - widget.Y);
            }
        }
    }

    protected virtual void OnMouseDown(MouseButton btn, int mouseX, int mouseY){
    }

    private static void GetScaledMousePos(out int mouseX, out int mouseY){
        Vector2D<float> mousePos = GameClient.Instance.InputHandler.MousePosition;
        mouseX = (int)(mousePos.X / GameClient.Instance.GuiScale);
        mouseY = (int)(mousePos.Y / GameClient.Instance.GuiScale);
    }

    public void OnSlotClicked(int slot, IItemContainer container){
        ItemStack? stack = container.GetItem(slot);

        if (HeldStackContainer != null) {
            if (container == HeldStackContainer && slot == HeldStackIndex) {
                HeldStackContainer = null;
                return;
            }
            ItemStack? cursorStack = GetCursorStack();
            container.CombineStacks(slot, ref cursorStack, true);
            HeldStackContainer.SetItemInSlot(cursorStack, HeldStackIndex);
            
            GameClient.Instance.ServerConnection.SendPacket(new SbMoveInventoryItemsPacket(HeldStackContainer.GetContainerId(), HeldStackIndex, container.GetContainerId(), slot));
            
            if (ItemStack.IsNull(cursorStack)) {
                HeldStackContainer = null;
            }
        }else if (!ItemStack.IsNull(stack)) {
            HeldStackContainer = container;
            HeldStackIndex = slot;
        }
    }

    public ItemStack? GetCursorStack(){
        return HeldStackContainer?.GetItem(HeldStackIndex);
    }

}