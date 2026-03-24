using BrainWithNoSkull.Client.Render.Shader;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;

public class VkShaderProgram : ShaderProgram {
    private readonly Vk _vk;
    private readonly ShaderModule _vertModule;
    private readonly ShaderModule _fragModule;
    private readonly ShaderModule _vertShadowModule;
    private readonly ShaderModule _fragShadowModule;

    public PipelineShaderStageCreateInfo VertStage{ get; private set; }
    public PipelineShaderStageCreateInfo FragStage{ get; private set; }
    
    public PipelineShaderStageCreateInfo VertStageShadow{ get; private set; }
    public PipelineShaderStageCreateInfo FragStageShadow{ get; private set; }

    public VkShaderProgram(ShaderType shType, Vk vk, ShaderModule vertModule, ShaderModule fragModule, ShaderModule vertShadowModule, ShaderModule fragShadowModule) : base(shType){
        _vk = vk;
        _vertModule = vertModule;
        _fragModule = fragModule;
        _vertShadowModule = vertShadowModule;
        _fragShadowModule = fragShadowModule;
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
        
        VertStageShadow = new PipelineShaderStageCreateInfo{
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = _vertShadowModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };
        FragStageShadow = new PipelineShaderStageCreateInfo{
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = _fragShadowModule,
            PName = (byte*)SilkMarshal.StringToPtr("main")
        };
    }

    public unsafe void DisposeModules(Device device){
        _vk.DestroyShaderModule(device, _vertModule, null);
        _vk.DestroyShaderModule(device, _fragModule, null);
        _vk.DestroyShaderModule(device, _vertShadowModule, null);
        _vk.DestroyShaderModule(device, _fragShadowModule, null);

        SilkMarshal.Free((nint)VertStage.PName);
        SilkMarshal.Free((nint)FragStage.PName);
        SilkMarshal.Free((nint)VertStageShadow.PName);
        SilkMarshal.Free((nint)FragStageShadow.PName);
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