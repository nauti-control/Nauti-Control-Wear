using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Util;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothManagerVM : ScanCallback
    {
        /// <summary>
        /// The UUID of the service to filter for
        /// </summary>
        private const string SERVICE_UUID = "778e5a27-1cc1-4bca-994f-7b2dbe34fcc6";

        /// <summary>
        /// Maximum scan period in milliseconds (2 minutes)
        /// </summary>
        private const long SCAN_PERIOD = 120000;

        /// <summary>
        /// Bluetooth Adapter
        /// </summary>
        private BluetoothAdapter? _bluetoothAdapter;

        /// <summary>
        /// Bluetooth Le Scanner
        /// </summary>
        private BluetoothLeScanner? _bluetoothLeScanner;

        /// <summary>
        /// Handler for scan timeout
        /// </summary>
        private Handler? _handler;

        /// <summary>
        /// Bluetooth Device VM
        /// </summary>
        private ObservableCollection<BluetoothDeviceVM> _bluetoothDeviceVMs = new ObservableCollection<BluetoothDeviceVM>();

        /// <summary>
        /// Bluetooth Devices
        /// </summary>
        public ObservableCollection<BluetoothDeviceVM> BluetoothDevices
        {
            get
            {
                return _bluetoothDeviceVMs;
            }
        }


        /// <summary>
        /// Is Scanning
        /// </summary>
        private bool _isScanning;


        /// <summary>
        /// Is Scanning Flag
        /// </summary>
        public bool IsScanning
        {
            get => _isScanning;
            set
            {
                if (value != _isScanning)
                {
                    _isScanning = value;
                    OnScanStatusChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        public event EventHandler? OnScanStatusChanged;
        
        /// <summary>
        /// Event triggered when scan fails
        /// </summary>
        public event EventHandler<ScanFailure>? OnScanFailedEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        public BluetoothManagerVM()
        {
            // Check for null Looper before creating Handler
            Looper? looper = Looper.MainLooper;
            if (looper != null)
            {
                _handler = new Handler(looper);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: MainLooper is null, scan timeout functionality will be disabled");
            }
        }

        /// <summary>
        /// Start Scanning
        /// </summary>
        public void StartScanning()
        {
            try
            {
                // Already scanning, don't start again
                if (IsScanning)
                {
                    System.Diagnostics.Debug.WriteLine("Already scanning, ignoring duplicate start request");
                    return;
                }
                
                // Get application context safely
                var context = Android.App.Application.Context;
                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: Application context is null");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }
                
                // Get bluetooth manager safely
                var bluetoothManager = (BluetoothManager?)context.GetSystemService(Context.BluetoothService);
                if (bluetoothManager == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: Could not get BluetoothManager service");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }

                // Get bluetooth adapter safely
                _bluetoothAdapter = bluetoothManager.Adapter;
                if (_bluetoothAdapter == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: BluetoothAdapter is null");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }

                // Get scanner safely
                _bluetoothLeScanner = _bluetoothAdapter.BluetoothLeScanner;
                if (_bluetoothLeScanner == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: BluetoothLeScanner is null");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }

                // Use simplified scan settings for better compatibility with Wear OS
                var settings = new ScanSettings.Builder();
                if (settings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: Failed to create ScanSettings.Builder");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }
                
                // Use balanced mode for better device discovery
                settings.SetScanMode(Android.Bluetooth.LE.ScanMode.Balanced);
                
                var scanSettings = settings.Build();
                if (scanSettings == null)
                {
                    System.Diagnostics.Debug.WriteLine("Error starting scan: Failed to build scan settings");
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                    return;
                }

                // Use an empty filter list to show all devices (more reliable on Wear OS)
                var filters = new List<ScanFilter>();
                
                try
                {
                    // Start the scan with minimal settings
                    _bluetoothLeScanner.StartScan(filters, scanSettings, this);
                    IsScanning = true;
                    
                    // Set a shorter timeout for Wear OS to prevent battery drain
                    _handler?.RemoveCallbacks(StopScanTimeout); // Remove any existing callbacks
                    _handler?.PostDelayed(StopScanTimeout, 60000); // 1 minute timeout instead of 2
                    
                    System.Diagnostics.Debug.WriteLine("Bluetooth LE scan started successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in StartScan: {ex.Message}");
                    IsScanning = false;
                    OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StartScanning: {ex.Message}");
                IsScanning = false;
                OnScanFailedEvent?.Invoke(this, ScanFailure.InternalError);
            }
        }

        /// <summary>
        /// Stop scan after timeout
        /// </summary>
        private void StopScanTimeout()
        {
            if (IsScanning)
            {
                StopScanning();
                System.Diagnostics.Debug.WriteLine("Scan stopped due to timeout");
            }
        }

        /// <summary>
        /// Stop Scanning
        /// </summary>
        public void StopScanning()
        {
            try
            {
                // Nothing to stop if not currently scanning
                if (!IsScanning)
                {
                    return;
                }
                
                if (_bluetoothLeScanner != null)
                {
                    try
                    {
                        _bluetoothLeScanner.StopScan(this);
                        System.Diagnostics.Debug.WriteLine("Bluetooth LE scan stopped successfully");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping scan: {ex.Message}");
                        // Continue to set IsScanning = false even if there was an error
                    }
                }
                
                // Always update state and remove callback
                IsScanning = false;
                _handler?.RemoveCallbacks(StopScanTimeout);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in StopScanning: {ex.Message}");
                // Ensure scanning flag is reset
                IsScanning = false;
            }
        }

        /// <summary>
        /// Scan Failed
        /// </summary>
        /// <param name="errorCode"></param>
        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            System.Diagnostics.Debug.WriteLine($"Scan failed with error code: {errorCode}");
            OnScanFailedEvent?.Invoke(this, errorCode);
        }

        /// <summary>
        /// On Scan Result
        /// </summary>
        /// <param name="callbackType">Call back</param>
        /// <param name="result">Result</param>
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            try
            {
                if (result?.Device == null)
                {
                    System.Diagnostics.Debug.WriteLine("Received null device in scan result");
                    return;
                }

                var device = result.Device;
                var deviceName = device.Name ?? "Unknown";
                var deviceAddress = device.Address ?? "Unknown Address";
                var rssi = result.Rssi;
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

                System.Diagnostics.Debug.WriteLine($"[{timestamp}] Found device:");
                System.Diagnostics.Debug.WriteLine($"  Name: {deviceName}");
                System.Diagnostics.Debug.WriteLine($"  Address: {deviceAddress}");
                System.Diagnostics.Debug.WriteLine($"  RSSI: {rssi} dBm");
                System.Diagnostics.Debug.WriteLine($"  Type: {device.Type}");
                System.Diagnostics.Debug.WriteLine($"  Bond State: {device.BondState}");
                
                if (result.ScanRecord != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Device Type: {result.ScanRecord.DeviceName}");
                    System.Diagnostics.Debug.WriteLine($"  Manufacturer Data: {result.ScanRecord.ManufacturerSpecificData}");
                }

                // Get the main thread handler
                var mainHandler = new Handler(Looper.MainLooper!);
                
                // Process on main thread to avoid threading issues with ObservableCollection
                mainHandler.Post(() => {
                    try
                    {
                        // Check if device already exists
                        var existingDevice = _bluetoothDeviceVMs.FirstOrDefault(d => d.Address == device.Address);
                        if (existingDevice != null)
                        {
                            // Update existing device
                            existingDevice.UpdateFromScanResult(result);
                            System.Diagnostics.Debug.WriteLine($"  Updated existing device: {deviceName}");
                        }
                        else
                        {
                            // Add new device
                            var deviceVM = new BluetoothDeviceVM(device);
                            deviceVM.UpdateFromScanResult(result);
                            _bluetoothDeviceVMs.Add(deviceVM);
                            System.Diagnostics.Debug.WriteLine($"  Added new device: {deviceName}");
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"  Total devices found: {_bluetoothDeviceVMs.Count}");
                        System.Diagnostics.Debug.WriteLine("----------------------------------------");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error adding/updating device on UI thread: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing scan result: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public override void OnBatchScanResults(IList<ScanResult>? results)
        {
            if (results == null)
            {
                System.Diagnostics.Debug.WriteLine("Received null batch scan results");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Received batch scan results: {results.Count} devices");
            foreach (var result in results)
            {
                OnScanResult(ScanCallbackType.AllMatches, result);
            }
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public new void Dispose()
        {
            StopScanning();
            _handler?.RemoveCallbacksAndMessages(null);
            _handler = null;
        }
    }
}