using Android.Bluetooth;
using Android.Graphics;
using Android.Text;
using Android.Util;
using Java.Util;
using Nauti_Control_Wear.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Nauti_Control_Wear.ViewModels
{
    public class BluetoothDeviceVM : BluetoothGattCallback
    {
        private const string ServiceUUID = "778e5a27-1cc1-4bca-994f-7b2dbe34fcc6";

        private const string CmdCharacteristicUUID = "46ba71f1-c22c-42ae-832c-81414bde99ee";

        private const string AwaCharacteristicUUID = "fd3e532c-f882-499a-b2fd-c68cca630949";

        private const string AwsCharacteristicUUID = "6dce0177-7b5c-4274-a726-9202e292ec0c";

        private const string StwCharacteristicUUID = "83e0c967-4352-4187-9feb-d7cd3c89e01d";

        private const string SogCharacteristicUUID = "5eacdf71-af9a-458c-86db-e247db17e399";

        public static BluetoothDeviceVM? ConnectedInstance { get; set; }

        public BoatData Data { get; set; }

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

        private BluetoothGattCharacteristic? _cmdCharacteristic;
        private BluetoothGattCharacteristic? _awaCharacteristic;
        private BluetoothGattCharacteristic? _awsCharacteristic;
        private BluetoothGattCharacteristic? _stwCharacteristic;
        private BluetoothGattCharacteristic? _sogCharacteristic;
        private BluetoothGatt? _gatt;
        private BluetoothDevice _device;

        public event EventHandler? OnDeviceDisonnected;

        public event EventHandler? OnConnected;

        public event EventHandler? OnDataUpdated;


        public BluetoothDeviceVM(BluetoothDevice device)
        {
            _device = device;
            Data = new BoatData();
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
            if (_gatt != null)
            {
                _gatt.Disconnect();
            }

        }

        public void SendCommand(int command)
        {
            if (_cmdCharacteristic != null && _gatt != null)
            {
                _cmdCharacteristic.SetValue(command.ToString());
                _gatt.WriteCharacteristic(_cmdCharacteristic);
            }
        }
        public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
        {

            if (newState == ProfileState.Connected && gatt != null)
            {


                // Discover services and characteristics
                gatt.DiscoverServices();
            }
            else if (newState == ProfileState.Disconnected)
            {
                if (OnDeviceDisonnected != null)
                {
                    OnDeviceDisonnected(this, null);
                }
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, GattStatus status)
        {
            if (status == GattStatus.Success && gatt != null)
            {

                // Find the characteristic you want to write to
                BluetoothGattService? gattService = gatt.GetService(UUID.FromString(ServiceUUID));

                if (gattService != null)
                {
                    _cmdCharacteristic = gattService.GetCharacteristic(UUID.FromString(CmdCharacteristicUUID));

                    if (_cmdCharacteristic != null)
                    {
                        if (OnConnected != null)
                        {

                            Connected = true;
                            ConnectedInstance = this;
                            OnConnected(this, null);


                        }
                    }
                    _awaCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwaCharacteristicUUID));
                    if (_awaCharacteristic != null)
                    {
                        _gatt.SetCharacteristicNotification(_awaCharacteristic, true);
                        BluetoothGattDescriptor descriptor = _awaCharacteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                        descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                        _gatt.WriteDescriptor(descriptor);

                    }
                    _awsCharacteristic = gattService.GetCharacteristic(UUID.FromString(AwsCharacteristicUUID));
                    if (_awsCharacteristic != null)
                    {
                        _gatt.SetCharacteristicNotification(_awsCharacteristic, true);
                        BluetoothGattDescriptor descriptor = _awsCharacteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                        descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                        _gatt.WriteDescriptor(descriptor);
                    }
                    _stwCharacteristic = gattService.GetCharacteristic(UUID.FromString(StwCharacteristicUUID));

                    if (_stwCharacteristic != null)
                    {
                        _gatt.SetCharacteristicNotification(_stwCharacteristic, true);

                        BluetoothGattDescriptor descriptor = _stwCharacteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                        descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                        _gatt.WriteDescriptor(descriptor);
                    }
                    _sogCharacteristic = gattService.GetCharacteristic(UUID.FromString(SogCharacteristicUUID));
                    if (_sogCharacteristic != null)
                    {
                        _gatt.SetCharacteristicNotification(_sogCharacteristic, true);

                        BluetoothGattDescriptor descriptor = _sogCharacteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805f9b34fb"));
                        descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray());
                        _gatt.WriteDescriptor(descriptor);
                    }
                }
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// Await Characteristic Changes
        /// </summary>
        /// <param name="gatt">Gatt</param>
        /// <param name="characteristic">Characteristic</param>
        /// <param name="value">Value</param>

        public override void OnCharacteristicChanged(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic)
        {
            base.OnCharacteristicChanged(gatt, characteristic);

            if (characteristic != null)
            {
                byte[]? value = characteristic.GetValue();

                if (value != null)
                {
                    double dobValue = BitConverter.ToDouble(value);
                    if (UUID.FromString(AwaCharacteristicUUID) == characteristic.Uuid)
                    {
                        Data.AWA = dobValue;
                    }
                    else if (UUID.FromString(AwsCharacteristicUUID) == characteristic.Uuid)
                    {
                        Data.AWS = dobValue;
                    }
                    else if (UUID.FromString(StwCharacteristicUUID) == characteristic.Uuid)
                    {
                        Data.STW = dobValue;
                    }
                    else if (UUID.FromString(SogCharacteristicUUID) == characteristic.Uuid)
                    {
                        Data.SOG = dobValue;
                    }
                    if (OnDataUpdated != null)
                    {

                        OnDataUpdated(this, null);
                    }
                }



            }
        }
    }
}