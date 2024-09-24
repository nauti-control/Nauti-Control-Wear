using Nauti_Control_Wear.ViewController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nauti_Control_Wear.ViewModels
{
    public class MainMenuVM
    {
        /// <summary>
        /// Main menu view controller
        /// </summary>
        private IMainMenuVC _mainMenuVC;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainMenuVC"></param>
        public MainMenuVM(IMainMenuVC mainMenuVC)
        {
            _mainMenuVC= mainMenuVC;
        }


        /// <summary>
        /// Command Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CommandButtonClick(object? sender, EventArgs e)
        {
            _mainMenuVC.OpenCommandActivity();
        }

        /// <summary>
        /// Display Button Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DisplayButtonClick(object? sender, EventArgs e)
        {
            _mainMenuVC.OpenDataDisplayActivity();
        }



    }
}
