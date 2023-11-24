using HP.Omnicept;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;
using Microsoft.Extensions.Logging;

namespace VRCFTOmniceptModule;

public class GliaManager
{
    private Glia? m_gliaClient;
    private GliaLastValueCacheCustom? m_gliaValCache;

    public bool m_isConnected { get; private set; }
    public Action<EyeTracking> OnEyeTracking;

    public void StopGlia()
    {
        // Verify Glia is Disposed
        if (m_gliaValCache != null)
            m_gliaValCache?.Stop();
        if (m_gliaClient != null)
            m_gliaClient?.Dispose();
        m_gliaValCache = null;
        m_gliaClient = null;
        m_isConnected = false;
        Glia.cleanupNetMQConfig();
    }

    public bool StartGlia()
    {
        // Verify Glia is Disposed
        StopGlia();

        // Start Glia
        try
        {
            m_gliaClient = new Glia("VRCFTOmniceptModule",
                new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
            m_gliaValCache = new GliaLastValueCacheCustom(m_gliaClient.Connection);
            SubscriptionList sl = new SubscriptionList
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
            m_isConnected = false;
            OmniceptModule.logger?.LogError("[VRCFTOmniceptModule] Failed to load Glia for reason {E}", e);
        }
        m_gliaValCache!.OnEyeTracking += (eyetrack) => { OnEyeTracking?.Invoke(eyetrack); };
        return m_isConnected;
    }

    public EyeTracking GetEyeTracking()
    {
        return m_gliaValCache!.GetEyeData();
    }
}