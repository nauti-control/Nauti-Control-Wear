using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Nauti_Control_Wear.Adapters
{
    public class MainMenuViewHolder : RecyclerView.ViewHolder
    {
        public TextView MenuItem { get; private set; }


        public MainMenuViewHolder(View itemView) : base(itemView)
        {


            MenuItem = itemView.FindViewById<TextView>(_Microsoft.Android.Resource.Designer.ResourceConstant.Id.menu_text);
        }
    }
}

