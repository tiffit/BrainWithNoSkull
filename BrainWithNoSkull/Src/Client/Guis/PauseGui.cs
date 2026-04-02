using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis;

public class PauseGui: Gui {
    
    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
    }

    public override bool PausesGame(){
        return true;
    }
}