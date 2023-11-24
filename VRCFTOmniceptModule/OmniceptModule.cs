using Microsoft.Extensions.Logging;
using System.Reflection;
using VRCFaceTracking;
using VRCFTOmniceptModule.EyeLidTools;

namespace VRCFTOmniceptModule;

public class OmniceptModule : ExtTrackingModule
{
    internal static ILogger? logger;
    private GliaManager? manager;
    private readonly VRCFTEyeTracking.VRCFTEyeTrackingData Data = new();

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eye, bool lip)
    {
        logger = Logger;
        if (manager == null)
            manager = new GliaManager();
        bool start = manager.StartGlia();
        if (start)
            SmoothFloatWorkers.Init();

        manager.OnEyeTracking += tracking => //When new ft recieved from omnicept, update
        {
            Data.Update(tracking);
            VRCFTEyeTracking.UpdateEyeTrackingData(Data);
        };

        List<Stream> streams = new()
            {Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCFTOmniceptModule.HMD.png")!};
        ModuleInformation = new ModuleMetadata
        {
            Name = "Omnicept Eye Tracking",
            StaticImages = streams
        };

        Logger.LogDebug("[VRCFTOmniceptModule] Init status {Start}", start);
        return (start, false);
    }

    public override void Update()
    {
        //We just stop this from using an entire cpu core by making it sleep
        Thread.Sleep(200);
    }

    public override void Teardown()
    {
        manager?.StopGlia();
        manager = null;
        SmoothFloatWorkers.Destroy();
    }

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);
}