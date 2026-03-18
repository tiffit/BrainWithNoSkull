using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public class VkShaderProgram : ShaderProgram {
    private readonly Vk _vk;
    private readonly ShaderModule _vertModule;
    private readonly ShaderModule _fragModule;

    public PipelineShaderStageCreateInfo VertStage{ get; private set; }
    public PipelineShaderStageCreateInfo FragStage{ get; private set; }

    public VkShaderProgram(ShaderType shType, Vk vk, ShaderModule vertModule, ShaderModule fragModule) : base(shType){
        _vk = vk;
        _vertModule = vertModule;
        _fragModule = fragModule;
        CreateStages();
    }

    public unsafe void CreateStages(){
        VertStage = new PipelineShaderStageCreateInfo{
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = _vertModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };
        FragStage = new PipelineShaderStageCreateInfo{
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = _fragModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };
    }

    public unsafe void DisposeModules(Device device){
        _vk.DestroyShaderModule(device, _vertModule, null);
        _vk.DestroyShaderModule(device, _fragModule, null);

        SilkMarshal.Free((nint)VertStage.PName);
        SilkMarshal.Free((nint)FragStage.PName);
    }

    public static unsafe ShaderModule CreateModule(Vk vk, Device device, byte[] data){
        fixed (byte* dataPtr = data) {
            ShaderModuleCreateInfo createInfo = new(){
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)data.Length,
                PCode = (uint*)dataPtr
            };
            if (vk.CreateShaderModule(device, in createInfo, null, out ShaderModule shaderModule) != Result.Success) {
                throw new Exception("Unable to create shader module");
            }

            return shaderModule;
        }
    }
}