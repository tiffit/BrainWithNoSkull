using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class ButtonWidget: Widget {
    
    protected readonly uint InvIconsTexture;
    
    public ButtonWidget(int x, int y, float z, int width, int height, Gui parent) : base(x, y, z, width, height, parent){
        InvIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        Parent.GuiRenderer.DrawTexture9Slice(guiVb, X, Y, Z, 0, 54, 18, 18, Width, Height, 2, 2, 2, 2, 256, InvIconsTexture);
    }
}