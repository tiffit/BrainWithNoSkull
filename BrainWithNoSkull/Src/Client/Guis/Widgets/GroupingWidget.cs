using BrainWithNoSkull.Client.Render.Data;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class GroupingWidget: Widget {

    private readonly List<Widget> _widgets = [];
    
    public GroupingWidget(int x, int y, float z, int width, int height) : base(x, y, z, width, height){}

    public void AddWidget(Widget widget){
        widget.X += X;
        widget.Y += Y;
        widget.Z += Z;
        _widgets.Add(widget);
    }

    public override void Render(VertexBuilder guiVb, Gui parent){
        foreach (Widget widget in _widgets) {
            widget.Render(guiVb, parent);
        }
    }
}