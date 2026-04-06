namespace BrainWithNoSkull.Client.Guis.Widgets;

public class VerticalListWidget: GroupingWidget {
    
    private const int ScrollBarWidth = 3;
    private int _nextY;
    
    public VerticalListWidget(int x, int y, float z, int width, int height, Gui parent) : base(x, y, z, width, height, parent){
    }

    public override void AddWidget(Widget widget){
        widget.X = X;
        widget.Z += Z;
        widget.Y = Y + _nextY;
        _nextY += widget.Height;
        widget.Width = Width - ScrollBarWidth;
        
        Widgets.Add(widget);
    }
}