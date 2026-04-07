using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.World.ItemStacks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.Client.Models.Items;

public interface IBakedItemModel {
    IEnumerable<Vertex> GetVertices(ItemStack stack);

    ItemModelTransform GetItemModelTransform(ItemModelTransformType type);
}

public class SimpleBakedItemModel: IBakedItemModel {

    private readonly List<Vertex> _vertices;
    private readonly Dictionary<ItemModelTransformType, ItemModelTransform> _transforms;
    
    public SimpleBakedItemModel(List<Vertex> vertices, Dictionary<ItemModelTransformType, ItemModelTransform> transforms){
        _vertices = vertices;
        _transforms = transforms;
    }
    
    public IEnumerable<Vertex> GetVertices(ItemStack stack){
        return _vertices;
    }

    public ItemModelTransform GetItemModelTransform(ItemModelTransformType type){
        return _transforms.TryGetValue(type, out ItemModelTransform? transform) ? transform : ItemModelTransform.Identity;
    }
}

public enum ItemModelTransformType {
    Gui,
    World
}

public class ItemModelTransform {
    public Matrix4X4<float> Transform;
    public Matrix4X4<float> Rotation;

    public static readonly ItemModelTransform Identity = new ItemModelTransform{ Transform = Matrix4X4<float>.Identity, Rotation = Matrix4X4<float>.Identity };
}

public class ItemModelTransforms {
    public Vector3D<float> Translation = new Vector3D<float>();
    public Vector3D<float> Rotation = new Vector3D<float>();
    public Vector3D<float> Scale = new Vector3D<float>();

    public ItemModelTransform GetTransform(){
        ItemModelTransform transform = new ItemModelTransform();
        
        Matrix4X4<float> matrix = Matrix4X4.CreateTranslation(Translation);
        
        Quaternion<float> quat = Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(1, 0, 0), float.DegreesToRadians(Rotation.X));
        quat *= Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0, 1, 0), float.DegreesToRadians(Rotation.Y));
        quat *= Quaternion<float>.CreateFromAxisAngle(new Vector3D<float>(0, 0, 1), float.DegreesToRadians(Rotation.Z));

        transform.Rotation = Matrix4X4.CreateFromQuaternion(quat);
        matrix *= transform.Rotation;
        matrix *= Matrix4X4.CreateScale(Scale);

        transform.Transform = matrix;

        return transform;
    }
}