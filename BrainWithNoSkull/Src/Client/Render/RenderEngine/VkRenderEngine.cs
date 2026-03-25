using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrainWithNoSkull.Client.Render.Data;
using BrainWithNoSkull.Client.Render.RenderEngine.Vulkan;
using BrainWithNoSkull.Client.Render.Shader;
using BrainWithNoSkull.Util;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using SkiaSharp;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace BrainWithNoSkull.Client.Render.RenderEngine;

public unsafe class VkRenderEngine : IRenderEngine {
    public static readonly VkRenderEngine Instance = new VkRenderEngine();

#if DEBUG
    private static bool _enableValidationLayer = true;
#else
    private static bool _enableValidationLayer = false;
#endif

    public const int ShadowMapCascadeCount = 4;
    private const uint ShadowMapSize = 4096;
    
    private readonly Vk _vk = Vk.GetApi();
    private IWindow? _window;
    private Camera _camera = null!;
    private Instance _instance;

    private PhysicalDevice _physicalDevice;
    private PhysicalDeviceLimits _physicalDeviceLimits;
    private Device _logicalDevice;

    private QueueFamilies _queueFamilies;
    private Queue _graphicsQueue;
    private Queue _presentQueue;

    private SurfaceKHR _surface;
    private KhrSurface? _surfaceExtension;

    private SwapchainKHR _swapChain;
    private KhrSwapchain? _swapChainExtension;
    private Image[] _swapChainImages = [];
    private ImageView[] _swapChainImageViews = [];
    private SurfaceFormatKHR _format;
    private Extent2D _extent;

    private RenderPass _renderPass;

    private readonly Dictionary<ShaderType, VkShaderPipeline> _pipelines = [];

    private Framebuffer[]? _framebuffers;

    private CommandPool _commandPool;
    private CommandBuffer[] _commandBuffers = [];

    private Image _textureImage;
    private DeviceMemory _textureImageMemory;
    private ImageView _textureImageView;
    private Sampler _textureSampler;

    private Image _depthImage;
    private DeviceMemory _depthImageMemory;
    private ImageView _depthImageView;

    private RenderPass _shadowRenderPass;
    private Image _shadowImage;
    private DeviceMemory _shadowImageMemory;
    private ImageView _shadowImageView;
    private Sampler _shadowSampler;
    private readonly VkCascade?[] _cascades = new VkCascade[ShadowMapCascadeCount];

    private Semaphore[] _imageAvailableSemaphore = [];
    private Semaphore[] _renderFinishedSemaphore = [];
    private Fence[] _fence = [];
    private uint _imageCount;
    private int _frameIndex;
    private bool _hasResized;
    private bool _minimized;

    private static readonly string[] RequiredDeviceExtensions = [KhrSwapchain.ExtensionName];
    private static readonly string[] ValidationLayers = ["VK_LAYER_KHRONOS_validation"];

    public void Init(IWindow window, Camera camera){
        _window = window;
        _camera = camera;
        if (_window.VkSurface is null) {
            throw new Exception("Vulkan not supported");
        }

        _window.FramebufferResize += size => {
            _hasResized = true;
            _minimized = size is{ X: 0, Y: 0 };
        };

        InitVk();
    }

    private void InitVk(){
        CreateVkInstance();
        CreateSurface();
        SelectGraphicsDevice();
        CreateLogicalDevice();
        CreateSwapChain();
        CreateImageViews();
        CreateRenderPass();
        CreateDepthResources();
        CreateShadowMappingResources();
        CreateTextureImage();
        CreateTextureSampler();
        foreach (ShaderType details in ShaderType.AllShaders) {
            VkShaderProgram program = CreateShader(details);
            VkShaderPipeline pipeline = new VkShaderPipeline(program, in _physicalDeviceLimits);
            CreateDescriptorSetLayout(pipeline);
            CreateGraphicsPipeline(pipeline);
            CreateVertexBuffer(pipeline);
            CreateUniformBuffers(pipeline);
            CreateDescriptorPool(pipeline);
            CreateDescriptorSets(pipeline);
            _pipelines[details] = pipeline;
        }

        CreateFramebuffers();
        CreateCommandPool();
        CreateCommandBuffer();
        CreateSyncObjects();
    }

    private void RecreateSwapChain(){
        _vk.DeviceWaitIdle(_logicalDevice);

        CleanupSwapChain();
        if (!_minimized) {
            CreateSwapChain();
            CreateImageViews();
            CreateDepthResources();
            CreateFramebuffers();
            CreateSyncObjects();
        }
    }

    private void CreateVkInstance(){
        byte* appName = (byte*)Marshal.StringToHGlobalAnsi("a");
        if (_enableValidationLayer && !CheckValidationLayerSupport()) {
            Console.Error.WriteLine("Validation layers requested, but not available!");
            _enableValidationLayer = false;
        }

        ApplicationInfo appInfo = new(){
            SType = StructureType.ApplicationInfo,
            PApplicationName = appName,
            ApiVersion = Vk.Version13
        };

        string[] extensions = GetRequiredInstanceExtensions();
        InstanceCreateInfo createInfo = new(){
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo,
            EnabledExtensionCount = (uint)extensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions),
            EnabledLayerCount = 0
        };

        if (_enableValidationLayer) {
            createInfo.EnabledLayerCount = (uint)ValidationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(ValidationLayers);

            ValidationFeatureEnableEXT* enabledFeatures = stackalloc[]{ ValidationFeatureEnableEXT.DebugPrintfExt, ValidationFeatureEnableEXT.BestPracticesExt };
            ValidationFeaturesEXT validationFeatures = new(){
                SType = StructureType.ValidationFeaturesExt,
                EnabledValidationFeatureCount = 2,
                PEnabledValidationFeatures = enabledFeatures
            };
            createInfo.PNext = &validationFeatures;
        }

        if (_vk.CreateInstance(in createInfo, null, out _instance) != Result.Success) {
            throw new Exception("Unable to create VK instance");
        }

        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        if (_enableValidationLayer) {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }

