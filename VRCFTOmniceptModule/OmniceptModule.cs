using HP.Omnicept;
using HP.Omnicept.Errors;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;
using Microsoft.Extensions.Logging;
using NetMQ;
using System.Reflection;
using VRCFaceTracking;
using VRCFTOmniceptModule.EyeLidTools;

namespace VRCFTOmniceptModule;

public class OmniceptModule : ExtTrackingModule
{
    private Glia? m_gliaClient;
    private readonly VRCFTEyeTracking.VRCFTEyeTrackingData Data = new();
    private bool m_isConnected = false;

    public override (bool eyeSuccess, bool expressionSuccess) Initialize(bool eye, bool lip)
    {

        try
        {
            m_gliaClient = new Glia("VRCFTOmniceptModule",
                new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
            SubscriptionList sl = new()
            {
                Subscriptions =
                {
                    new Subscription(MessageTypes.ABI_MESSAGE_EYE_TRACKING, String.Empty, String.Empty,
                        String.Empty, String.Empty, new MessageVersionSemantic("1.0.0"))
                }
            };
            m_gliaClient.setSubscriptions(sl);
            m_isConnected = true;
        }
        catch (Exception e)
        {
            Logger?.LogError("[VRCFTOmniceptModule] Failed to load for reason {E}", e);
        }
        if (m_isConnected)
        {
            SmoothFloatWorkers.Init();
        }

        List<Stream> streams = new()
            {Assembly.GetExecutingAssembly().GetManifestResourceStream("VRCFTOmniceptModule.HMD.png")!};
        ModuleInformation = new ModuleMetadata
        {
            Name = "Omnicept Eye Tracking",
            StaticImages = streams
        };

        Logger?.LogDebug("[VRCFTOmniceptModule] Init status {Start}", m_isConnected);
        return (m_isConnected, false);
    }

    public override void Update()
    {
        if (m_isConnected)
        {
            ITransportMessage? transportMessage = null;
            try
            {
                transportMessage = m_gliaClient!.Connection.Receive(100);
            }
            catch (TransportError e)
            {
                Logger?.LogDebug("[VRCFTOmniceptModule] TransportError {e}", e);
            }
            catch (TerminatingException e)
            {
                Logger?.LogDebug("[VRCFTOmniceptModule] TerminatingException {e}", e);
            }
            if (transportMessage != null)
            {
                if (transportMessage.Header.MessageType == MessageTypes.ABI_MESSAGE_EYE_TRACKING)
                {
                    Data.Update(m_gliaClient!.Connection.Build<EyeTracking>(transportMessage));
                    VRCFTEyeTracking.UpdateEyeTrackingData(Data);
                }
            }
        }
    }

    public override void Teardown()
    {
        if (m_gliaClient != null)
        {
            m_gliaClient.Dispose();
            m_gliaClient = null;
        }
        m_isConnected = false;
        Glia.cleanupNetMQConfig();
        SmoothFloatWorkers.Destroy();
    }

    public override (bool SupportsEye, bool SupportsExpression) Supported => (true, false);
}