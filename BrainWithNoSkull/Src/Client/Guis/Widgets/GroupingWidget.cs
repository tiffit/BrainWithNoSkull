using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Input;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class GroupingWidget: Widget {

    protected readonly List<Widget> Widgets = [];
    
    public GroupingWidget(int x, int y, float z, int width, int height, Gui parent) : base(x, y, z, width, height, parent){}

    public virtual void AddWidget(Widget widget){
        widget.X += X;
        widget.Y += Y;
        widget.Z += Z;
        Widgets.Add(widget);
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        foreach (Widget widget in Widgets) {
            widget.Render(guiVb, mouseX, mouseY);
        }
    }

    public override bool OnMouseDown(MouseButton btn, int relX, int relY){
        bool any = false;
        foreach (Widget widget in Widgets) {
            if (widget.IsWithin(relX + X, relY + Y)) {
                if (widget.OnMouseDown(btn, widget.X - relX, widget.Y - relY)) {
                    any = true;
                }
            }
        }
        return any;
    }
}