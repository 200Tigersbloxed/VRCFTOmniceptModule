using Microsoft.Extensions.Logging;
using System.Reflection;
using VRCFaceTracking;
using VRCFaceTracking.Core.Library;
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
        if(manager == null)
            manager = new GliaManager();
        bool start = manager.StartGlia();
        manager.OnEyeTracking += tracking =>
        {
            Data.Update(tracking);
            try
            {
                VRCFTEyeTracking.UpdateEyeTrackingData(Data);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "[VRCFTOmniceptModule] Failed to update VRCFaceTracking! {E}", e);
            }
        };
        if(start)
            SmoothFloatWorkers.Init();

        List<Stream> streams = new();
        Assembly a = Assembly.GetExecutingAssembly();
        var hmdStream = a.GetManifestResourceStream
            ("VRCFTOmniceptModule.HMD.png");
        streams.Add(hmdStream!);
        ModuleInformation = new ModuleMetadata()
        {
            Name = "Omnicept Eye Tracking",
            StaticImages = streams
        };

        Logger.Log(LogLevel.Debug, "[VRCFTOmniceptModule] Init status {Start}", start);
        return (start, false);
    }

    public override void Update()
    {
        if (Status == ModuleState.Active)
            manager?.UpdateMessage();
    }

    public override void Teardown()
    {
        manager?.StopGlia();
        manager = null;
        SmoothFloatWorkers.Destroy();
    }

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);
}