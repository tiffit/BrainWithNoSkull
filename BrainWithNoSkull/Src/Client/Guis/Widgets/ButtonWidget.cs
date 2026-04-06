using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class ButtonWidget: Widget {
    
    protected readonly uint InvIconsTexture;
    protected readonly ButtonClickHandler? ClickHandler;
    
    public ButtonWidget(int x, int y, float z, int width, int height, Gui parent, ButtonClickHandler? handler) : base(x, y, z, width, height, parent){
        InvIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
        ClickHandler = handler;
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        Parent.GuiRenderer.DrawTexture9Slice(guiVb, X, Y, Z, 0, 54, 18, 18, Width, Height, 2, 2, 2, 2, 256, InvIconsTexture);
    }

    public override bool OnMouseDown(MouseButton btn, int relX, int relY){
        return ClickHandler?.Invoke(btn, relX, relY) ?? false;
    }
}

public delegate bool ButtonClickHandler(MouseButton btn, int mouseX, int mouseY);