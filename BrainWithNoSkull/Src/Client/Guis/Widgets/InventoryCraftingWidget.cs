using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.World.Recipes;

namespace BrainWithNoSkull.Client.Guis.Widgets;

public sealed class InventoryCraftingWidget: PanelWidget {
    
    public InventoryCraftingWidget(int x, int y, float z, int width, int height, Gui parent) : base(x, y, z, width, height, parent){
        VerticalListWidget list = new VerticalListWidget(4, 20, 0.01f, width - 4, height - 20 - 4, parent);
        AddWidget(list);
        
        foreach (Recipe recipe in RecipeRegistry.Registry.GetEntries()) {
            list.AddWidget(new InvCraftingButtonWidget(recipe, parent, (_, _, _) => {
                OnCraftingButtonClicked(recipe);
                return true;
            }));
        }
    }

    private void OnCraftingButtonClicked(Recipe recipe){
        ClientPlayerEntity? player = GameClient.Instance.ServerConnection.PlayerEntity;
        if (player != null) {
            Console.WriteLine(recipe.CanCraft(player.Inventory));
        }
    }
    
}