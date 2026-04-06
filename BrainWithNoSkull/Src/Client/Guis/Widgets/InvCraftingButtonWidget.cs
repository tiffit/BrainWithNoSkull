using BrainWithNoSkull.Client.Render;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.World.ItemStacks;
using BrainWithNoSkull.World.Recipes;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public class InvCraftingButtonWidget: ButtonWidget {

    private readonly Recipe _recipe;
    private readonly FontRenderer _font;
    
    public InvCraftingButtonWidget(Recipe recipe, Gui parent, ButtonClickHandler? clickHandler) : base(0, 0, -0.02f, 0, 25, parent, clickHandler){
        _recipe = recipe;
        _font = parent.Font;
    }

    public override void Render(VertexBuilder guiVb, int mouseX, int mouseY){
        base.Render(guiVb, mouseX, mouseY);

        int x = X + 3;
        foreach (ItemStack input in _recipe.Outputs) {
            GameClient.Instance.ItemRenderer.RenderItemInGuiWithCount(guiVb, input, x + 9, Y + 4 + 9, Z - 0.02f, Parent.Font);
            Parent.GuiRenderer.DrawTexture(guiVb, x, Y + 4, Z - 0.01f, 0, 0, 18, 18, 256, InvIconsTexture);
            x += 20;
        }
        
        Parent.GuiRenderer.DrawTexture(guiVb, x + 2, Y + 10, Z - 0.01f, 90, 0, 12, 7, 256, InvIconsTexture);
        
        x += 20;
        
        foreach (ItemStack input in _recipe.Inputs) {
            GameClient.Instance.ItemRenderer.RenderItemInGuiWithCount(guiVb, input, x + 9, Y + 4 + 9, Z - 0.02f, Parent.Font);
            Parent.GuiRenderer.DrawTexture(guiVb, x, Y + 4, Z - 0.01f, 0, 0, 18, 18, 256, InvIconsTexture);
            x += 20;
        }
    }
}