﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nauti_Control_Wear.ViewModels
{
    public class CommandMenuVM
    {
        /// <summary>
        /// Menu Items
        /// </summary>
        public List<CommandItemVM> MenuItems { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CommandMenuVM ()
        {
            MenuItems = new List<CommandItemVM>();
            MenuItems.Add(new CommandItemVM("Minus 1",0));
            MenuItems.Add(new CommandItemVM("Plus 1",1));
            MenuItems.Add(new CommandItemVM("Minus 10",2));
            MenuItems.Add(new CommandItemVM("Plus 10",3));
            MenuItems.Add(new CommandItemVM("Port Tack",4));
            MenuItems.Add(new CommandItemVM("Stb Tack",5));
            MenuItems.Add(new CommandItemVM("Auto", 6));
            MenuItems.Add(new CommandItemVM("Stand By",7));        
            MenuItems.Add(new CommandItemVM("Track",8));
            MenuItems.Add(new CommandItemVM("Wind Mode",9));
            MenuItems.Add(new CommandItemVM("MOB Alert",10));
            MenuItems.Add(new CommandItemVM("Cancel MOB",11));
            MenuItems.Add(new CommandItemVM("Start Timer",12));



        }

        /// <summary>
        /// On Item Click
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="position">The position of the clicked item</param>
        public void OnItemClick(object? sender, int position)
        {
            var item = MenuItems[position];
            if (BluetoothDeviceVM.ConnectedInstance != null)
            {
                BluetoothDeviceVM.ConnectedInstance.SendCommand(item.Command);
            }

            Console.WriteLine(item.MenuText);
        }
    }
}