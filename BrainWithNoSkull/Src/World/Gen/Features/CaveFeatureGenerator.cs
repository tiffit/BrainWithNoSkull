using BrainWithNoSkull.Client.World;
using BrainWithNoSkull.Registries;
using BrainWithNoSkull.Util;
using BrainWithNoSkull.World.Gen.Chunks;
using Silk.NET.Maths;

namespace BrainWithNoSkull.World.Gen.Features;

public class CaveFeatureGenerator: IFeatureGenerator {

    private static readonly SimplexHigherD Noise5D = new SimplexHigherD(5);
    private readonly WorldSettings _settings;
    private readonly long _seed;
    private readonly RandomSource _random;

    public CaveFeatureGenerator(WorldSettings settings){
        _seed = settings.Seed - "Caves".GetHashCode();
        _random = new RandomSource(_seed);
        _settings = settings;
    }
    
    public void GenerateFeatures(WorldBase world, Vector2D<int> coords, IChunkGenerator chunkGen, GenChunk genChunk){

        Dictionary<Vector3D<int>, CaveSection> caveSections = new Dictionary<Vector3D<int>, CaveSection>(40);
        AABB chunkBox = AABB.FromExtents(coords.X * 16, 0, coords.Y * 16, 16, Constants.MaxWorldHeight, 16);
        
        for (int x = -1; x <= 1; x++) {
            for (int y = 0; y < 160/Constants.ChunkSize; y++) {
                for (int z = -1; z <= 1; z++) {
                    Vector3D<int> offset = new Vector3D<int>(x, y, z);
                    CaveSection? section = GetSection(in offset, in coords);
                    if (section != null) {
                        caveSections[offset] = section;
                    }
                }
            }
        }

        foreach ((Vector3D<int> offset, CaveSection section) in caveSections) {
            //Create cave section spheres
            for (int x = -section.Radius; x <= section.Radius; x++) {
                for (int y = -section.Radius; y <= section.Radius; y++) {
                    for (int z = -section.Radius; z <= section.Radius; z++) {
                        if (x * x + y * y + z * z < section.Radius * section.Radius) {
                            int chunkX = (int)section.Center.X + x - coords.X * 16;
                            int chunkY = (int)section.Center.Y + y;
                            int chunkZ = (int)section.Center.Z + z - coords.Y * 16;
                            genChunk.SetBlock(chunkX, chunkY, chunkZ, BlockRegistry.Air);
                        }
                    }
                }
            }

            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    for (int z = -1; z <= 1; z++) {
                        if ((x | y | z) == 0)
                            continue;
                        Vector3D<int> otherPos = offset + new Vector3D<int>(x, y, z);
                        if (caveSections.TryGetValue(otherPos, out CaveSection? other)) {
                            Vector3D<float> max = Vector3D.Max(section.GetMaxRadius(), other.GetMaxRadius());
                            Vector3D<float> min = Vector3D.Min(section.GetMinRadius(), other.GetMinRadius());
                            AABB frustumBox = AABB.And(new AABB(min, max), chunkBox);

                            int minX = (int)MathF.Floor(frustumBox.MinX);
                            int minY = (int)MathF.Floor(frustumBox.MinY);
                            int minZ = (int)MathF.Floor(frustumBox.MinZ);
                            
                            int maxX = (int)MathF.Ceiling(frustumBox.MaxX);
                            int maxY = (int)MathF.Ceiling(frustumBox.MaxY);
                            int maxZ = (int)MathF.Ceiling(frustumBox.MaxZ);
                            
                            for (int blockX = minX; blockX < maxX; blockX++) {
                                for (int blockY = minY; blockY < maxY; blockY++) {
                                    for (int blockZ = minZ; blockZ < maxZ; blockZ++) {
                                        if (IsInFrustum(new Vector3D<float>(blockX, blockY, blockZ), section, other)) {
                                            genChunk.Blocks[blockX - coords.X * 16, blockY, blockZ - coords.Y * 16] = BlockRegistry.Air;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsInFrustum(Vector3D<float> point, CaveSection section1, CaveSection section2){
        Vector3D<float> v = section2.Center - section1.Center;
        float length = v.Length;

        if (length < 0.001f)
            return false;

        Vector3D<float> norm = Vector3D.Normalize(v);
        Vector3D<float> w = point - section1.Center;

        float h = Vector3D.Dot(w, norm);

        if (h < 0 || h > length) {
            return false;
        }

        float currentRadius = section1.Radius + (h / length) * (section2.Radius - section1.Radius);

        float wSq = w.LengthSquared;
        float dSq = wSq - (h * h);
        return dSq < currentRadius * currentRadius;
    }

    private CaveSection? GetSection(in Vector3D<int> offset, in Vector2D<int> coords){
        Vector3D<int> actualSection = new Vector3D<int>(coords.X + offset.X, offset.Y, coords.Y + offset.Z);
        
        float angleX = MathF.PI * 2 * actualSection.X / _settings.WorldSize;
        float angleZ = MathF.PI * 2 * actualSection.Z / _settings.WorldSize;
        float scale = _settings.WorldSize / (4 * 8);
        float noiseX = MathF.Cos(angleX) * scale;
        float noiseY = MathF.Sin(angleX) * scale;
        float noiseZ = MathF.Cos(angleZ) * scale;
        float noiseW = MathF.Sin(angleZ) * scale;

        float[] loc = [noiseX, noiseY, noiseZ, noiseW, actualSection.Y];
        float n = Noise5D.Noise(_settings.Seed, loc);
        if (n > 0.3f) {
            _random.SetSeed(_seed + actualSection.GetHashCode());
            Vector3D<float> center = new Vector3D<float>(4 + _random.NextInt(8), 4 + _random.NextInt(8), 4 + _random.NextInt(8));
            center += new Vector3D<float>(actualSection.X * 16, actualSection.Y * 16, actualSection.Z * 16);
            int radius = (int)(n * 7);
            CaveSection section = new CaveSection(center, radius);
            return section;
        }
        return null;
    }
    
    private class CaveSection {
        internal readonly Vector3D<float> Center;
        internal readonly int Radius;

        public CaveSection(Vector3D<float> center, int radius){
            Center = center;
            Radius = radius;
        }

        public Vector3D<float> GetMaxRadius(){
            return new Vector3D<float>(Center.X + Radius, Center.Y + Radius, Center.Z + Radius);
        }
        public Vector3D<float> GetMinRadius(){
            return new Vector3D<float>(Center.X - Radius, Center.Y - Radius, Center.Z - Radius);
        }
    }
}