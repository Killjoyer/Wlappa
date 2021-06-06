using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidBlankApp1.ViewModels.GameViewModels;
using Unity;

namespace AndroidBlankApp1.UI.GamesViews.Hat
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatWordsChooserActivity : AppCompatActivity
    {
        private HatViewModel _viewModel;
        private Button? _addWordsButton;
        private EditText? _wordsTextInput;
        private TextView? _numberOfPlayersRemaining;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_words_chooser);
            _viewModel = (Application as App)!.Container.Resolve<HatViewModel>();

            _addWordsButton = FindViewById<Button>(Resource.Id.start_choose_pairs_btn);
            _wordsTextInput = FindViewById<EditText>(Resource.Id.words_input);
            _numberOfPlayersRemaining = FindViewById<TextView>(Resource.Id.number_of_players_ready);

            if (_viewModel.MyRole != Shared.Protos.HatSharedClasses.HatRolePlayer.Value)
            {
                _addWordsButton!.Visibility = ViewStates.Gone;
                _wordsTextInput!.Visibility = ViewStates.Gone;
            }

            _addWordsButton!.Click +=
                async (sender, args) => await _viewModel.SendWords(sender as View);


            _numberOfPlayersRemaining!.Text = _viewModel.RemainingPlayersToWriteWords.ToString();

            _wordsTextInput!.TextChanged += (sender, args) =>
                _viewModel.WordsInput = string.Concat(args.Text!);
        }

        private void OnViewModelWordsSuccessfullyAddedByMe()
        {
            RunOnUiThread(() =>
            {
                _addWordsButton.Visibility = ViewStates.Gone;
                _wordsTextInput!.Enabled = false;
            });
        }

        private void OnViewModelWordsSuccessfullyAddedBySomeOne()
        {
            RunOnUiThread(() => _numberOfPlayersRemaining!.Text = _viewModel.RemainingPlayersToWriteWords.ToString());
        }

        private void OnViewModelAnnouncedNextPair()
        {
            StartActivity(typeof(HatPairChoosenActivity));
            Finish();
        }

        private void OnViewModelInvalidWordSet()
        {
            RunOnUiThread(() =>
            {
                _addWordsButton!.Visibility = ViewStates.Visible;
                _wordsTextInput!.Enabled = true;
                Snackbar.Make(_addWordsButton, "Invalid word set", 2000).Show();
                // Toast.MakeText(_addWordsButton.Context, "Invalid word set", ToastLength.Short)?.Show();
            });
        }

        protected override void OnStart()
        {
            base.OnStart();
            _viewModel.WordsSuccessfullyAddedBySomeOne += OnViewModelWordsSuccessfullyAddedBySomeOne;
            _viewModel.AnnouncedNextPair += OnViewModelAnnouncedNextPair;
            _viewModel.InvalidWordSet += OnViewModelInvalidWordSet;
            _viewModel.WordsSuccessfullyAddedByMe += OnViewModelWordsSuccessfullyAddedByMe;
        }

        protected override void OnStop()
        {
            base.OnStop();
            _viewModel.WordsSuccessfullyAddedBySomeOne -= OnViewModelWordsSuccessfullyAddedBySomeOne;
            _viewModel.AnnouncedNextPair -= OnViewModelAnnouncedNextPair;
            _viewModel.InvalidWordSet -= OnViewModelInvalidWordSet;
            _viewModel.WordsSuccessfullyAddedByMe -= OnViewModelWordsSuccessfullyAddedByMe;
        }
    }
}