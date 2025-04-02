using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Nauti_Control_Wear.Views;

namespace Nauti_Control_Wear.Adapters;

public class GaugePagerAdapter : RecyclerView.Adapter
{
    private readonly Context _context;
    private const int PAGE_COUNT = 4; // Wind, Depth, Speed, Compass

    public GaugePagerAdapter(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public override int ItemCount => PAGE_COUNT;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        if (parent.Context == null) throw new InvalidOperationException("Parent context is null");

        View view;
        switch (viewType)
        {
            case 0: // Wind gauge
                view = new WindGaugeView(_context);
                break;
            case 1: // Depth gauge
                view = new DepthGaugeView(_context);
                break;
            case 2: // Speed gauge
                view = new SpeedGaugeView(_context);
                break;
            case 3: // Compass gauge
                view = new CompassGaugeView(_context);
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
        // No binding needed for gauge views
    }

    public override int GetItemViewType(int position)
    {
        return position;
    }

    public BaseGaugeView? GetGauge(int position)
    {
        return position switch
        {
            0 => new WindGaugeView(_context),
            1 => new DepthGaugeView(_context),
            2 => new SpeedGaugeView(_context),
            3 => new CompassGaugeView(_context),
            _ => null
        };
    }

    private class ViewHolder : RecyclerView.ViewHolder
    {
        public ViewHolder(View itemView) : base(itemView)
        {
        }
    }
} 