using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BrainWithNoSkull.Render.Data;
using BrainWithNoSkull.Render.RenderEngine.Vulkan;
using BrainWithNoSkull.Render.Shader;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace BrainWithNoSkull.Render.RenderEngine;

public unsafe class VkRenderEngine : IRenderEngine {
    public static readonly VkRenderEngine Instance = new VkRenderEngine();

    private static bool _enableValidationLayer = true;

    private readonly Vk _vk = Vk.GetApi();
    private IWindow? _window;
    private Instance _instance;

    private PhysicalDevice _physicalDevice;
    private Device _logicalDevice;

    private Queue _graphicsQueue;
    private Queue _presentQueue;

    private SurfaceKHR _surface;
    private KhrSurface? _surfaceExtension;

    private SwapchainKHR _swapChain;
    private KhrSwapchain? _swapChainExtension;
    private Image[]? _swapChainImages;
    private ImageView[]? _swapChainImageViews;
    private SurfaceFormatKHR _format;
    private Extent2D _extent;

    private RenderPass _renderPass;
    private PipelineLayout _pipelineLayout;
    private Pipeline _pipeline;

    private Framebuffer[]? _framebuffers;

    private CommandPool _commandPool;
    private CommandBuffer[] _commandBuffers = [];

    private Buffer _buffer;
    private DeviceMemory _vertexBufferMemory;

    private Semaphore[] _imageAvailableSemaphore = [];
    private Semaphore[] _renderFinishedSemaphore = [];
    private Fence[] _fence = [];
    private int _frameIndex;
    private bool _hasResized;
    private bool _minimized;

    private static readonly string[] RequiredDeviceExtensions = [KhrSwapchain.ExtensionName];
    private static readonly string[] ValidationLayers = ["VK_LAYER_KHRONOS_validation"];

    public void Init(){
        WindowOptions windowOptions = WindowOptions.DefaultVulkan with{
            Size = new Vector2D<int>(800, 600),
            Title = "Brain with no Skull"
        };
        _window = Window.Create(windowOptions);
        _window.FramebufferResize += size => {
            _hasResized = true;
            _minimized = size is{ X: 0, Y: 0 };
        };
        _window.Initialize();
        if (_window.VkSurface is null) {
            throw new Exception("Vulkan not supported");
        }

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
        CreateGraphicsPipeline();
        CreateFramebuffers();
        CreateCommandPool();
        CreateVertexBuffer();
        CreateCommandBuffer();
        CreateSyncObjects();
    }

    private void RecreateSwapChain(){
        _vk.DeviceWaitIdle(_logicalDevice);

        CleanupSwapChain();
        if (!_minimized) {
            CreateSwapChain();
            CreateImageViews();
            CreateFramebuffers();
            CreateSyncObjects();
        }
    }

    private void CreateVkInstance(){
        byte* appName = (byte*)Marshal.StringToHGlobalAnsi("Brain with no Skull");
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
        QueueFamilies indices = FindQueueFamilies(_physicalDevice);

        DeviceQueueCreateInfo* createInfoPtr = CreateQueueFamilyStructs(indices, out uint numQueueFamilies, out GlobalMemory queueMemory);
        PhysicalDeviceFeatures deviceFeatures = new();

        PhysicalDeviceVulkan13Features features13 = new(){
            SType = StructureType.PhysicalDeviceVulkan13Features,
            Synchronization2 = true
        };
        DeviceCreateInfo createInfo = new(){
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = numQueueFamilies,
            PQueueCreateInfos = createInfoPtr,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)RequiredDeviceExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(RequiredDeviceExtensions),
            EnabledLayerCount = 0,
            PNext = &features13
        };

        if (_vk.CreateDevice(_physicalDevice, in createInfo, null, out _logicalDevice) != Result.Success) {
            throw new Exception("Unable to create logical device!");
        }

        _vk.GetDeviceQueue(_logicalDevice, indices.Graphics!.Value, 0, out _graphicsQueue);
        _vk.GetDeviceQueue(_logicalDevice, indices.Present!.Value, 0, out _presentQueue);
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

        QueueFamilies indices = FindQueueFamilies(_physicalDevice);
        uint* queueFamilyIndices = stackalloc[]{ indices.Graphics!.Value, indices.Present!.Value };

