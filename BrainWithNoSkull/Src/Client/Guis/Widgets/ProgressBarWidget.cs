using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class ProgressBarWidget: Widget {

    private readonly uint _invIconsTexture;
    private readonly Func<float> _progressGetter;
    
    public ProgressBarWidget(int x, int y, float z, int width, int height, Gui parent, Func<float> progressGetter) : base(x, y, z, width, height, parent){
        _progressGetter = progressGetter;
        _invIconsTexture = ClientAssetLoader.BlockTextureAtlas.LookupGui("Inv_Icons");
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        Parent.GuiRenderer.DrawTexture9Slice(guiVb, X, Y, Z, 18, 54, 18, 18, Width, Height, 2, 2, 2, 2, 256, _invIconsTexture);
        float progress = Math.Clamp(_progressGetter(), 0, 1);
        int width = (int)MathF.Round((Width - 2) * progress);
        if (width > 0) {
            int uWidth = width == Width - 2 ? 16 : 15;
            Parent.GuiRenderer.Color = 0xff_fc_84_03;
            Parent.GuiRenderer.DrawTexture9Slice(guiVb, X + 1, Y + 1, Z - 0.001f, 55, 55, uWidth, 16, width, Height - 2, 1, 1, 1, 1, 256, _invIconsTexture);
            Parent.GuiRenderer.Color = 0xff_ff_ff_ff;
        }
    }
}