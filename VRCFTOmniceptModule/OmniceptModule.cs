using System;
using System.Threading;
using VRCFaceTracking;

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
        return (start, false);
    }

    public override Action GetUpdateThreadFunc() => () =>
    {
        while (true)
        {
            manager?.UpdateMessage();
            Thread.Sleep(1);
        }
    };

    public override void Teardown()
    {
        manager?.StopGlia();
        manager = null;
    }

    public override (bool SupportsEye, bool SupportsLip) Supported => (true, false);
}