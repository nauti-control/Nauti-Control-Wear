using Android.Bluetooth;
using Android.Text;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothDeviceVM : BluetoothGattCallback
    {
        public bool Connected { get; set; }

        public string? Name
        {
            get
            {
                return _device.Name;
            }
        }

        ///
        public string? Address
        {
            get { return _device.Address; }
        }

        private BluetoothGattCharacteristic? _characteristic;
        private BluetoothGatt? _gatt;
        private BluetoothDevice _device;

        public event EventHandler OnDeviceDisonnected;


        public BluetoothDeviceVM(BluetoothDevice device)
        {
            _device = device;
        }

        public void Connect()
        {
            if (_device != null)
            {
                _gatt = _device.ConnectGatt(null, false, this);
            }

        }

        public void Disconnect()
        {
            _gatt.Disconnect();
            if (OnDeviceDisonnected != null)
            {
                OnDeviceDisonnected(this, null);
            }
        }

        public void SendCommand(int command)
        {

            _characteristic.SetValue(command.ToString());
            _gatt.WriteCharacteristic(_characteristic);
        }
        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {

            if (newState == ProfileState.Connected)
            {


                // Discover services and characteristics
                gatt.DiscoverServices();
            }
            else if (newState == ProfileState.Disconnected)
            {

            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            if (status == GattStatus.Success)
            {

                // Find the characteristic you want to write to
                BluetoothGattService gattService = gatt.GetService(UUID.FromString("3fbc5eb7-ca52-42fe-a43c-5f980e555436"));

                if (gattService != null)
                {
                    _characteristic = gattService.GetCharacteristic(UUID.FromString("46ba71f1-c22c-42ae-832c-81414bde99ee"));
                    if (_characteristic != null)
                    {
                        Debug.WriteLine("Connected To Device");
                        Connected = true;
                    }
                }
            }
            else
            {

            }
        }
    }
}