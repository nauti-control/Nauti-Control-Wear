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

        /// <summary>
        /// Update Display
        /// </summary>
        /// <param name="data">Boat Data</param>
        void UpdateDataDisplay(BoatData data);
    }
}
