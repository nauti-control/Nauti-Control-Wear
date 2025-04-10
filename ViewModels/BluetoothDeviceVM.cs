using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Java.Util;
using Nauti_Control_Wear.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothDeviceVM : BluetoothGattCallback, IDisposable
    {
        private const string ServiceUUID = "778e5a27-1cc1-4bca-994f-7b2dbe34fcc6";

        private const string CmdCharacteristicUUID = "46ba71f1-c22c-42ae-832c-81414bde99ee";

        private const string AwaCharacteristicUUID = "fd3e532c-f882-499a-b2fd-c68cca630949";

        private const string AwsCharacteristicUUID = "6dce0177-7b5c-4274-a726-9202e292ec0c";

        private const string StwCharacteristicUUID = "83e0c967-4352-4187-9feb-d7cd3c89e01d";

        private const string SogCharacteristicUUID = "5eacdf71-af9a-458c-86db-e247db17e399";

        private const string CogCharacteristicUUID = "dc0439f8-23bc-44b0-8e0c-a8e06272f4fa";

        private const string HdgCharacteristicUUID = "799ee17b-5f1a-4962-b236-ac80185d9186";

        private const string DptCharacteristicUUID = "7fdda184-b61a-4358-9213-41a5f934a7bb";

        /// <summary>
        /// Connection timeout in milliseconds (10 seconds)
        /// </summary>
        private const long CONNECTION_TIMEOUT = 10000;

        private List<BluetoothGattCharacteristic?> _toSubscribe = new List<BluetoothGattCharacteristic?>();

        /// <summary>
        /// Handler for connection timeout
        /// </summary>
        private Handler? _timeoutHandler;

        /// <summary>
        /// Whether we're currently in the connection process
        /// </summary>
        private bool _connecting;

        public static BluetoothDeviceVM? ConnectedInstance { get; set; }

        public BoatData Data { get; private set; } = new BoatData();

        public bool Connected { get; private set; }

        public bool Connecting => _connecting;

        public string? Name => _device?.Name ?? $"Unknown Device ({_device?.Address})";

        public string? Address => _device?.Address;

        private BluetoothGattCharacteristic? _cmdCharacteristic;
        private BluetoothGattCharacteristic? _awaCharacteristic;
        private BluetoothGattCharacteristic? _awsCharacteristic;
        private BluetoothGattCharacteristic? _stwCharacteristic;
        private BluetoothGattCharacteristic? _sogCharacteristic;
        private BluetoothGattCharacteristic? _cogCharacteristic;
        private BluetoothGattCharacteristic? _hdgCharacteristic;
        private BluetoothGattCharacteristic? _dptCharacteristic;
        private BluetoothGatt? _gatt;
        private readonly BluetoothDevice _device;

        public event EventHandler? OnDeviceDisonnected;
        public event EventHandler? OnConnected;
        public event EventHandler? OnDataUpdated;
        public event EventHandler? OnConnectionTimeout;

        /// <summary>
        /// Signal strength in dBm
        /// </summary>
        public int Rssi { get; private set; }

        public BluetoothDeviceVM(BluetoothDevice device)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            
            // Check for null Looper before creating Handler
            Looper? looper = Looper.MainLooper;
            if (looper != null)
            {
                _timeoutHandler = new Handler(looper);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Warning: MainLooper is null, timeout functionality will be disabled");
            }
        }

        public void Connect()
        {
            if (_connecting || Connected)
            {
                System.Diagnostics.Debug.WriteLine($"Already connecting or connected to {Name}");
                return;
            }

            try
            {
                _connecting = true;
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] Connecting to {Name}:");
                System.Diagnostics.Debug.WriteLine($"  Address: {Address}");
                System.Diagnostics.Debug.WriteLine($"  Type: {_device.Type}");
                System.Diagnostics.Debug.WriteLine($"  Bond State: {_device.BondState}");
                
                _gatt = _device.ConnectGatt(Android.App.Application.Context, false, this);
                
                // Set a timeout for the connection
                _timeoutHandler?.PostDelayed(ConnectionTimedOut, CONNECTION_TIMEOUT);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error connecting to device: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                _connecting = false;
                throw;
            }
        }

        /// <summary>
        /// Called when connection times out
        /// </summary>
        private void ConnectionTimedOut()
        {
            if (_connecting && !Connected)
            {
                System.Diagnostics.Debug.WriteLine($"Connection to {Name} timed out");
                Disconnect();
                _connecting = false;
                OnConnectionTimeout?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Disconnect()
        {
            // Cancel any pending timeout
            _timeoutHandler?.RemoveCallbacks(ConnectionTimedOut);
            
            if (_gatt != null)
            {
                try
                {
                    _gatt.Disconnect();
                    _gatt.Close();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disconnecting: {ex.Message}");
                }
                finally
                {
                    _gatt = null;
                    Connected = false;
                    _connecting = false;
                    
                    if (ConnectedInstance == this)
                    {
                        ConnectedInstance = null;
                    }
                }
            }
        }

        public void SendCommand(int command)
        {
            if (_gatt == null || _cmdCharacteristic == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot send command: not connected or command characteristic not found");
                return;
            }

            try
            {
                #pragma warning disable CA1422 // Suppress warning about deprecated APIs
                _cmdCharacteristic.SetValue(command.ToString());
                bool success = _gatt.WriteCharacteristic(_cmdCharacteristic);
                #pragma warning restore CA1422
                
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to write command {command}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error sending command: {ex.Message}");
            }
        }
        
        public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            System.Diagnostics.Debug.WriteLine($"[{timestamp}] Connection state changed for {Name}:");
            System.Diagnostics.Debug.WriteLine($"  New State: {newState}");
            System.Diagnostics.Debug.WriteLine($"  Status: {status}");
            
            // Cancel any pending timeouts regardless of connection outcome
            _timeoutHandler?.RemoveCallbacks(ConnectionTimedOut);
            _connecting = false;
            
            if (newState == ProfileState.Connected && gatt != null)
            {
                if (status == GattStatus.Success)
                {
                    System.Diagnostics.Debug.WriteLine($"  Connected successfully, starting service discovery...");
                    bool success = gatt.DiscoverServices();
                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine("  Failed to start service discovery");
                        Disconnect();
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  Connected with non-success status: {status}");
                    Disconnect();
                }
            }
            else if (newState == ProfileState.Disconnected)
            {
                System.Diagnostics.Debug.WriteLine($"  Disconnected from device");
                Connected = false;
                
                if (ConnectedInstance == this)
                {
                    ConnectedInstance = null;
                }
                
                // Close GATT client
                if (gatt != null)
                {
                    try
                    {
                        gatt.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Error closing GATT: {ex.Message}");
                    }
                }
                
                OnDeviceDisonnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
        {
            if (status != GattStatus.Success || gatt == null) 
            {
                System.Diagnostics.Debug.WriteLine($"Service discovery failed with status: {status}");
                Disconnect();
                return;
            }

            try
            {
                BluetoothGattService? gattService = gatt.GetService(UUID.FromString(ServiceUUID));
                if (gattService == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Service {ServiceUUID} not found");
                    Disconnect();
                    return;
                }

                _cmdCharacteristic = gattService.GetCharacteristic(UUID.FromString(CmdCharacteristicUUID));
                if (_cmdCharacteristic == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Command characteristic {CmdCharacteristicUUID} not found");
                    Disconnect();
                    return;
                }

                Connected = true;
                ConnectedInstance = this;
                System.Diagnostics.Debug.WriteLine($"Successfully connected to {Name}");
                OnConnected?.Invoke(this, EventArgs.Empty);

                // Get all the characteristics and assign them
                _awsCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwsCharacteristicUUID));
                if (_awsCharacteristic != null) _toSubscribe.Add(_awsCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"AWS characteristic not found");

                _awaCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwaCharacteristicUUID));
                if (_awaCharacteristic != null) _toSubscribe.Add(_awaCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"AWA characteristic not found");

                _stwCharacteristic = gattService.GetCharacteristic(UUID.FromString(StwCharacteristicUUID));
                if (_stwCharacteristic != null) _toSubscribe.Add(_stwCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"STW characteristic not found");

                _cogCharacteristic = gattService.GetCharacteristic(UUID.FromString(CogCharacteristicUUID));
                if (_cogCharacteristic != null) _toSubscribe.Add(_cogCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"COG characteristic not found");

                _sogCharacteristic = gattService.GetCharacteristic(UUID.FromString(SogCharacteristicUUID));
                if (_sogCharacteristic != null) _toSubscribe.Add(_sogCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"SOG characteristic not found");

                _hdgCharacteristic = gattService.GetCharacteristic(UUID.FromString(HdgCharacteristicUUID));
                if (_hdgCharacteristic != null) _toSubscribe.Add(_hdgCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"HDG characteristic not found");

                _dptCharacteristic = gattService.GetCharacteristic(UUID.FromString(DptCharacteristicUUID));
                if (_dptCharacteristic != null) _toSubscribe.Add(_dptCharacteristic);
                else System.Diagnostics.Debug.WriteLine($"DPT characteristic not found");

                if (_toSubscribe.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Subscribing to {_toSubscribe.Count} characteristics");
                    EnableNotification(gatt);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No characteristics to subscribe to");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during service discovery: {ex.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// Enable Notification
        /// </summary>
        /// <param name="gatt"></param>
        private void EnableNotification(BluetoothGatt? gatt)
        {
            if (gatt == null || _toSubscribe.Count == 0) return;

            try
            {
                BluetoothGattCharacteristic? characteristic = _toSubscribe[0];
                if (characteristic == null) return;

                bool success = gatt.SetCharacteristicNotification(characteristic, true);
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to enable notification for characteristic");
                    _toSubscribe.RemoveAt(0);
                    if (_toSubscribe.Count > 0)
                    {
                        EnableNotification(gatt);
                    }
                    return;
                }

                BluetoothGattDescriptor? descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                if (descriptor == null)
                {
                    System.Diagnostics.Debug.WriteLine("Client Configuration Descriptor not found");
                    _toSubscribe.RemoveAt(0);
                    if (_toSubscribe.Count > 0)
                    {
                        EnableNotification(gatt);
                    }
                    return;
                }

                byte[]? configValue = BluetoothGattDescriptor.EnableNotificationValue?.ToArray();
                if (configValue == null)
                {
                    System.Diagnostics.Debug.WriteLine("EnableNotificationValue is null");
                    _toSubscribe.RemoveAt(0);
                    if (_toSubscribe.Count > 0)
                    {
                        EnableNotification(gatt);
                    }
                    return;
                }

                #pragma warning disable CA1422 // Suppress warning about deprecated APIs
                descriptor.SetValue(configValue);
                success = gatt.WriteDescriptor(descriptor);
                #pragma warning restore CA1422
                
                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to write descriptor");
                    _toSubscribe.RemoveAt(0);
                    if (_toSubscribe.Count > 0)
                    {
                        EnableNotification(gatt);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error enabling notification: {ex.Message}");
                _toSubscribe.RemoveAt(0);
                if (_toSubscribe.Count > 0)
                {
                    EnableNotification(gatt);
                }
            }
        }

        /// <summary>
        /// On Descriptor Write
        /// </summary>
        /// <param name="gatt">gatt</param>
        /// <param name="descriptor">descriptor</param>
        /// <param name="status">status</param>
        public override void OnDescriptorWrite(BluetoothGatt? gatt, BluetoothGattDescriptor? descriptor, [GeneratedEnum] GattStatus status)
        {
            base.OnDescriptorWrite(gatt, descriptor, status);
            
            if (status != GattStatus.Success)
            {
                System.Diagnostics.Debug.WriteLine($"Descriptor write failed with status: {status}");
            }
            
            if (_toSubscribe.Count > 0)
            {
                _toSubscribe.RemoveAt(0);
                if (_toSubscribe.Count > 0)
                {
                    EnableNotification(gatt);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("All characteristics subscribed successfully");
                }
            }
        }

        /// <summary>
        /// Await Characteristic Changes
        /// </summary>
        /// <param name="gatt">Gatt</param>
        /// <param name="characteristic">Characteristic</param>
        #pragma warning disable CA1422 // Suppress warning about deprecated APIs
        public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            if (characteristic?.Uuid == null) return;

            try
            {
                byte[]? value = characteristic.GetValue();
                if (value == null || value.Length < 8) return;  // Double is 8 bytes

                double dobValue = BitConverter.ToDouble(value);
                UUID uuid = characteristic.Uuid;  // We know it's not null from the check above
                    
                var data = Data;  // Cache the reference to avoid multiple property accesses

                // Using pattern matching to make the code more concise and safer
                if (uuid.Equals(UUID.FromString(AwaCharacteristicUUID)))
                    data.AWA = dobValue;
                else if (uuid.Equals(UUID.FromString(AwsCharacteristicUUID)))
                    data.AWS = dobValue;
                else if (uuid.Equals(UUID.FromString(StwCharacteristicUUID)))
                    data.STW = dobValue;
                else if (uuid.Equals(UUID.FromString(SogCharacteristicUUID)))
                    data.SOG = dobValue;
                else if (uuid.Equals(UUID.FromString(CogCharacteristicUUID)))
                    data.COG = dobValue;
                else if (uuid.Equals(UUID.FromString(HdgCharacteristicUUID)))
                    data.HDG = dobValue;
                else if (uuid.Equals(UUID.FromString(DptCharacteristicUUID)))
                    data.DPT = dobValue;

                OnDataUpdated?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing characteristic value: {ex.Message}");
            }
        }
        #pragma warning restore CA1422

        /// <summary>
        /// Update Boat Data
        /// </summary>
        /// <param name="data">Boat Data</param>
        public void UpdateBoatData(BoatData data)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            OnDataUpdated?.Invoke(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Dispose of resources
        /// </summary>
        public new void Dispose()
        {
            // Disconnect if connected
            Disconnect();
            
            // Clean up handler
            _timeoutHandler?.RemoveCallbacksAndMessages(null);
            _timeoutHandler = null;
            
            // Clear event handlers
            OnDeviceDisonnected = null;
            OnConnected = null;
            OnDataUpdated = null;
            OnConnectionTimeout = null;
        }

        /// <summary>
        /// Update device information from scan result
        /// </summary>
        /// <param name="result">Scan result containing updated device information</param>
        public void UpdateFromScanResult(ScanResult result)
        {
            if (result == null)
            {
                System.Diagnostics.Debug.WriteLine("Received null scan result");
                return;
            }

            try
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                var oldRssi = Rssi;
                Rssi = result.Rssi;
                
                System.Diagnostics.Debug.WriteLine($"[{timestamp}] Updated device info for {Name}:");
                System.Diagnostics.Debug.WriteLine($"  RSSI changed: {oldRssi} dBm -> {Rssi} dBm");
                
                if (result.ScanRecord != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Device Type: {result.ScanRecord.DeviceName}");
                    System.Diagnostics.Debug.WriteLine($"  Manufacturer Data: {result.ScanRecord.ManufacturerSpecificData}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating from scan result: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}