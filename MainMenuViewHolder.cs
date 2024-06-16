using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;

namespace Nauti_Control_Wear
{
    public class MainMenuViewHolder : RecyclerView.ViewHolder
    {
        public TextView MenuItem { get; private set; }   
        
           
        public MainMenuViewHolder(View itemView) : base(itemView)
        {

            
            MenuItem = itemView.FindViewById<TextView>(Resource.Id.menu_text);
        }
    }
}

