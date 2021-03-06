using System;
using System.Threading;
using VRCFaceTracking;
using VRCFTOmniceptModule.EyeLidTools;

namespace VRCFTOmniceptModule;

public class OmniceptModule : ExtTrackingModule
{
    private GliaManager manager;
    private readonly VRCFTEyeTracking.VRCFTEyeTrackingData Data = new();
    
    public override (bool eyeSuccess, bool lipSuccess) Initialize(bool eye, bool lip)
    {
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
                Logger.Error("[VRCFTOmniceptModule] Failed to update VRCFaceTracking! " + e);
            }
        };
        if(start)
            SmoothFloatWorkers.Init();
        return (start, false);
    }

    public override Action GetUpdateThreadFunc() => () =>
    {
        while (true)
        {
            if (Status.EyeState == ModuleState.Active)
                manager?.UpdateMessage();
            Thread.Sleep(1);
        }
    };

    public override void Teardown()
    {
        manager?.StopGlia();
        manager = null;
        Data.LeftEye.Destroy();
        Data.RightEye.Destroy();
        SmoothFloatWorkers.Destroy();
    }

    public override (bool SupportsEye, bool SupportsLip) Supported => (true, false);
}