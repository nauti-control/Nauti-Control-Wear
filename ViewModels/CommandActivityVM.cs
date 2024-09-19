
using System;
using System.Collections.Generic;

namespace Nauti_Control_Wear.ViewModels
{
    public class CommandActivityVM
    {
        public CommandMenuVM? CommandMenuVM { get; private set; }  
        public  BluetoothManagerVM BluetoothManagerVM { get; private set; }
        public CommandActivityVM()
        {
            BluetoothManagerVM=new BluetoothManagerVM();
            CommandMenuVM= new CommandMenuVM();
         


        }


    }
}