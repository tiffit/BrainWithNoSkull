using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Net;

namespace BrainWithNoSkull.Client.Guis;

public class ContainerGui: Gui {

    private ContainerSync _sync;
    
    public ContainerGui(ContainerSync sync){
        _sync = sync;
    }
    
    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
    }

    public override bool PausesGame(){
        return false;
    }
}