        if (indices.Graphics != indices.Present) {
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

        uint imageCount = 0;
        _swapChainExtension.GetSwapchainImages(_logicalDevice, _swapChain, ref imageCount, null);
        _swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = _swapChainImages) {
            _swapChainExtension.GetSwapchainImages(_logicalDevice, _swapChain, ref imageCount, swapChainImagesPtr);
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
        _swapChainImageViews = new ImageView[_swapChainImages!.Length];

        for (int i = 0; i < _swapChainImageViews.Length; i++) {
            ImageViewCreateInfo createInfo = new(){
                SType = StructureType.ImageViewCreateInfo,
                Image = _swapChainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = _format.Format,
                Components = new ComponentMapping(ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity, ComponentSwizzle.Identity),
                SubresourceRange = new ImageSubresourceRange{
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };
            if (_vk.CreateImageView(_logicalDevice, in createInfo, null, out _swapChainImageViews[i]) != Result.Success) {
                throw new Exception("Unable to create image view " + i);
            }
        }
    }

    private void CreateGraphicsPipeline(){
        VkShaderProgram shader = CreateShader(ShaderDetails.Shader);

        PipelineShaderStageCreateInfo* shaderStages = stackalloc[]{
            shader.VertStage,
            shader.FragStage
        };

        VkVertexConsumer consumer = new VkVertexConsumer();
        VertexInputBindingDescription bindingDescription = consumer.GetBindingDescription(shader.Details);
        VertexInputAttributeDescription[] attributeDescriptions = consumer.GetAttributeDescriptions(shader.Details);

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

        PipelineLayoutCreateInfo pipelineLayout = new(){
            SType = StructureType.PipelineLayoutCreateInfo
        };

        if (_vk.CreatePipelineLayout(_logicalDevice, in pipelineLayout, null, out _pipelineLayout) != Result.Success) {
            throw new Exception("Unable to create pipeline layout");
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
            Layout = _pipelineLayout,
            RenderPass = _renderPass,
            Subpass = 0
        };

        if (_vk!.CreateGraphicsPipelines(_logicalDevice, default, 1, in createInfo, null, out _pipeline) != Result.Success) {
            throw new Exception("Unable to create pipeline");
        }

        shader.DisposeModules(_logicalDevice);
    }

    private VkShaderProgram CreateShader(ShaderDetails details){
        ShaderPrograms.LoadShaderBytes(details.Name, out byte[] vert, out byte[] frag);
        ShaderModule vertModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, vert);
        ShaderModule fragModule = VkShaderProgram.CreateModule(_vk, _logicalDevice, frag);
        return new VkShaderProgram(details, _vk, vertModule, fragModule);
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

        AttachmentReference colorAttachmentRef = new(){
            Attachment = 0,
            Layout = ImageLayout.AttachmentOptimal
        };

        SubpassDescription subpass = new(){
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef
        };

        SubpassDependency dependency = new(){
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        RenderPassCreateInfo createInfo = new(){
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
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
        _framebuffers = new Framebuffer[_swapChainImageViews!.Length];
        for (var i = 0; i < _framebuffers.Length; i++) {
            ImageView[] attachments = [_swapChainImageViews[i]];

            fixed (ImageView* attachmentsPtr = attachments) {
                FramebufferCreateInfo createInfo = new(){
                    SType = StructureType.FramebufferCreateInfo,
                    RenderPass = _renderPass,
                    AttachmentCount = 1,
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
            QueueFamilyIndex = FindQueueFamilies(_physicalDevice).Graphics!.Value
        };
        if (_vk.CreateCommandPool(_logicalDevice, in createInfo, null, out _commandPool) != Result.Success) {
            throw new Exception("Unable to create graphics command pool");
        }
    }

    private void CreateVertexBuffer(){
        VertexBuilder builder = new VertexBuilder(this, ShaderDetails.Shader);
        builder.Pos(0, -0.5f, 0).Color(1, 0, 0).Build();
        builder.Pos(0.5f, 0.5f, 0).Color(0, 1, 0).Build();
        builder.Pos(-0.5f, 0.5f, 0).Color(0, 0, 1).Build();
        
        builder.Pos(-0.9f, 0, 0).Color(1, 0, 0).Build();
        builder.Pos(-0.8f, 0.1f, 0).Color(0, 1, 0).Build();
        builder.Pos(-1f, 0.1f, 0).Color(0, 0, 1).Build();
        float[] vertices = builder.Pack();
        ulong bufferSize = ShaderDetails.Shader.BufferSize * 4 * (ulong)vertices.Length;
        VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, bufferSize,
            BufferUsageFlags.TransferSrcBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.HostVisibleBit | MemoryPropertyFlags.HostCoherentBit,
            out Buffer stagingBuffer, out DeviceMemory stagingBufferMemory);

        void* data;
        _vk.MapMemory(_logicalDevice, stagingBufferMemory, 0, bufferSize, 0, &data);
        vertices.AsSpan().CopyTo(new Span<float>(data, vertices.Length));
        _vk.UnmapMemory(_logicalDevice, stagingBufferMemory);

        VkBuffers.CreateBuffer(_vk, _logicalDevice, _physicalDevice, bufferSize,
            BufferUsageFlags.TransferDstBit | BufferUsageFlags.VertexBufferBit, MemoryPropertyFlags.DeviceLocalBit,
            out _buffer, out _vertexBufferMemory);
        
        VkBuffers.CopyBuffer(_vk, _graphicsQueue, FindQueueFamilies(_physicalDevice), _logicalDevice, stagingBuffer, _buffer, bufferSize);
        
        _vk.DestroyBuffer(_logicalDevice, stagingBuffer, null);
        _vk.FreeMemory(_logicalDevice, stagingBufferMemory, null);
    }

    private void CreateCommandBuffer(){
        _commandBuffers = new CommandBuffer[_framebuffers!.Length];
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

    private void RecordCommandBuffer(CommandBuffer buffer, uint imageIndex){
        CommandBufferBeginInfo beginInfo = new(){
            SType = StructureType.CommandBufferBeginInfo
        };
        if (_vk.BeginCommandBuffer(buffer, in beginInfo) != Result.Success) {
            throw new Exception("Unable to begin recording command buffer");
        }

        ClearValue clearValue = new ClearValue(new ClearColorValue(0f, 0f, 0f, 1f));
        RenderPassBeginInfo renderPassBegin = new(){
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = _renderPass,
            Framebuffer = _framebuffers![imageIndex],
            RenderArea = new Rect2D{
                Offset = new Offset2D(0, 0),
                Extent = _extent
            },
            ClearValueCount = 1,
            PClearValues = &clearValue
        };

        _vk.CmdBeginRenderPass(buffer, &renderPassBegin, SubpassContents.Inline);
        _vk.CmdBindPipeline(buffer, PipelineBindPoint.Graphics, _pipeline);

        fixed (Buffer* bufferPtr = new[]{ _buffer }) {
            ulong offset = 0;
            _vk.CmdBindVertexBuffers(buffer, 0, 1, bufferPtr, ref offset);
        }

        Viewport viewport = new(){
            X = 0f,
            Y = 0f,
            Width = _extent.Width,
            Height = _extent.Height,
            MinDepth = 0,
            MaxDepth = 1
        };
        _vk.CmdSetViewport(buffer, 0, 1, &viewport);

        Rect2D scissor = new(){
            Offset = new Offset2D(0, 0),
            Extent = _extent
        };
        _vk.CmdSetScissor(buffer, 0, 1, &scissor);

        _vk.CmdDraw(buffer, (uint)3, 1, 0, 0);

        _vk.CmdEndRenderPass(buffer);
        if (_vk.EndCommandBuffer(buffer) != Result.Success) {
            throw new Exception("Unable to finish recording command buffer");
        }
    }

    private void CreateSyncObjects(){
        SemaphoreCreateInfo semaphoreCreateInfo = new SemaphoreCreateInfo(StructureType.SemaphoreCreateInfo);
        FenceCreateInfo fenceCreateInfo = new(){
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };
        _imageAvailableSemaphore = new Semaphore[_swapChainImages!.Length];
        _renderFinishedSemaphore = new Semaphore[_swapChainImages!.Length];
        _fence = new Fence[_swapChainImages!.Length];
        for (var i = 0; i < _swapChainImages.Length; i++) {
            if (_vk.CreateSemaphore(_logicalDevice, &semaphoreCreateInfo, null, out _imageAvailableSemaphore[i]) != Result.Success ||
                _vk.CreateSemaphore(_logicalDevice, &semaphoreCreateInfo, null, out _renderFinishedSemaphore[i]) != Result.Success ||
                _vk.CreateFence(_logicalDevice, &fenceCreateInfo, null, out _fence[i]) != Result.Success) {
            }
        }
    }

    public void StartLoop(){
        _window!.Render += OnRender;
        _window!.Run();
        _vk.DeviceWaitIdle(_logicalDevice);
    }

    private void OnRender(double delta){
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

        _frameIndex = (_frameIndex + 1) % _fence.Length;
    }

    private void CleanupSwapChain(){
        for (var i = 0; i < _swapChainImageViews?.Length; i++) {
            _vk.DestroySemaphore(_logicalDevice, _imageAvailableSemaphore[i], null);
            _vk.DestroySemaphore(_logicalDevice, _renderFinishedSemaphore[i], null);
            _vk.DestroyFence(_logicalDevice, _fence[i], null);
        }

        foreach (Framebuffer fb in _framebuffers ?? []) {
            _vk.DestroyFramebuffer(_logicalDevice, fb, null);
        }

        _framebuffers = null;
        foreach (ImageView imageView in _swapChainImageViews ?? []) {
            _vk.DestroyImageView(_logicalDevice, imageView, null);
        }

        _swapChainImageViews = null;
        _swapChainExtension?.DestroySwapchain(_logicalDevice, _swapChain, null);
    }

    public void Cleanup(){
        CleanupSwapChain();
        _vk.DestroyBuffer(_logicalDevice, _buffer, null);
        _vk.FreeMemory(_logicalDevice, _vertexBufferMemory, null);
        _vk.DestroyCommandPool(_logicalDevice, _commandPool, null);

        _vk.DestroyPipeline(_logicalDevice, _pipeline, null);
        _vk.DestroyPipelineLayout(_logicalDevice, _pipelineLayout, null);
        _vk.DestroyRenderPass(_logicalDevice, _renderPass, null);

        _vk.DestroyDevice(_logicalDevice, null);
        _surfaceExtension?.DestroySurface(_instance, _surface, null);
        _vk.DestroyInstance(_instance, null);
        _vk.Dispose();
        _window?.Dispose();
    }
}