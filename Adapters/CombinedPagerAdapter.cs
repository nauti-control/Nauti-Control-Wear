using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear;
using Nauti_Control_Wear.Views;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear.Adapters;

public class CombinedPagerAdapter : RecyclerView.Adapter
{
    private readonly CombinedControlActivity _activity;
    private const int PAGE_COUNT = 5; // Command view + 4 gauges

    public CombinedPagerAdapter(CombinedControlActivity activity)
    {
        _activity = activity ?? throw new ArgumentNullException(nameof(activity));
    }

    public override int ItemCount => PAGE_COUNT;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var context = parent.Context ?? throw new InvalidOperationException("Parent context is null");

        View view;
        switch (viewType)
        {
            case 0: // Command view
                var inflater = LayoutInflater.From(context) ?? throw new InvalidOperationException("Could not get LayoutInflater");
                view = inflater.Inflate(Resource.Layout.command_content, parent, false) ?? throw new InvalidOperationException("Failed to inflate command_content");
                break;
            case 1: // Wind gauge
                view = new WindGaugeView(_activity);
                break;
            case 2: // Depth gauge
                view = new DepthGaugeView(_activity);
                break;
            case 3: // Speed gauge
                view = new SpeedGaugeView(_activity);
                break;
            case 4: // Compass gauge
                view = new CompassGaugeView(_activity);
                break;
            default:
                throw new ArgumentException($"Invalid view type: {viewType}");
        }

        view.LayoutParameters = new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.MatchParent,
            ViewGroup.LayoutParams.MatchParent);

        return new ViewHolder(view);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        if (holder?.ItemView == null) return;
        
        var view = holder.ItemView;
        
        // If this is the command view (position 0), set up the command list
        if (position == 0 && view is ViewGroup commandView)
        {
            var recyclerView = commandView.FindViewById<WearableRecyclerView>(Resource.Id.command_menu);
            if (recyclerView != null)
            {
                recyclerView.EdgeItemsCenteringEnabled = true;
                recyclerView.SetLayoutManager(new WearableLinearLayoutManager(_activity));
                
                var commandVM = new CommandActivityVM();
                if (commandVM.CommandMenuVM != null)
                {
                    var adapter = new CommandAdapter(commandVM.CommandMenuVM);
                    adapter.ItemClick += commandVM.CommandMenuVM.OnItemClick;
                    recyclerView.SetAdapter(adapter);
                }
            }
        }
    }

    public override int GetItemViewType(int position)
    {
        return position;
    }

    public View? GetCommandView()
    {
        var inflater = LayoutInflater.From(_activity) ?? throw new InvalidOperationException("Could not get LayoutInflater");
        return inflater.Inflate(Resource.Layout.command_content, null);
    }

    public View? GetDataDisplayView()
    {
        var inflater = LayoutInflater.From(_activity) ?? throw new InvalidOperationException("Could not get LayoutInflater");
        return inflater.Inflate(Resource.Layout.data_display_content, null);
    }

    private class ViewHolder : RecyclerView.ViewHolder
    {
        public ViewHolder(View itemView) : base(itemView)
        {
        }
    }
} 