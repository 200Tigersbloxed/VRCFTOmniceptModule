using System;
using HP.Omnicept;
using HP.Omnicept.Messaging;
using HP.Omnicept.Messaging.Messages;
using VRCFaceTracking;

namespace VRCFTOmniceptModule;

public class GliaManager
{
    private Glia m_gliaClient;
    private GliaValueCache m_gliaValCache;
    
    public bool m_isConnected { get; private set; }
    public Action<EyeTracking> OnEyeTracking = tracking => { };

    public void StopGlia()
    { 
        // Verify Glia is Disposed
        if(m_gliaValCache != null)
            m_gliaValCache?.Stop();
        if(m_gliaClient != null)
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
            m_gliaClient = new Glia("HRtoVRChat_OSC",
                new SessionLicense(String.Empty, String.Empty, LicensingModel.Core, false));
            m_gliaValCache = new GliaValueCache(m_gliaClient.Connection);
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
        catch (Exception)
        {
            m_isConnected = false;
        }
        return m_isConnected;
    }
    
    void HandleMessage(ITransportMessage msg)
    {
        switch (msg.Header.MessageType)
        {
            case MessageTypes.ABI_MESSAGE_EYE_TRACKING:
                EyeTracking eyeTracking = m_gliaClient.Connection.Build<EyeTracking>(msg);
                OnEyeTracking.Invoke(eyeTracking);
                break;
        }
    }
        
    ITransportMessage RetrieveMessage()
    {
        ITransportMessage msg = null;
        if (m_gliaValCache != null)
        {
            try
            {
                msg = m_gliaValCache.GetNext();
            }
            catch (HP.Omnicept.Errors.TransportError e)
            {
                Logger.Error("[VRCFTOmniceptModule] Failed to start Glia! " + e);
            }
        }
        return msg;
    }

    public void UpdateMessage()
    {
        try
        {
            if (m_isConnected)
            {
                ITransportMessage msg = RetrieveMessage();
                if(msg != null)
                    HandleMessage(msg);
            }
        }
        catch (Exception e)
        {
            Logger.Error("[VRCFTOmniceptModule] Failed to get message! " + e);
        }
    }
}