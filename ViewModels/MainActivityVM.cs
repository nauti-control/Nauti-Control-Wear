
using System;
using System.Collections.Generic;

namespace Nauti_Control_Wear.ViewModels
{
    public class MainActivityVM
    {
        public MainMenuVM MainMenuVM { get; private set; }  
        public  BluetoothManagerVM BluetoothManagerVM { get; private set; }
        public MainActivityVM()
        {
            BluetoothManagerVM=new BluetoothManagerVM();
            MainMenuVM= new MainMenuVM();
         


        }


    }
}