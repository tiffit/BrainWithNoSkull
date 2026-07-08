using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine;
using BrainWithNoSkull.Net.Pckt.Sb;
using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Guis;

public class ChatGui : Gui {

    private String _input = "";
    
    public override void Init(IRenderEngine renderEngine, FontRenderer font, int scaledWidth, int scaledHeight){
        base.Init(renderEngine, font, scaledWidth, scaledHeight);
    }
    
    public override void Render(VertexBuilder vb, int mouseX, int mouseY){
        GuiRenderer.DrawSolid(vb, 0, ScaledHeight/2, 0.99f, ScaledWidth/2, ScaledHeight/2, 0x50_20_20_20);
        Font.RenderString(5, ScaledHeight - 15, 0.98f, _input, 0xff_ff_ff_ff);
    }

    protected override void OnKeyDown(Key key){
        switch (key) {
            case Key.Backspace: {
                if (_input.Length > 0) {
                    _input = _input[..^1];
                }

                break;
            }
            case Key.Enter:
                if (_input.Length > 0) {
                    GameClient.Instance.ServerConnection.SendPacket(new SbChatPacket(_input));
                }
                GameClient.Instance.SetGui(null);
                break;
        }
    }

    protected override void OnCharTyped(char chr){
        _input += chr;
    }
}