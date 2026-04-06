using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class PanelWidget: GroupingWidget {
    private readonly uint _invIconsTexture;
    
    public PanelWidget(int x, int y, float z, int width, int height, Gui parent) : base(x, y, z, width, height, parent){
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
    }


    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        base.Render(guiVb, mouseX, mouseY);

        Parent.GuiRenderer.DrawTexture9Slice(guiVb, X, Y, Z, 0, 18, 176, 24, Width, Height, 3, 3, 3, 3, 256, _invIconsTexture);
    }
}