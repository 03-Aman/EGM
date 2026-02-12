using EGM.Core.Enums;
using EGM.Core.Interfaces;

namespace EGM.Core.Services
{
    public class BillValidatorService : IBillValidator, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IStateManager _stateManager;
        private Timer ?_heartbeatTimer;
        private bool _ackReceived;
        private bool _isSimulatedFailure;

        // Configuration constants
        private const int PingIntervalMs = 10000; // 10 seconds 
        private const int TimeoutMs = 2000;       // 2 seconds 

        public BillValidatorService(ILogger logger, IStateManager stateManager)
        {
            _logger = logger;
            _stateManager = stateManager;
        }

        public void Start()
        {
            _logger.Log(LogType.Info, "Bill Validator Service Started.");
            // Start the timer to run 'PingLoop' every 10 seconds
            _heartbeatTimer = new Timer(PingLoop, null, 0, PingIntervalMs);
        }

        public void Stop()
        {
            _heartbeatTimer?.Change(Timeout.Infinite, 0);
        }

        public void ReceiveAck()
        {
            // Only accept ACK if we aren't simulating a broken wire
            if (!_isSimulatedFailure)
            {
                _ackReceived = true;
                _logger.Log(LogType.Info, "Bill Validator ACK received."); // Optional: verify it works
            }
            else
            {
                _logger.Log(LogType.Warning, "Bill Validator ACK ignored (Simulated Failure Active).");
            }
        }

        public void SetSimulatedFailure(bool shouldFail)
        {
            _isSimulatedFailure = shouldFail;
            string status = shouldFail ? "BROKEN (No ACKs)" : "WORKING";
            _logger.Log(LogType.Warning, $"Bill Validator simulation set to: {status}");
        }

        // This runs on a background thread every 10s
        private void PingLoop(object state)
        {
            //  Reset ACK flag for this new cycle
            _ackReceived = false;

            // Log the Ping
            _logger.Log(LogType.Info, "[Hardware] Pinging Bill Validator...");

            //    Simulate automatic ACK (In a real scenario, hardware does this. 
            //    Here, we assume it works unless 'SetSimulatedFailure' is true).
            //    *The prompt implies the SYSTEM sends ping, Validator returns ACK.*
            //    *Let's simulate the DEVICE responding automatically after 500ms if not broken.*

            Task.Delay(500).ContinueWith(_ =>
            {
                if (!_isSimulatedFailure) ReceiveAck();
            });

            //  Wait for the Timeout window (2s) to check result
            Task.Delay(TimeoutMs).ContinueWith(_ => CheckForAck());
        }

        private void CheckForAck()
        {
            if (!_ackReceived)
            {
                _logger.Log(LogType.Error, "[Hardware] Bill Validator Timeout! No ACK received.");

                // CRITICAL: Transition to MAINTENANCE
                _stateManager.ForceState(EGMStateEnum.MAINTENANCE, "Bill Validator Hardware Failure");
            }
        }

        public void Dispose()
        {
            _heartbeatTimer?.Dispose();
        }
    }
}