using Nauti_Control_Wear.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauti_Control_Wear.ViewController
{
    public  interface IDataDisplayVC
    {
        void UpdateDataDisplay(BoatData data);
    }
}
