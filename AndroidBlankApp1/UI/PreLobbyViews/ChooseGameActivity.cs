using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using AndroidBlankApp1.UI.PreLobbyViews.GameConfigurators;
using AndroidBlankApp1.ViewModels;
using Unity;

namespace AndroidBlankApp1.UI.PreLobbyViews
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class ChooseGameActivity : AppCompatActivity
    {
        private PreLobbyViewModel _viewModel = null!;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.choose_game);
            var app = Application as App;
            _viewModel = app!.Container.Resolve<PreLobbyViewModel>();

            FindViewById<Button>(Resource.Id.next_to_configure_game_btn)!.Click +=
                (sender, args) => _viewModel.HandleGameSelected();
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.ShouldConfigureGame = ViewModelShouldConfigureGame;
            _viewModel.ShowNotification += ShowNotification;
        }
        
        private void ShowNotification(string text) =>
            Toast.MakeText(ApplicationContext, text, ToastLength.Long)?.Show();

        private void ViewModelShouldConfigureGame()
        {
            var radioGroup = FindViewById<RadioGroup>(Resource.Id.game_choosing_group);
            var selected = radioGroup?.CheckedRadioButtonId;
            Log.Info(nameof(ChooseGameActivity), selected.ToString());
            switch (selected)
            {
                case Resource.Id.game_choice_hat:
                    StartActivity(typeof(HatConfigurationActivity));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.ShouldConfigureGame = null;
            _viewModel.ShowNotification -= ShowNotification;
        }
    }
}