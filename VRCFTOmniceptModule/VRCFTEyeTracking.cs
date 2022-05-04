using HP.Omnicept.Messaging.Messages;
using VRCFaceTracking;
using VRCFaceTracking.Params;
using Eye = HP.Omnicept.Messaging.Messages.Eye;

namespace VRCFTOmniceptModule;

public class VRCFTEyeTracking
{
    public class VRCFTEye
    {
        public Vector2 Look;
        public float Openness;
        public float PupilDilate;

        private static float ProperRangeDilate(float dilation) => (dilation - 1.5f) / 6.5f;

        public void Update(Eye data)
        {
            Look = new Vector2(data.Gaze.X * -1, data.Gaze.Y);
            Openness = data.Openness;
            PupilDilate = ProperRangeDilate(data.PupilDilation);
        }
    }

    public class VRCFTEyeTrackingData
    {
        public VRCFTEye LeftEye;
        public VRCFTEye RightEye;

        public VRCFTEye CombinedEye => new()
        {
            Look = new Vector2((LeftEye.Look.x + LeftEye.Look.y) / 2, (RightEye.Look.x + RightEye.Look.y) / 2),
            Openness = (LeftEye.Openness + RightEye.Openness) / 2,
            PupilDilate = (LeftEye.PupilDilate + RightEye.PupilDilate) / 2
        };

        public VRCFTEyeTrackingData()
        {
            LeftEye = new VRCFTEye();
            RightEye = new VRCFTEye();
        }

        public void Update(EyeTracking data)
        {
            LeftEye.Update(data.LeftEye);
            RightEye.Update(data.RightEye);
        }
    }
    
    public static void UpdateEyeTrackingData(VRCFTEyeTrackingData data)
    {
        // LeftEye
        UnifiedTrackingData.LatestEyeData.Left.Look = data.LeftEye.Look;
        UnifiedTrackingData.LatestEyeData.Left.Openness = data.LeftEye.Openness;
        // RightEye
        UnifiedTrackingData.LatestEyeData.Right.Look = data.RightEye.Look;
        UnifiedTrackingData.LatestEyeData.Right.Openness = data.RightEye.Openness;
        // CombinedEye
        UnifiedTrackingData.LatestEyeData.Combined.Look = data.CombinedEye.Look;
        UnifiedTrackingData.LatestEyeData.Combined.Openness = data.CombinedEye.Openness;
        // Pupil
        UnifiedTrackingData.LatestEyeData.EyesDilation = data.CombinedEye.PupilDilate;
    }
}