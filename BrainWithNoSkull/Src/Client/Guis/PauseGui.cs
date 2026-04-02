using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis;

public class PauseGui: Gui {
    
    public override void Render(VertexBuilder guiVb){
    }

    public override bool PausesGame(){
        return true;
    }
}