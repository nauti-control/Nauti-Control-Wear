using Android.Views;
using Nauti_Control_Wear.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauti_Control_Wear.Adapters
{
    public class BluetoothListAdapter : BaseAdapter<BluetoothDeviceVM>
    {
        private BluetoothManagerVM _bluetoothManager;

        public BluetoothListAdapter(BluetoothManagerVM bluetoothManager)
        {
            this._bluetoothManager = bluetoothManager;
            _bluetoothManager.BluetoothDevices.CollectionChanged += BluetoothDevices_CollectionChanged;
        }

        /// <summary>
        /// Collection Changed Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BluetoothDevices_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetInvalidated();
        }

        /// <summary>
        /// Bluetooth Device VM
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public override BluetoothDeviceVM this[int position]
        {
            get
            {
                return _bluetoothManager.BluetoothDevices[position];
            }
        }

        /// <summary>
        /// Count
        /// </summary>
        public override int Count
        {
            get
            {
                return _bluetoothManager.BluetoothDevices.Count;
            }
        }


        /// <summary>
        /// Get Item Id
        /// </summary>
        /// <param name="position">Position</param>
        /// <returns>Position</returns>
        public override long GetItemId(int position)
        {
            return position;
        }

        /// <summary>
        /// Get View
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="convertView">Convert View</param>
        /// <param name="parent">Parent </param>
        /// <returns>View</returns>
        public override View? GetView(int position, View? convertView, ViewGroup? parent)
        {
            View? view = convertView;

            if (view == null)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.bluetooth_item, parent, false);

                TextView? label = view.FindViewById<TextView>(Resource.Id.bt_text);

                if (label != null)
                {
                    view.Tag = new BluetoothViewHolder(label);
                }
            }

            else if (view.Tag != null)
            {
                BluetoothViewHolder? holder = view.Tag as BluetoothViewHolder;

                if (holder != null && holder.Name != null)
                {
                    holder.Name.Text = _bluetoothManager.BluetoothDevices[position].Name;
                }
            }

            return view;
        }
    }
}
