using Android.Content;
using AndroidX.Wear.Widget;
using Nauti_Control_Wear.ViewController;
using Nauti_Control_Wear.ViewModels;

namespace Nauti_Control_Wear;

[Activity(Label = "Main Menu")]
public class MainMenuActivity : Activity, IMainMenuVC
{
    /// <summary>
    /// Main Menu VM
    /// </summary>
    private MainMenuVM? _mainMenuVM;

    /// <summary>
    /// Remote Button
    /// </summary>
    private Button? _remoteButton;


    /// <summary>
    /// Display Button
    /// </summary>
    private Button? _displayButton;


    /// <summary>
    /// Open Command Activity
    /// </summary>
    public void OpenCommandActivity()
    {
        Intent intent = new Intent(this, typeof(CommandActivity));

        this.StartActivity(intent);
    }

    /// <summary>
    /// Open Data Display Activity
    /// </summary>
    public void OpenDataDisplayActivity()
    {
        Intent intent = new Intent(this, typeof(DataDisplayActivity));

        this.StartActivity(intent);
    }

    /// <summary>
    /// On Create
    /// </summary>
    /// <param name="savedInstanceState">saved Instance State</param>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.main_menu);
        SetupVM();
    }

    /// <summary>
    /// Setup ViewModel 
    /// </summary>
    private void SetupVM()
    {
        _mainMenuVM = new MainMenuVM(this);
        _remoteButton = FindViewById<Button>(Resource.Id.button_command);
        _remoteButton.Click += _mainMenuVM.CommandButtonClick;
        _displayButton = FindViewById<Button>(Resource.Id.button_display);
        _displayButton.Click += _mainMenuVM.DisplayButtonClick;
    }
}