using HP.Omnicept.Messaging.Messages;
using VRCFaceTracking;
using VRCFaceTracking.Core.Types;
using VRCFTOmniceptModule.EyeLidTools;
using Eye = HP.Omnicept.Messaging.Messages.Eye;

namespace VRCFTOmniceptModule;

public class VRCFTEyeTracking
{
    public class VRCFTEye
    {
        private EyeType _eyeType;
        
        public Vector2 Look;
        public float Openness
        {
            get
            {
                switch (_eyeType)
                {
                    case EyeType.Left:
                        return Smoothened_Openness_Left.Value;
                    case EyeType.Right:
                        return Smoothened_Openness_Right.Value;
                }
                return (Smoothened_Openness_Left.Value + Smoothened_Openness_Right.Value) / 2;
            }
            set
            {
                switch (_eyeType)
                {
                    case EyeType.Left:
                        Smoothened_Openness_Left.Value = value;
                        break;
                    case EyeType.Right:
                        Smoothened_Openness_Right.Value = value;
                        break;
                }
            }
        }
        
        public float PupilDilate;

        private static SmoothFloat Smoothened_Openness_Left = new();
        private static SmoothFloat Smoothened_Openness_Right = new();

        private static float ProperRangeDilate(float dilation) => (dilation - 1.5f) / 6.5f;

        public void Update(Eye data)
        {
            if (data.Gaze.Confidence >= 0.25f)
                Look = new Vector2(data.Gaze.X * -1, data.Gaze.Y);
            if(data.OpennessConfidence >= 0.25f)
                Openness = data.Openness;
            if(data.PupilDilationConfidence >= 0.25f)
                PupilDilate = ProperRangeDilate(data.PupilDilation);
        }

        public VRCFTEye(EyeType eyeType) => _eyeType = eyeType;

        public enum EyeType
        {
            Left,
            Right,
            Combined
        }
    }

    public class VRCFTEyeTrackingData
    {
        public VRCFTEye LeftEye = new(VRCFTEye.EyeType.Left);
        public VRCFTEye RightEye = new(VRCFTEye.EyeType.Right);

        public VRCFTEye CombinedEye => new(VRCFTEye.EyeType.Combined)
        {
            Look = new Vector2((LeftEye.Look.x + RightEye.Look.x) / 2, (LeftEye.Look.y + RightEye.Look.y) / 2),
            Openness = (LeftEye.Openness + RightEye.Openness) / 2,
            PupilDilate = (LeftEye.PupilDilate + RightEye.PupilDilate) / 2
        };

        public void Update(EyeTracking data)
        {
            LeftEye.Update(data.LeftEye);
            RightEye.Update(data.RightEye);
        }
    }
    
    public static void UpdateEyeTrackingData(VRCFTEyeTrackingData data)
    {
        // LeftEye
        UnifiedTracking.Data.Eye.Left.Gaze = data.LeftEye.Look;
        UnifiedTracking.Data.Eye.Left.Openness = data.LeftEye.Openness;
        UnifiedTracking.Data.Eye.Left.PupilDiameter_MM = data.LeftEye.PupilDilate;
        // RightEye
        UnifiedTracking.Data.Eye.Right.Gaze = data.RightEye.Look;
        UnifiedTracking.Data.Eye.Right.Openness = data.RightEye.Openness;
        UnifiedTracking.Data.Eye.Right.PupilDiameter_MM = data.RightEye.PupilDilate;
    }
}