        Marshal.FreeHGlobal((IntPtr)appName);
    }

    private string[] GetRequiredInstanceExtensions(){
        byte** glfwExtensions = _window!.VkSurface!.GetRequiredExtensions(out uint glfwExtensionCount);
        string[] extensions = SilkMarshal.PtrToStringArray((nint)glfwExtensions, (int)glfwExtensionCount);

        if (_enableValidationLayer) {
            extensions = extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }

        return extensions;
    }

    private void CreateSurface(){
        if (!_vk.TryGetInstanceExtension<KhrSurface>(_instance, out _surfaceExtension))
            throw new NotSupportedException("KHR_surface extension not found");

        _surface = _window!.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();
    }

    private void CreateLogicalDevice(){
        _queueFamilies = FindQueueFamilies(_physicalDevice);

        DeviceQueueCreateInfo* createInfoPtr = CreateQueueFamilyStructs(_queueFamilies, out uint numQueueFamilies, out GlobalMemory queueMemory);
        PhysicalDeviceFeatures deviceFeatures = new(){
            SamplerAnisotropy = true,
            DepthClamp = true
        };

        PhysicalDeviceVulkan13Features features13 = new(){
            SType = StructureType.PhysicalDeviceVulkan13Features,
            Synchronization2 = true
        };
        PhysicalDeviceVulkan12Features features12 = new(){
            SType = StructureType.PhysicalDeviceVulkan12Features,
            SeparateDepthStencilLayouts = true,
            PNext = &features13
        };
        DeviceCreateInfo createInfo = new(){
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = numQueueFamilies,
            PQueueCreateInfos = createInfoPtr,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)RequiredDeviceExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(RequiredDeviceExtensions),
            EnabledLayerCount = 0,
            PNext = &features12
        };

        if (_vk.CreateDevice(_physicalDevice, in createInfo, null, out _logicalDevice) != Result.Success) {
            throw new Exception("Unable to create logical device!");
        }

        _vk.GetDeviceQueue(_logicalDevice, _queueFamilies.Graphics!.Value, 0, out _graphicsQueue);
        _vk.GetDeviceQueue(_logicalDevice, _queueFamilies.Present!.Value, 0, out _presentQueue);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        queueMemory.Dispose();
    }

    private DeviceQueueCreateInfo* CreateQueueFamilyStructs(QueueFamilies indices, out uint arraySize, out GlobalMemory mem){
        uint[] uniqueIndices = new[]{ indices.Graphics!.Value, indices.Present!.Value }
            .Distinct().ToArray();

        mem = GlobalMemory.Allocate(uniqueIndices.Length * sizeof(DeviceQueueCreateInfo));
        DeviceQueueCreateInfo* createInfoPtr = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        float queuePriority = 1.0f;
        for (int i = 0; i < uniqueIndices.Length; i++) {
            createInfoPtr[i] = new DeviceQueueCreateInfo{
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueIndices[i],
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        arraySize = (uint)uniqueIndices.Length;
        return createInfoPtr;
    }

    private void SelectGraphicsDevice(){
        IReadOnlyCollection<PhysicalDevice> devices = _vk.GetPhysicalDevices(_instance);

        if (devices.Count == 0) throw new Exception("No valid vk GPU found!");

        foreach (PhysicalDevice device in devices) {
            if (IsValidGraphicsDevice(device)) {
                _physicalDevice = device;
                _physicalDeviceLimits = _vk.GetPhysicalDeviceProperties(_physicalDevice).Limits;
                break;
            }
        }

        if (_physicalDevice.Handle == 0)
            throw new Exception("No valid GPU found for this application!");
    }

    private bool IsValidGraphicsDevice(PhysicalDevice device){
        QueueFamilies indices = FindQueueFamilies(device);
        if (!indices.Complete)
            return false;

        if (!CheckExtensionSupport(device))
            return false;

        SwapChainSupportDetails swapChainSupportDetails = QuerySwapChainSupport(device);
        if (swapChainSupportDetails.Formats.Length == 0 || swapChainSupportDetails.PresentModes.Length == 0)
            return false;

        PhysicalDeviceFeatures supportedFeatures = _vk.GetPhysicalDeviceFeatures(device);
        if (!supportedFeatures.SamplerAnisotropy)
            return false;

        return true;
    }

    private QueueFamilies FindQueueFamilies(PhysicalDevice device){
        QueueFamilies indices = new QueueFamilies();

        uint count = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, null);

        QueueFamilyProperties[] properties = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* ptr = properties) {
            _vk.GetPhysicalDeviceQueueFamilyProperties(device, ref count, ptr);
        }

        for (uint i = 0; i < properties.Length; i++) {
            QueueFamilyProperties property = properties[i];
            if (property.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
                indices.Graphics = i;

            _surfaceExtension!.GetPhysicalDeviceSurfaceSupport(device, i, _surface, out Bool32 presentSupported);
            if (presentSupported)
                indices.Present = i;

            if (indices.Complete)
                break;
        }

        return indices;
    }


    private bool CheckExtensionSupport(PhysicalDevice device){
        uint count = 0;
        _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, null);
        ExtensionProperties[] available = new ExtensionProperties[count];
        fixed (ExtensionProperties* availablePtr = available) {
            _vk.EnumerateDeviceExtensionProperties(device, (byte*)null, ref count, availablePtr);
        }

        HashSet<string?> availableStr = available.Select(extension => Marshal.PtrToStringAnsi((IntPtr)extension.ExtensionName)).ToHashSet();
        return RequiredDeviceExtensions.All(extName => availableStr.Contains(extName));
    }

    private SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device){
        SwapChainSupportDetails details = default;

        _surfaceExtension!.GetPhysicalDeviceSurfaceCapabilities(device, _surface, out details.Capabilities);

        uint formatCount = 0;
        _surfaceExtension.GetPhysicalDeviceSurfaceFormats(device, _surface, ref formatCount, null);
        details.Formats = new SurfaceFormatKHR[formatCount];
        if (formatCount > 0) {
            fixed (SurfaceFormatKHR* formatsPtr = details.Formats) {
                _surfaceExtension.GetPhysicalDeviceSurfaceFormats(device, _surface, ref formatCount, formatsPtr);
            }
        }

        uint modeCount = 0;
        _surfaceExtension.GetPhysicalDeviceSurfacePresentModes(device, _surface, ref modeCount, null);
        details.PresentModes = new PresentModeKHR[modeCount];
        if (modeCount > 0) {
            fixed (PresentModeKHR* modesPtr = details.PresentModes) {
                _surfaceExtension.GetPhysicalDeviceSurfacePresentModes(device, _surface, ref modeCount, modesPtr);
            }
        }

        return details;
    }

    private void CreateSwapChain(){
        SwapChainSupportDetails details = QuerySwapChainSupport(_physicalDevice);

        _format = details.ChooseSwapSurfaceFormat();
        _extent = details.ChooseSwapExtent(_window!);
        SwapchainCreateInfoKHR createInfo = new(){
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = _surface,
            MinImageCount = details.ChooseImageCount(),
            ImageFormat = _format.Format,
            ImageColorSpace = _format.ColorSpace,
            ImageExtent = _extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit,

            PreTransform = details.Capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = details.ChooseSwapPresentMode(),
            Clipped = true,
            OldSwapchain = default
        };

        uint* queueFamilyIndices = stackalloc[]{ _queueFamilies.Graphics!.Value, _queueFamilies.Present!.Value };

        if (_queueFamilies.Graphics != _queueFamilies.Present) {
            createInfo.ImageSharingMode = SharingMode.Concurrent;
            createInfo.QueueFamilyIndexCount = 2;
            createInfo.PQueueFamilyIndices = queueFamilyIndices;
        }
        else {
            createInfo.ImageSharingMode = SharingMode.Exclusive;
        }

        if (!_vk.TryGetDeviceExtension<KhrSwapchain>(_instance, _logicalDevice, out _swapChainExtension)) {
            throw new NotSupportedException("VK_KHR_swapchain extension not found");
        }

        if (_swapChainExtension!.CreateSwapchain(_logicalDevice, in createInfo, null, out _swapChain) != Result.Success) {
            throw new Exception("Unable to create swap chain");
        }

        _swapChainExtension.GetSwapchainImages(_logicalDevice, _swapChain, ref _imageCount, null);
        _swapChainImages = new Image[_imageCount];
        fixed (Image* swapChainImagesPtr = _swapChainImages) {
            _swapChainExtension.GetSwapchainImages(_logicalDevice, _swapChain, ref _imageCount, swapChainImagesPtr);
        }
    }

    private bool CheckValidationLayerSupport(){
        uint count = 0;
        _vk.EnumerateInstanceLayerProperties(ref count, null);
        var available = new LayerProperties[count];
        fixed (LayerProperties* availablePtr = available) {
            _vk.EnumerateInstanceLayerProperties(ref count, availablePtr);
        }

        var availableStr = available.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

        return ValidationLayers.All(layerName => availableStr.Contains(layerName));
    }

    private void CreateImageViews(){
        _swapChainImageViews = new ImageView[_imageCount];

        for (int i = 0; i < _imageCount; i++) {
            _swapChainImageViews[i] = VkImages.CreateImageView(_vk, _logicalDevice, _swapChainImages[i], _format.Format);
        }
    }

    private void CreateDescriptorSetLayout(VkShaderPipeline shaderPipeline){
        DescriptorSetLayoutBinding uboLayoutBinding = new(){
            Binding = 0,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit
        };

        DescriptorSetLayoutBinding samplerLayoutBinding = new(){
            Binding = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
            PImmutableSamplers = null,
            StageFlags = ShaderStageFlags.FragmentBit
        };
        
        DescriptorSetLayoutBinding shadowUboLayoutBinding = new(){
            Binding = 2,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.VertexBit | ShaderStageFlags.FragmentBit
        };
        
        DescriptorSetLayoutBinding shadowMapSamplerLayoutBinding = new(){
            Binding = 3,
            DescriptorType = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
            PImmutableSamplers = null,
            StageFlags = ShaderStageFlags.FragmentBit
        };
        
        DescriptorSetLayoutBinding shadowMapDataLayoutBinding = new(){
            Binding = 4,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.FragmentBit
        };
        DescriptorSetLayoutBinding* layoutBindings = stackalloc[]{ uboLayoutBinding, samplerLayoutBinding, shadowUboLayoutBinding, shadowMapSamplerLayoutBinding, shadowMapDataLayoutBinding };

        DescriptorSetLayoutCreateInfo createInfo = new(){
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = 5,
            PBindings = layoutBindings
        };

        if (_vk.CreateDescriptorSetLayout(_logicalDevice, &createInfo, null, out shaderPipeline.DescriptorSetLayout) != Result.Success) {
            throw new Exception("Unable to create descriptor set layout");
        }
    }

    private void CreateGraphicsPipeline(VkShaderPipeline shaderPipeline){
        PipelineShaderStageCreateInfo* shaderStages = stackalloc[]{
            shaderPipeline.ShaderProgram.VertStage,
            shaderPipeline.ShaderProgram.FragStage
        };

        VertexInputBindingDescription bindingDescription = shaderPipeline.GetBindingDescription();
        VertexInputAttributeDescription[] attributeDescriptions = shaderPipeline.GetAttributeDescriptions();

        PipelineVertexInputStateCreateInfo vertexInput = new(){
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 1,
            VertexAttributeDescriptionCount = (uint)attributeDescriptions.Length,
            PVertexBindingDescriptions = &bindingDescription
        };
        fixed (VertexInputAttributeDescription* ptr = attributeDescriptions) {
            vertexInput.PVertexAttributeDescriptions = ptr;
        }

        PipelineInputAssemblyStateCreateInfo inputAssembly = new(){
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.TriangleList,
            PrimitiveRestartEnable = false,
        };

        DynamicState* dynamicStates = stackalloc[]{
            DynamicState.Viewport,
            DynamicState.Scissor
        };

        PipelineDynamicStateCreateInfo dynamicState = new(){
            SType = StructureType.PipelineDynamicStateCreateInfo,
            DynamicStateCount = 2,
            PDynamicStates = dynamicStates
        };

        PipelineViewportStateCreateInfo viewportState = new(){
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            ScissorCount = 1
        };

        PipelineRasterizationStateCreateInfo rasterization = new(){
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            DepthClampEnable = false,
            RasterizerDiscardEnable = false,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1,
            CullMode = CullModeFlags.BackBit,
            FrontFace = FrontFace.Clockwise,
            DepthBiasEnable = false,
        };

        PipelineMultisampleStateCreateInfo multisample = new(){
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1Bit,
        };

        PipelineColorBlendAttachmentState colorBlendAttachment = new(){
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit | ColorComponentFlags.ABit,
            BlendEnable = true,
            SrcColorBlendFactor = BlendFactor.SrcAlpha,
            DstColorBlendFactor = BlendFactor.OneMinusSrcAlpha,
            ColorBlendOp = BlendOp.Add,
            SrcAlphaBlendFactor = BlendFactor.One,
            DstAlphaBlendFactor = BlendFactor.Zero,
            AlphaBlendOp = BlendOp.Add
        };

        PipelineColorBlendStateCreateInfo colorBlend = new(){
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOpEnable = false,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment
        };

        PipelineDepthStencilStateCreateInfo depthStencil = new(){
            SType = StructureType.PipelineDepthStencilStateCreateInfo,
            DepthTestEnable = true,
            DepthWriteEnable = true,
            DepthCompareOp = CompareOp.Less,
            DepthBoundsTestEnable = false,
            StencilTestEnable = false
        };

        PushConstantRange pushConstants = new(){
            Offset = 0,
            Size = TransformPushConstant.SizeInBytes,
            StageFlags = ShaderStageFlags.VertexBit
        };

        DescriptorSetLayout* descriptorSetLayout = stackalloc[]{ shaderPipeline.DescriptorSetLayout };
        PipelineLayoutCreateInfo pipelineLayoutInfo = new(){
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = descriptorSetLayout,
            PushConstantRangeCount = 1,
            PPushConstantRanges = &pushConstants
        };

        if (_vk.CreatePipelineLayout(_logicalDevice, in pipelineLayoutInfo, null, out shaderPipeline.PipelineLayout) != Result.Success) {
            throw new Exception("Unable to create pipeline layout");
        }
        
        PushConstantRange pushConstantsShadow = new(){
            Offset = 0,
            Size = TransformPushConstantShadow.SizeInBytes,
            StageFlags = ShaderStageFlags.VertexBit
        };

        pipelineLayoutInfo.PPushConstantRanges = &pushConstantsShadow;
        
        if (_vk.CreatePipelineLayout(_logicalDevice, in pipelineLayoutInfo, null, out shaderPipeline.ShadowPipelineLayout) != Result.Success) {
            throw new Exception("Unable to create shadow pipeline layout");
        }

        GraphicsPipelineCreateInfo createInfo = new(){
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 2,
            PStages = shaderStages,
            PVertexInputState = &vertexInput,
            PInputAssemblyState = &inputAssembly,
            PViewportState = &viewportState,
            PRasterizationState = &rasterization,
            PMultisampleState = &multisample,
            PColorBlendState = &colorBlend,
            PDynamicState = &dynamicState,
            PDepthStencilState = &depthStencil,
            Layout = shaderPipeline.PipelineLayout,
            RenderPass = _renderPass,
            Subpass = 0
        };

        if (_vk.CreateGraphicsPipelines(_logicalDevice, default, 1, in createInfo, null, out shaderPipeline.GraphicsPipeline) != Result.Success) {
            throw new Exception("Unable to create pipeline");
        }

        colorBlend.AttachmentCount = 0;
        depthStencil.DepthCompareOp = CompareOp.LessOrEqual;
        rasterization.DepthClampEnable = true;
        createInfo.RenderPass = _shadowRenderPass;
        PipelineShaderStageCreateInfo* shadowShaderStages = stackalloc[]{
            shaderPipeline.ShaderProgram.VertStageShadow,
            shaderPipeline.ShaderProgram.FragStageShadow
        };
        createInfo.PStages = shadowShaderStages;
        createInfo.Layout = shaderPipeline.ShadowPipelineLayout;
        if (_vk.CreateGraphicsPipelines(_logicalDevice, default, 1, in createInfo, null, out shaderPipeline.ShadowPipeline) != Result.Success) {
            throw new Exception("Unable to create shadow pipeline");
        }

        shaderPipeline.ShaderProgram.DisposeModules(_logicalDevice);
    }

    private VkShaderProgram CreateShader(ShaderType shaderType){
        ShaderPrograms.LoadShaderBytes(shaderType.Name, out byte[] vert, out byte[] frag);
        ShaderModule vertModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, vert);
        ShaderModule fragModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, frag);
        ShaderPrograms.LoadShaderBytes(shaderType.Name + "_shadow", out byte[] vertShadow, out byte[] fragShadow);
        ShaderModule vertShadowModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, vertShadow);
        ShaderModule fragShadowModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, fragShadow);
        return new VkShaderProgram(shaderType, _vk, vertModule, fragModule, vertShadowModule, fragShadowModule);
    }

    private void CreateRenderPass(){
        AttachmentDescription colorAttachment = new(){
            Format = _format.Format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };

        AttachmentDescription depthAttachment = new(){
            Format = Format.D32Sfloat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
        };

        AttachmentReference colorAttachmentRef = new(){
            Attachment = 0,
            Layout = ImageLayout.AttachmentOptimal
        };

        AttachmentReference depthAttachmentRef = new(){
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal
        };

        SubpassDescription subpass = new(){
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = &depthAttachmentRef
        };

        SubpassDependency dependency = new(){
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.LateFragmentTestsBit,
            SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
        };

        AttachmentDescription* attachmentDescriptions = stackalloc[]{ colorAttachment, depthAttachment };
        RenderPassCreateInfo createInfo = new(){
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 2,
            PAttachments = attachmentDescriptions,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        if (_vk.CreateRenderPass(_logicalDevice, in createInfo, null, out _renderPass) != Result.Success) {
            throw new Exception("Unable to create render pass");
        }
    }

    private void CreateFramebuffers(){
        _framebuffers = new Framebuffer[_imageCount];
        for (var i = 0; i < _framebuffers.Length; i++) {
            ImageView[] attachments = [_swapChainImageViews[i], _depthImageView];

            fixed (ImageView* attachmentsPtr = attachments) {
                FramebufferCreateInfo createInfo = new(){
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = _renderPass,
                    AttachmentCount = (uint)attachments.Length,
                    PAttachments = attachmentsPtr,
                    Width = _extent.Width,
                    Height = _extent.Height,
                    Layers = 1
                };
                if (_vk.CreateFramebuffer(_logicalDevice, in createInfo, null, out _framebuffers[i]) != Result.Success) {
                    throw new Exception("Unable to create framebuffer " + i);
                }
            }
        }
    }

    private void CreateCommandPool(){
        CommandPoolCreateInfo createInfo = new(){
            SType = StructureType.CommandPoolCreateInfo,
            Flags = CommandPoolCreateFlags.ResetCommandBufferBit,
            QueueFamilyIndex = _queueFamilies.Graphics!.Value
        };
        if (_vk.CreateCommandPool(_logicalDevice, in createInfo, null, out _commandPool) != Result.Success) {
            throw new Exception("Unable to create graphics command pool");
        }
    }

    private void CreateDepthResources(){
        const Format depthFormat = Format.D32Sfloat;
        VkImages.CreateImage(_vk, _logicalDevice, _physicalDevice, _extent.Width, _extent.Height, depthFormat,
            ImageTiling.Optimal, ImageUsageFlags.DepthStencilAttachmentBit, MemoryPropertyFlags.DeviceLocalBit,
            out _depthImage, out _depthImageMemory);
        _depthImageView = VkImages.CreateImageView(_vk, _logicalDevice, _depthImage, depthFormat, ImageAspectFlags.DepthBit);
    }

    private void CreateShadowMappingResources(){
        const Format depthFormat = Format.D32SfloatS8Uint;
        AttachmentDescription attachmentDesc = new(){
            Format = depthFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilReadOnlyOptimal
        };

        AttachmentReference depthReference = new(0, ImageLayout.DepthStencilAttachmentOptimal);
        SubpassDescription subpass = new(){
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 0,
            PDepthStencilAttachment = &depthReference
        };

        SubpassDependency subpassDep1 = new(){
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.FragmentShaderBit,
            DstStageMask = PipelineStageFlags.EarlyFragmentTestsBit,
            SrcAccessMask = AccessFlags.ShaderReadBit,
            DstAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
            DependencyFlags = DependencyFlags.ByRegionBit
        };
        SubpassDependency subpassDep2 = new(){
            SrcSubpass = 0,
            DstSubpass = Vk.SubpassExternal,
            SrcStageMask = PipelineStageFlags.LateFragmentTestsBit,
            DstStageMask = PipelineStageFlags.FragmentShaderBit,
            SrcAccessMask = AccessFlags.DepthStencilAttachmentWriteBit,
            DstAccessMask = AccessFlags.ShaderReadBit,
            DependencyFlags = DependencyFlags.ByRegionBit
        };

        SubpassDependency* subpassDeps = stackalloc[]{ subpassDep1, subpassDep2 };

        RenderPassCreateInfo renderPassCreate = new(){
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &attachmentDesc,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 2,
            PDependencies = subpassDeps
        };

        if (_vk.CreateRenderPass(_logicalDevice, &renderPassCreate, null, out _shadowRenderPass) != Result.Success) {
            throw new Exception("Unable to create shadow mapping render pass");
        }
        VkImages.CreateImage(_vk, _logicalDevice, _physicalDevice, ShadowMapSize, ShadowMapSize, depthFormat, ImageTiling.Optimal, 
            ImageUsageFlags.DepthStencilAttachmentBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit, 
            ShadowMapCascadeCount, out _shadowImage, out _shadowImageMemory);
        
        ImageViewCreateInfo imageViewCreateInfo = new(){
            SType = StructureType.ImageViewCreateInfo,
            Image = _shadowImage,
            ViewType = ImageViewType.Type2DArray,
            Format = depthFormat,
            Components = new ComponentMapping(ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity),
            SubresourceRange = new ImageSubresourceRange{
                AspectMask = ImageAspectFlags.DepthBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = ShadowMapCascadeCount
            }
        };
        if (_vk.CreateImageView(_logicalDevice, in imageViewCreateInfo, null, out _shadowImageView) != Result.Success) {
            throw new Exception("Unable to create shadow image view");
        }

        for (uint i = 0; i < ShadowMapCascadeCount; i++) {
            VkCascade cascade = new VkCascade();
            ImageViewCreateInfo cascadeImageView = new(){
                SType = StructureType.ImageViewCreateInfo,
                Image = _shadowImage,
                ViewType = ImageViewType.Type2DArray,
                Format = depthFormat,
                Components = new ComponentMapping(ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity),
                SubresourceRange = new ImageSubresourceRange{
                    AspectMask = ImageAspectFlags.DepthBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = i,
                    LayerCount = 1
                }
            };
            if (_vk.CreateImageView(_logicalDevice, in cascadeImageView, null, out cascade.View) != Result.Success) {
                throw new Exception("Unable to create shadow image view");
            }

            ImageView imageView = cascade.View;

            FramebufferCreateInfo cascadeFramebuffer = new(){
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = _shadowRenderPass,
                AttachmentCount = 1,
                PAttachments = &imageView,
                Width = ShadowMapSize,
                Height = ShadowMapSize,
                Layers = 1
            };
            
            if (_vk.CreateFramebuffer(_logicalDevice, in cascadeFramebuffer, null, out cascade.Framebuffer) != Result.Success) {
                throw new Exception("Unable to create shadow framebuffer");
            }
            _cascades[i] = cascade;
        }

        SamplerCreateInfo sampler = new(){
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            MipmapMode = SamplerMipmapMode.Linear,
            AddressModeU = SamplerAddressMode.ClampToEdge,
            AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge,
            MipLodBias = 0f,
            MaxAnisotropy = 1f,
            MinLod = 0f,
            MaxLod = 1f,
            BorderColor = BorderColor.FloatOpaqueWhite
        };
        if (_vk.CreateSampler(_logicalDevice, in sampler, null, out _shadowSampler) != Result.Success) {
            throw new Exception("Unable to create shadow sampler");
        }
    }

    private void CreateTextureImage(){
        SKBitmap bitmap = SKBitmap.Decode("./Assets/Textures/Block/Tile.png");
        uint width = (uint)bitmap.Width;
        uint height = (uint)bitmap.Height;
        Span<byte> pixels = bitmap.GetPixelSpan();
        ulong imageSize = (ulong)bitmap.ByteCount;

        VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, imageSize,
            BufferUsageFlags.TransferSrcBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out Buffer stagingBuffer, out DeviceMemory stagingBufferMemory);

        void* data;
        _vk.MapMemory(_logicalDevice, stagingBufferMemory, 0, imageSize, 0, &data);
        pixels.CopyTo(new Span<byte>(data, (int)imageSize));
        _vk.UnmapMemory(_logicalDevice, stagingBufferMemory);

        bitmap.Dispose();

        VkImages.CreateImage(_vk, _logicalDevice, _physicalDevice, width, height, Format.B8G8R8A8Srgb,
            ImageTiling.Optimal, ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit,
            out _textureImage, out _textureImageMemory);

        VkImages.TransitionImageLayout(_vk, _queueFamilies, _graphicsQueue, _logicalDevice, _textureImage,
            Format.B8G8R8A8Srgb, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        VkImages.CopyBufferToImage(_vk, _queueFamilies, _graphicsQueue, _logicalDevice, stagingBuffer, _textureImage, width, height);
        VkImages.TransitionImageLayout(_vk, _queueFamilies, _graphicsQueue, _logicalDevice, _textureImage,
            Format.B8G8R8A8Srgb, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

        _vk.DestroyBuffer(_logicalDevice, stagingBuffer, null);
        _vk.FreeMemory(_logicalDevice, stagingBufferMemory, null);

        _textureImageView = VkImages.CreateImageView(_vk, _logicalDevice, _textureImage, Format.B8G8R8A8Srgb);
    }

    private void CreateTextureSampler(){
        SamplerCreateInfo samplerCreateInfo = new(){
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Nearest,
            MinFilter = Filter.Nearest,
            AddressModeU = SamplerAddressMode.ClampToEdge,
            AddressModeV = SamplerAddressMode.ClampToEdge,
            AddressModeW = SamplerAddressMode.ClampToEdge,
            AnisotropyEnable = true,
            MaxAnisotropy = _physicalDeviceLimits.MaxSamplerAnisotropy,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Linear,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = 0
        };

        if (_vk.CreateSampler(_logicalDevice, in samplerCreateInfo, null, out _textureSampler) != Result.Success) {
            throw new Exception("Unable to create graphics command pool");
        }
    }

    private void CreateVertexBuffer(VkShaderPipeline pipeline){
        ulong bufferSize = pipeline.ShaderProgram.ShType.BufferSize;
        VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, bufferSize,
            BufferUsageFlags.TransferSrcBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out pipeline.StagingVertexBuffer, out pipeline.StagingVertexBufferMemory);

        VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, bufferSize,
            BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.DeviceLocalBit,
            out pipeline.VertexBuffer, out pipeline.VertexBufferMemory);
    }

    private void CreateUniformBuffers(VkShaderPipeline pipeline){
        ulong bufferSize = UniformBufferObject.SizeInBytes;
        pipeline.UniformBuffers = new Buffer[_imageCount];
        pipeline.UniformBuffersMemory = new DeviceMemory[_imageCount];
        pipeline.UniformBufferMappedMemory = new void*[_imageCount];
        
        ulong shadowBufferSize = UniformBufferObjectArray.SizeInBytes;
        pipeline.ShadowUniformBuffers = new Buffer[_imageCount];
        pipeline.ShadowUniformBuffersMemory = new DeviceMemory[_imageCount];
        pipeline.ShadowUniformBufferMappedMemory = new void*[_imageCount];

        pipeline.ShadowMapDataUbo.Create(_vk, _logicalDevice, _physicalDevice, _imageCount, ShadowMapData.SizeInBytes);
        
        for (int i = 0; i < _imageCount; i++) {
            VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, bufferSize,
                BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out pipeline.UniformBuffers[i], out pipeline.UniformBuffersMemory[i]);
            fixed (void** mappedMemory = pipeline.UniformBufferMappedMemory) {
                _vk.MapMemory(_logicalDevice, pipeline.UniformBuffersMemory[i], 0, bufferSize, 0, &mappedMemory[i]);
            }
            
            VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, shadowBufferSize,
                BufferUsageFlags.UniformBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
                out pipeline.ShadowUniformBuffers[i], out pipeline.ShadowUniformBuffersMemory[i]);
            fixed (void** mappedMemory = pipeline.ShadowUniformBufferMappedMemory) {
                _vk.MapMemory(_logicalDevice, pipeline.ShadowUniformBuffersMemory[i], 0, shadowBufferSize, 0, &mappedMemory[i]);
            }
        }
    }

    private void CreateDescriptorPool(VkShaderPipeline pipeline){
        DescriptorPoolSize uboPoolSize = new(DescriptorType.UniformBuffer, _imageCount);
        DescriptorPoolSize samplerPoolSize = new(DescriptorType.CombinedImageSampler, _imageCount);
        DescriptorPoolSize shadowUboPoolSize = new(DescriptorType.UniformBuffer, _imageCount);
        DescriptorPoolSize shadowSamplerPoolSize = new(DescriptorType.CombinedImageSampler, _imageCount);
        DescriptorPoolSize shadowMapDataUboPoolSize = new(DescriptorType.UniformBuffer, _imageCount);

        DescriptorPoolSize* poolSizes = stackalloc[]{ uboPoolSize, samplerPoolSize, shadowUboPoolSize, shadowSamplerPoolSize, shadowMapDataUboPoolSize };

        DescriptorPoolCreateInfo createInfo = new(){
            SType = StructureType.DescriptorPoolCreateInfo,
            PoolSizeCount = 5,
            PPoolSizes = poolSizes,
            MaxSets = _imageCount
        };
        if (_vk.CreateDescriptorPool(_logicalDevice, in createInfo, null, out pipeline.DescriptorPool) != Result.Success) {
            throw new Exception("Unable to create descriptor pool");
        }
    }

    private void CreateDescriptorSets(VkShaderPipeline pipeline){
        DescriptorSetLayout* layouts = stackalloc DescriptorSetLayout[(int)_imageCount];
        for (int i = 0; i < _imageCount; i++) {
            layouts[i] = pipeline.DescriptorSetLayout;
        }

        DescriptorSetAllocateInfo allocateInfo = new(){
            SType = StructureType.DescriptorSetAllocateInfo,
            DescriptorPool = pipeline.DescriptorPool,
            DescriptorSetCount = _imageCount,
            PSetLayouts = layouts
        };
        pipeline.DescriptorSets = new DescriptorSet[_imageCount];
        if (_vk.AllocateDescriptorSets(_logicalDevice, &allocateInfo, pipeline.DescriptorSets) != Result.Success) {
            throw new Exception("Unable to create descriptor sets");
        }

        for (int i = 0; i < _imageCount; i++) {
            DescriptorBufferInfo bufferInfo = new(){
                Buffer = pipeline.UniformBuffers[i],
                Offset = 0,
                Range = Vk.WholeSize
            };

            DescriptorImageInfo imageInfo = new(){
                ImageLayout = ImageLayout.ReadOnlyOptimal,
                ImageView = _textureImageView,
                Sampler = _textureSampler
            };
            
            DescriptorBufferInfo shadowBufferInfo = new(){
                Buffer = pipeline.ShadowUniformBuffers[i],
                Offset = 0,
                Range = Vk.WholeSize
            };
            
            DescriptorImageInfo shadowImageInfo = new(){
                ImageLayout = ImageLayout.ReadOnlyOptimal,
                ImageView = _shadowImageView,
                Sampler = _shadowSampler
            };

            DescriptorBufferInfo shadowMapDataInfo = pipeline.ShadowMapDataUbo.CreateBufferInfo(i);

            WriteDescriptorSet bufferWriteSet = new(){
                SType = StructureType.WriteDescriptorSet,
                DstSet = pipeline.DescriptorSets[i],
                DstBinding = 0,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &bufferInfo
            };

            WriteDescriptorSet imageWriteSet = new(){
                SType = StructureType.WriteDescriptorSet,
                DstSet = pipeline.DescriptorSets[i],
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };
            
            WriteDescriptorSet shadowBufferWriteSet = new(){
                SType = StructureType.WriteDescriptorSet,
                DstSet = pipeline.DescriptorSets[i],
                DstBinding = 2,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &shadowBufferInfo
            };
            
            WriteDescriptorSet shadowImageWriteSet = new(){
                SType = StructureType.WriteDescriptorSet,
                DstSet = pipeline.DescriptorSets[i],
                DstBinding = 3,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &shadowImageInfo
            };
            
            WriteDescriptorSet shadowMapDataBufferWriteSet = new(){
                SType = StructureType.WriteDescriptorSet,
                DstSet = pipeline.DescriptorSets[i],
                DstBinding = 4,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                PBufferInfo = &shadowMapDataInfo
            };

            WriteDescriptorSet* writes = stackalloc[]{ bufferWriteSet, imageWriteSet, shadowBufferWriteSet, shadowImageWriteSet, shadowMapDataBufferWriteSet };

            _vk.UpdateDescriptorSets(_logicalDevice, 5, writes, 0, null);
        }
    }

    private void CreateCommandBuffer(){
        _commandBuffers = new CommandBuffer[_imageCount];
        CommandBufferAllocateInfo allocateInfo = new(){
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = _commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)_commandBuffers.Length
        };
        fixed (CommandBuffer* commandBuffer = _commandBuffers) {
            if (_vk.AllocateCommandBuffers(_logicalDevice, in allocateInfo, commandBuffer) != Result.Success) {
                throw new Exception("Unable to create graphics command buffer");
            }
        }
    }

    private void RecordShadowMap(CommandBuffer buffer){
        ClearValue depthClearValue = new ClearValue(null, new ClearDepthStencilValue(1f, 0));
        Extent2D extent = new Extent2D(ShadowMapSize, ShadowMapSize);
        RenderPassBeginInfo renderPassBegin = new(){
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _shadowRenderPass,
            RenderArea = new Rect2D{
                Offset = new Offset2D(0, 0),
                Extent = extent
            },
            ClearValueCount = 1,
            PClearValues = &depthClearValue
        };
        
        for (uint i = 0; i < ShadowMapCascadeCount; i++) {
            renderPassBegin.Framebuffer = _cascades[i]!.Framebuffer;
            _vk.CmdBeginRenderPass(buffer, &renderPassBegin, SubpassContents.Inline);
            RenderScene(buffer, extent, true, i);
            _vk.CmdEndRenderPass(buffer);
        }
    }

    private void RecordCommandBuffer(CommandBuffer buffer, uint imageIndex){
        CommandBufferBeginInfo beginInfo = new(){
            SType = StructureType.CommandBufferBeginInfo
        };
        if (_vk.BeginCommandBuffer(buffer, in beginInfo) != Result.Success) {
            throw new Exception("Unable to begin recording command buffer");
        }

        RecordShadowMap(buffer);

        ClearValue colorClearValue = new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f));
        ClearValue depthClearValue = new ClearValue(null, new ClearDepthStencilValue(1f, 0));
        ClearValue* clearValues = stackalloc[]{ colorClearValue, depthClearValue };
        RenderPassBeginInfo renderPassBegin = new(){
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffers![imageIndex],
            RenderArea = new Rect2D{
                Offset = new Offset2D(0, 0),
                Extent = _extent
            },
            ClearValueCount = 2,
            PClearValues = clearValues
        };

        _vk.CmdBeginRenderPass(buffer, &renderPassBegin, SubpassContents.Inline);
        RenderScene(buffer, _extent, false);
        _vk.CmdEndRenderPass(buffer);
        if (_vk.EndCommandBuffer(buffer) != Result.Success) {
            throw new Exception("Unable to finish recording command buffer");
        }
    }

    private void RenderScene(CommandBuffer buffer, Extent2D extent, bool shadowMapping, uint cascadeIndex = 0){
        foreach ((ShaderType shaderType, VkShaderPipeline pipeline) in _pipelines) {
            Dictionary<Mesh, CompiledMeshData?> meshes = pipeline.Meshes;
            if (meshes.Count == 0)
                continue;
            UpdateUniformBuffer((uint)_frameIndex, pipeline);

            PipelineLayout pipelineLayout = shadowMapping ? pipeline.ShadowPipelineLayout : pipeline.PipelineLayout;
            Pipeline vkPipeline = shadowMapping ? pipeline.ShadowPipeline : pipeline.GraphicsPipeline;
            _vk.CmdBindDescriptorSets(buffer, PipelineBindPoint.Graphics, pipelineLayout, 0, 1, ref pipeline.DescriptorSets[_frameIndex], 0, null);
            _vk.CmdBindPipeline(buffer, PipelineBindPoint.Graphics, vkPipeline);

            Viewport viewport = new(){
                X = 0f,
                Y = 0f,
                Width = extent.Width,
                Height = extent.Height,
                MinDepth = 0,
                MaxDepth = 1
            };
            _vk.CmdSetViewport(buffer, 0, 1, &viewport);

            Rect2D scissor = new(){
                Offset = new Offset2D(0, 0),
                Extent = extent
            };
            _vk.CmdSetScissor(buffer, 0, 1, &scissor);

            foreach ((Mesh mesh, CompiledMeshData? data) in meshes) {
                if (data != null) {
                    if (!shadowMapping && _camera.Frustum != null && data.Aabb != null && !_camera.Frustum.IsBoxInside(data.Aabb)) {
                        continue;
                    }

                    fixed (Buffer* bufferPtr = new[]{ pipeline.VertexBuffer }) {
                        ulong offset = mesh.Memory.Offset;
                        _vk.CmdBindVertexBuffers(buffer, 0, 1, bufferPtr, ref offset);
                    }

                    if (shadowMapping) {
                        TransformPushConstantShadow transformPushConstant = new TransformPushConstantShadow{
                            Transform = data.PushConstant,
                            CascadeIndex = cascadeIndex
                        };
                        _vk.CmdPushConstants(buffer, pipeline.ShadowPipelineLayout, ShaderStageFlags.VertexBit, 0, TransformPushConstantShadow.SizeInBytes, &transformPushConstant);
                    }
                    else {
                        TransformPushConstant transformPushConstant = data.PushConstant;
                        _vk.CmdPushConstants(buffer, pipeline.PipelineLayout, ShaderStageFlags.VertexBit, 0, TransformPushConstant.SizeInBytes, &transformPushConstant);
                    }
                    _vk.CmdDraw(buffer, data.VertexCount, 1, 0, 0);
                }
            }
        }
    }

    private void UpdateUniformBuffer(uint frame, VkShaderPipeline pipeline){
        _camera.AspectRatio = (float)_extent.Width / _extent.Height;
        UniformBufferObject ubo = new UniformBufferObject{
            Model = Matrix4X4<float>.Identity,
            View = Matrix4X4.CreateLookAt(_camera.Position, _camera.Target, Vector3D<float>.UnitY),
            Projection = Matrix4X4.CreatePerspectiveFieldOfView(Scalar.DegreesToRadians(_camera.FoV), _camera.AspectRatio, _camera.Near, _camera.Far),
        };
        ubo.Projection.M22 *= -1;

        float[] data = ubo.ToFloatArray();
        data.AsSpan().CopyTo(new Span<float>(pipeline.UniformBufferMappedMemory[frame], data.Length));

        UpdateCascadeUniformBuffers(frame, pipeline, in ubo);
    }

    private void UpdateCascadeUniformBuffers(uint frame, VkShaderPipeline pipeline, in UniformBufferObject cameraUbo){
        float[] cascadeSplits = new float[ShadowMapCascadeCount];
        float[] cascadeSplitDepths = new float[ShadowMapCascadeCount];
        float near = _camera.Near;
        float far = _camera.Far;
        float clipRange = far - near;
        float minZ = near;
        float maxZ = near + clipRange;
        float range = maxZ - minZ;
        float ratio = maxZ / minZ;

        UniformBufferObjectArray array = new UniformBufferObjectArray{
            CascadeUbos = new UniformBufferObject[ShadowMapCascadeCount]
        };

        for (int i = 0; i < ShadowMapCascadeCount; i++) {
            float p = (i + 1) / (float)ShadowMapCascadeCount;
            float log = minZ * MathF.Pow(ratio, p);
            float uniform = minZ + range * p;
            float d = 0.95f * (log - uniform) + uniform;
            cascadeSplitDepths[i] = -d;
            cascadeSplits[i] = (d-near) / clipRange;
        }

        float lastSplitDist = 0f;
        float degs = (float)_window!.Time * 5;
        Vector3D<float> lightPos = new Vector3D<float>(float.Cos(float.DegreesToRadians(degs)) * 20f, 10f, float.Sin(float.DegreesToRadians(degs)) * 20f);
        Vector3D<float> lightDir = Vector3D.Normalize(-lightPos);
        for (int i = 0; i < ShadowMapCascadeCount; i++) {
            float splitDist = cascadeSplits[i];
            
            Vector3D<float>[] corners = [
                new Vector3D<float>(-1.0f,  1.0f, 0.0f),
                new Vector3D<float>( 1.0f,  1.0f, 0.0f),
                new Vector3D<float>( 1.0f, -1.0f, 0.0f),
                new Vector3D<float>(-1.0f, -1.0f, 0.0f),
                new Vector3D<float>(-1.0f,  1.0f,  1.0f),
                new Vector3D<float>( 1.0f,  1.0f,  1.0f),
                new Vector3D<float>( 1.0f, -1.0f,  1.0f),
                new Vector3D<float>(-1.0f, -1.0f,  1.0f),
            ];
            
            Matrix4X4.Invert(cameraUbo.View * cameraUbo.Projection, out Matrix4X4<float> invCam);
            for (int j = 0; j < 8; j++) {
                Vector4D<float> invCorner = Vector4D.Transform(new Vector4D<float>(corners[j], 1f), invCam);
                Vector4D<float> invCornerNormalized = invCorner / invCorner.W;
                corners[j] = new Vector3D<float>(invCornerNormalized.X, invCornerNormalized.Y, invCornerNormalized.Z);
            }
            
            for (int j = 0; j < 4; j++) {
                Vector3D<float> dist = corners[j + 4] - corners[j];
                corners[j + 4] = corners[j] + (dist * splitDist);
                corners[j] += (dist * lastSplitDist);
            }
            
            Vector3D<float> frustumCenter = new Vector3D<float>(0);
            for (int j = 0; j < 8; j++) {
                frustumCenter += corners[j];
            }
            frustumCenter /= 8;
            
            float radius = 0f;
            for (int j = 0; j < 8; j++) {
                float distance = (corners[j] - frustumCenter).Length;
                radius = MathF.Max(radius, distance);
            }
            
            radius = MathF.Ceiling(radius * 16) / 16f;
            
            Vector3D<float> maxExtents = new Vector3D<float>(radius);
            Vector3D<float> minExtents = -maxExtents;
            
            Matrix4X4<float> lightViewMatrix = Matrix4X4.CreateLookAt(frustumCenter - lightDir * -minExtents.Z, frustumCenter, Vector3D<float>.UnitY);
            Matrix4X4<float> lightOrthoMatrix = Matrix4X4.CreateOrthographicOffCenter(minExtents.X, maxExtents.X, minExtents.Y, maxExtents.Y, 0, maxExtents.Z - minExtents.Z);
            lightOrthoMatrix.M22 *= -1;

            UniformBufferObject cascadeUbo = new UniformBufferObject{
                Model = Matrix4X4<float>.Identity,
                View = lightViewMatrix,
                Projection = lightOrthoMatrix
            };
            array.CascadeUbos[i] = cascadeUbo;
            
            lastSplitDist = cascadeSplits[i];
        }
        
        float[] data = array.ToFloatArray();
        data.AsSpan().CopyTo(new Span<float>(pipeline.ShadowUniformBufferMappedMemory[frame], data.Length));

        ShadowMapData shadowMapData = new ShadowMapData{
            CascadeSplits = new Vector4D<float>(cascadeSplitDepths[0], cascadeSplitDepths[1], cascadeSplitDepths[2], cascadeSplitDepths[3]),
            LightDir = lightDir
        };
        float[] shadowMapDataArr = shadowMapData.ToFloatArray();
        shadowMapDataArr.AsSpan().CopyTo(new Span<float>(pipeline.ShadowMapDataUbo.MappedMemories[frame], shadowMapDataArr.Length));
    }

    private void CreateSyncObjects(){
        SemaphoreCreateInfo semaphoreCreateInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);
        FenceCreateInfo fenceCreateInfo = new(){
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };
        _imageAvailableSemaphore = new Semaphore[_imageCount];
        _renderFinishedSemaphore = new Semaphore[_imageCount];
        _fence = new Fence[_imageCount];
        for (var i = 0; i < _imageCount; i++) {
            if (_vk.CreateSemaphore(_logicalDevice, &semaphoreCreateInfo, null, out _imageAvailableSemaphore[i]) != Result.Success ||
                _vk.CreateSemaphore(_logicalDevice, &semaphoreCreateInfo, null, out _renderFinishedSemaphore[i]) != Result.Success ||
                _vk.CreateFence(_logicalDevice, &fenceCreateInfo, null, out _fence[i]) != Result.Success) {
                throw new Exception("Unable to create sync objects");
            }
        }
    }

    public void PreRender(double delta){
    }

    public void OnRender(double delta){
        if (_minimized) {
            return;
        }

        _vk.WaitForFences(_logicalDevice, 1, ref _fence[_frameIndex], true, ulong.MaxValue);
        uint imageIndex = 0;
        Result acquireResult = _swapChainExtension!.AcquireNextImage(_logicalDevice, _swapChain, UInt64.MaxValue, _imageAvailableSemaphore[_frameIndex], default, ref imageIndex);

        if (acquireResult == Result.ErrorOutOfDateKhr) {
            RecreateSwapChain();
            return;
        }

        _vk.ResetFences(_logicalDevice, 1, ref _fence[_frameIndex]);

        _vk.ResetCommandBuffer(_commandBuffers[_frameIndex], 0);
        RecordCommandBuffer(_commandBuffers[_frameIndex], imageIndex);

        Semaphore* waitSemaphores = stackalloc[]{ _imageAvailableSemaphore[_frameIndex] };
        PipelineStageFlags* waitStages = stackalloc[]{ PipelineStageFlags.ColorAttachmentOutputBit };
        CommandBuffer* commandBuffers = stackalloc[]{ _commandBuffers[_frameIndex] };
        Semaphore* signalSemaphores = stackalloc[]{ _renderFinishedSemaphore[_frameIndex] };
        SubmitInfo submitInfo = new(){
            SType = StructureType.SubmitInfo,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = waitSemaphores,
            PWaitDstStageMask = waitStages,
            CommandBufferCount = 1,
            PCommandBuffers = commandBuffers,
            SignalSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores
        };
        if (_vk.QueueSubmit(_graphicsQueue, 1, &submitInfo, _fence[_frameIndex]) != Result.Success) {
            throw new Exception("Unable to finish recording command buffer");
        }

        SwapchainKHR* swapChains = stackalloc[]{ _swapChain };
        PresentInfoKHR presentInfo = new(){
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = signalSemaphores,
            SwapchainCount = 1,
            PSwapchains = swapChains,
            PImageIndices = &imageIndex
        };
        Result presentResult = _swapChainExtension.QueuePresent(_presentQueue, &presentInfo);
        if (presentResult == Result.ErrorOutOfDateKhr || presentResult == Result.SuboptimalKhr || _hasResized) {
            _hasResized = false;
            RecreateSwapChain();
            return;
        }

        _frameIndex = (_frameIndex + 1) % (int)_imageCount;
    }

    public Mesh CreateMesh(ShaderType shaderType, ulong size){
        VkShaderPipeline pipeline = _pipelines[shaderType];
        if (pipeline.Allocator.TryAllocate(size, out BufferAllocator.MemoryBlock? block)) {
            Mesh mesh = new Mesh(shaderType, block);
            pipeline.Meshes.Add(mesh, null);
            pipeline.MemoryBlockRefCount[block] = 1;
            return mesh;
        }

        throw new Exception($"Unable to allocate {size} bytes for shader {shaderType.Name}");
    }

    public void WriteMesh(Mesh mesh, VertexBuilder builder, AABB? aabb, ref TransformPushConstant pushConstant){
        VkShaderPipeline pipeline = _pipelines[mesh.SType];
        float[] vertices = builder.Pack();
        void* data;
        ulong dataSize = (ulong)vertices.Length * 4;
        _vk.MapMemory(_logicalDevice, pipeline.StagingVertexBufferMemory, mesh.Memory.Offset, dataSize, 0, &data);
        vertices.AsSpan().CopyTo(new Span<float>(data, vertices.Length));
        _vk.UnmapMemory(_logicalDevice, pipeline.StagingVertexBufferMemory);
        VkBuffers.CopyBuffer(_vk, _graphicsQueue, _queueFamilies, _logicalDevice, pipeline.StagingVertexBuffer, pipeline.VertexBuffer, mesh.Memory.Offset, dataSize);
        pipeline.Meshes[mesh] = new CompiledMeshData(builder.VerticesCount, aabb, pushConstant);
    }

    public void DestroyMesh(Mesh mesh){
        VkShaderPipeline pipeline = _pipelines[mesh.SType];
        pipeline.MemoryBlockRefCount[mesh.Memory]--;
        if (pipeline.MemoryBlockRefCount[mesh.Memory] == 0) {
            pipeline.Allocator.FreeBlock(mesh.Memory);
            pipeline.Meshes.Remove(mesh);
        }
    }

    private void CleanupSwapChain(){
        for (var i = 0; i < _imageCount; i++) {
            _vk.DestroySemaphore(_logicalDevice, _imageAvailableSemaphore[i], null);
            _vk.DestroySemaphore(_logicalDevice, _renderFinishedSemaphore[i], null);
            _vk.DestroyFence(_logicalDevice, _fence[i], null);
        }

        foreach (Framebuffer fb in _framebuffers ?? []) {
            _vk.DestroyFramebuffer(_logicalDevice, fb, null);
        }

        _framebuffers = null;
        foreach (ImageView imageView in _swapChainImageViews) {
            _vk.DestroyImageView(_logicalDevice, imageView, null);
        }

        _swapChainImageViews = [];
        _swapChainExtension?.DestroySwapchain(_logicalDevice, _swapChain, null);

        _vk.DestroyImageView(_logicalDevice, _depthImageView, null);
        _vk.DestroyImage(_logicalDevice, _depthImage, null);
        _vk.FreeMemory(_logicalDevice, _depthImageMemory, null);
    }

    public void Cleanup(){
        _vk.DeviceWaitIdle(_logicalDevice);
        CleanupSwapChain();
        
        foreach (VkCascade? cascade in _cascades) {
            cascade?.Destroy(_vk, _logicalDevice);
        }

        _vk.DestroySampler(_logicalDevice, _textureSampler, null);
        _vk.DestroyImageView(_logicalDevice, _textureImageView, null);
        _vk.DestroyImage(_logicalDevice, _textureImage, null);
        _vk.FreeMemory(_logicalDevice, _textureImageMemory, null);

        _vk.DestroyCommandPool(_logicalDevice, _commandPool, null);
        foreach (VkShaderPipeline pipeline in _pipelines.Values) {
            pipeline.Destroy(_vk, _logicalDevice);
        }

        _vk.DestroyRenderPass(_logicalDevice, _renderPass, null);

        _vk.DestroyDevice(_logicalDevice, null);
        _surfaceExtension?.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
        _window?.Dispose();
    }
}