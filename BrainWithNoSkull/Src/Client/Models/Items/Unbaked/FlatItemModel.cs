using BrainWithNoSkull.Client.Models.Blocks.Unbaked;
using BrainWithNoSkull.Client.Render.Data;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Items.Unbaked;

public class FlatItemModel: UnbakedItemModel {

    private readonly string _texture;

    public Dictionary<ItemModelTransformType, ItemModelTransforms> Transforms = new Dictionary<ItemModelTransformType, ItemModelTransforms>{
        {
            ItemModelTransformType.Gui, new ItemModelTransforms(){
                Scale = new Vector3D<float>(32, 32, 1)
            }
        }
    };
    
    public FlatItemModel(string texture){
        _texture = texture;
    }
    
    public override void GatherDependencies(){
        TextureDependencies.Add(_texture);
    }

    public override IBakedItemModel Bake(TextureResolver resolver){
        uint textureIndex = resolver(_texture);
        List<Vertex> vertices = [
            new Vertex{
                Pos = new Vector3D<float>(-1, -1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(0, 0)
            },
            new Vertex{
                Pos = new Vector3D<float>(1, -1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(1, 0)
            },
            new Vertex{
                Pos = new Vector3D<float>(1, 1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(1, 1)
            },
            
            new Vertex{
                Pos = new Vector3D<float>(-1, -1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(0, 0)
            },
            new Vertex{
                Pos = new Vector3D<float>(1, 1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(1, 1)
            },
            new Vertex{
                Pos = new Vector3D<float>(-1, 1, 0),
                Color = 0xff_ff_ff_ff,
                TextureIndex = textureIndex,
                Uv = new Vector2D<float>(0, 1)
            }
        ];

        Dictionary<ItemModelTransformType, ItemModelTransform> transforms = Transforms
            .Select(pair => new KeyValuePair<ItemModelTransformType, ItemModelTransform>(pair.Key, pair.Value.GetTransform()))
            .ToDictionary();
        
        return new SimpleBakedItemModel(vertices, transforms);
    }
}