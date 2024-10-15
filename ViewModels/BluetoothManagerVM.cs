using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.Util;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothManagerVM : ScanCallback
    {
        /// <summary>
        /// Bluetooth Adapter
        /// </summary>
        private BluetoothAdapter? _bluetoothAdapter;

        /// <summary>
        /// Bluetooth Le Scanner
        /// </summary>
        private BluetoothLeScanner? _bluetoothLeScanner;

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
            get
            {
                return _isScanning;
            }

            set
            {
                if (value != _isScanning)
                {

                    _isScanning = value;
                    if (OnScanStatusChanged != null)
                    {
                        OnScanStatusChanged(this, EventArgs.Empty);
                    }
                }

            }
        }


        public event EventHandler? OnScanStatusChanged;

        /// <summary>
        /// Start Scanning
        /// </summary>
        public void StartScanning()
        {
            BluetoothManager? bluetoothManager = Android.App.Application.Context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            if (bluetoothManager != null)
            {
                _bluetoothAdapter = bluetoothManager.Adapter;
                if (_bluetoothAdapter != null)
                {
                    _bluetoothLeScanner = _bluetoothAdapter.BluetoothLeScanner;

                    if (_bluetoothLeScanner != null)
                    {

                        List<ScanFilter> filters = new List<ScanFilter>();
                        ScanSettings settings = new ScanSettings.Builder()
                                                       .SetScanMode(Android.Bluetooth.LE.ScanMode.Balanced)
                                                       .Build();
                        //ScanFilter? filter = new ScanFilter.Builder().SetServiceUuid(ParcelUuid.FromString("778e5a27-1cc1-4bca-994f-7b2dbe34fcc6")).Build();
                        //filters.Add(filter);


                        _bluetoothLeScanner.StartScan(filters, settings, this);

                        IsScanning = true;

                    }
                }
            }
        }

        /// <summary>
        /// Stop Scanning
        /// </summary>
        public void StopScanning()
        {
            if (_bluetoothLeScanner != null)
            {

                _bluetoothLeScanner.StopScan(this);
                IsScanning = false;
            }
        }

        /// <summary>
        /// Scan Failed
        /// </summary>
        /// <param name="errorCode"></param>
        public override void OnScanFailed([GeneratedEnum] ScanFailure errorCode)
        {
            System.Diagnostics.Debug.WriteLine("Scan Failure = " + errorCode.ToString());
            base.OnScanFailed(errorCode);
            IsScanning = false;
        }



        /// <summary>
        /// On Scan Result
        /// </summary>
        /// <param name="callbackType">Call back</param>
        /// <param name="result">Result</param>
        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            base.OnScanResult(callbackType, result);

            if (result != null && result.Device != null && result.Device.Name != null)
            {
                if (!BluetoothDevices.Select(m => m.Address == result.Device.Address).Any())
                {
                    System.Diagnostics.Debug.WriteLine("Bluetooth Device Found=" + result.Device?.Name?.ToString());

                    BluetoothDeviceVM bluetoothDeviceVM = new BluetoothDeviceVM(result.Device);
                    BluetoothDevices.Add(bluetoothDeviceVM);

                }

            }
        }

    }
}