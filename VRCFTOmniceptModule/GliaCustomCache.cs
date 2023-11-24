using HP.Omnicept.Errors;
using HP.Omnicept.Messaging.Messages;
using HP.Omnicept.Messaging;
using HP.Omnicept;
using NetMQ;

namespace VRCFTOmniceptModule
{
    public class GliaLastValueCacheCustom
    {
        private readonly Thread handler;

        private readonly ITransportMessage[] LastTransportValues;

        private readonly object TransportArrayLock = new();

        private readonly IGliaConnection m_connection;

        private volatile bool m_shouldThreadContinue;

        public event Action<Exception>? TransportException;

        public Action<EyeTracking> OnEyeTracking;

        public GliaLastValueCacheCustom(IGliaConnection connection)
        {
            m_connection = connection;
            LastTransportValues = new ITransportMessage[43];
            m_shouldThreadContinue = true;
            handler = new Thread(new ThreadStart(ThreadFunction));
            handler.Start();
        }
        ~GliaLastValueCacheCustom()
        {
            Stop();
        }
        public void Stop()
        {
            m_shouldThreadContinue = false;
            handler.Join();
        }
        private void readChannel()
        {
            ITransportMessage? transportMessage = null;
            try
            {
                transportMessage = m_connection.Receive(100);

            }
            catch (TransportError e)
            {
                m_shouldThreadContinue = false;
                if (TransportException != null)
                    TransportException.Invoke(e);
            }
            catch (TerminatingException e)
            {
                m_shouldThreadContinue = false;
                if (TransportException != null)
                    TransportException.Invoke(e);
            }
            if (transportMessage != null)
            {
                lock (TransportArrayLock)
                {
                    LastTransportValues[transportMessage.Header.MessageType] = transportMessage;
                }
                if (LastTransportValues[MessageTypes.ABI_MESSAGE_EYE_TRACKING] != null && OnEyeTracking!= null)
                {
                    OnEyeTracking.Invoke(m_connection.Build<EyeTracking>(LastTransportValues[MessageTypes.ABI_MESSAGE_EYE_TRACKING]));
                }
            }
        }
        private void ThreadFunction()
        {
            using (m_connection)
            {
                while (m_shouldThreadContinue)
                {
                    readChannel();
                }
            }
        }
        public T? ReturnLastMessageOfType<T>(uint messageType) where T : IDomainType
        {
            lock (TransportArrayLock)
            {

                if (LastTransportValues[messageType] != null)
                    return m_connection.Build<T>(LastTransportValues[messageType]);
            }
            return default;
        }
        public EyeTracking? GetEyeData()
        {
            lock (TransportArrayLock)
            {
                if (LastTransportValues[MessageTypes.ABI_MESSAGE_EYE_TRACKING] != null)
                    return m_connection.Build<EyeTracking>(LastTransportValues[MessageTypes.ABI_MESSAGE_EYE_TRACKING]);
            }
            return default;
        }
    }
}