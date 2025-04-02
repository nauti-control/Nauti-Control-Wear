using Android.Views;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Adapters
{
    public class CommandAdapter : RecyclerView.Adapter
    {
        private readonly List<CommandItemVM> _commandItems;

        public event EventHandler<int>? ItemClick;

        public CommandAdapter(CommandMenuVM commandMenuVM)
        {
            _commandItems = commandMenuVM.MenuItems;
        }

        public override int ItemCount => _commandItems.Count;

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder is CommandViewHolder vh)
            {
                vh.MenuItemVM = _commandItems[position];
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View? itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.command_item, parent, false);
            if (itemView == null)
            {
                throw new InvalidOperationException("Could not inflate command_item layout");
            }

            return new CommandViewHolder(itemView, OnClick);
        }
    }
}