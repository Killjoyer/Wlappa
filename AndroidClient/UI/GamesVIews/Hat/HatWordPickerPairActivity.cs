using System;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidClient.ViewModels.GameViewModels;
using Unity;

namespace AndroidClient.UI.GamesVIews.Hat
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = false)]
    public class HatWordPickerPairActivity : AppCompatActivity
    {
        private bool _canceledTimer;
        private HatViewModel _viewModel = null!;
        private Button _guessedBtn = null!;
        private Button _cancelBtn = null!;
        private TextView _scores = null!;
        private TextView _gameTimer = null!;
        private TextView _wordTextView = null!;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.hat_word_picker_pair);
            _viewModel = (Application as App)!.Container.Resolve<HatViewModel>();

            _wordTextView = FindViewById<TextView>(Resource.Id.word_to_guess)!;
            _scores = FindViewById<TextView>(Resource.Id.in_game_scores)!;
            _scores!.Text = _viewModel.LastScoresConcatenated;
            _guessedBtn = FindViewById<Button>(Resource.Id.guess_btn)!;
            _cancelBtn = FindViewById<Button>(Resource.Id.cancel_btn)!;
            _gameTimer = FindViewById<TextView>(Resource.Id.game_timer)!;
            _wordTextView!.Text = _viewModel.CurrentWord;

            if (!_viewModel.AmControllingExplanation)
            {
                _guessedBtn.Visibility = ViewStates.Gone;
                _cancelBtn.Visibility = ViewStates.Gone;
            }

            _guessedBtn.Click += async (sender, args) => await _viewModel.GuessWord();
            _cancelBtn.Click += async (sender, args) => await _viewModel.CancelExplanation();
        }

        private void OnViewModelGetWord()
        {
            RunOnUiThread(() => _wordTextView.Text = _viewModel.CurrentWord);
        }

        private void OnViewModelTimeIsUp() =>
            RunOnUiThread(() =>
            {
                _guessedBtn.Visibility = ViewStates.Gone;
                _cancelBtn.Visibility = ViewStates.Gone;
                Snackbar.Make(_guessedBtn, "Time is up", 2000);
            });

        private void OnViewModelAnnouncedNextPair()
        {
            StartActivity(typeof(HatPairChosenActivity));
            Finish();
        }

        private void OnViewModelScoresUpdated()
        {
            Log.Info(nameof(HatWordPickerPairActivity), "Should update scores");
            RunOnUiThread(() => _scores.Text = _viewModel.LastScoresConcatenated);
        }

        private void OnViewModelGameOver()
        {
            EndHatGameActivity.Launch(_viewModel.LastScoresConcatenated, StartActivity, this);
            Finish();
        }

        private async void ExecuteEvery(TimeSpan duration, Action callback)
        {
            while (!_canceledTimer)
            {
                await Task.Delay(duration);
                callback();
            }

            _canceledTimer = false;
        }

        protected override void OnStart()
        {
            Log.Info(nameof(HatWordPickerPairActivity), "Start");
            base.OnStart();
            _viewModel.GetWord += OnViewModelGetWord;
            _viewModel.TimeIsUp += OnViewModelTimeIsUp;
            _viewModel.AnnouncedNextPair += OnViewModelAnnouncedNextPair;
            _viewModel.ScoresUpdated += OnViewModelScoresUpdated;
            _viewModel.GameOver += OnViewModelGameOver;
            ExecuteEvery(TimeSpan.FromMilliseconds(100),
                () => RunOnUiThread(() => _gameTimer.Text = _viewModel.TimerString));
        }

        protected override void OnStop()
        {
            Log.Info(nameof(HatWordPickerPairActivity), "Stop");
            base.OnStop();
            _viewModel.GetWord -= OnViewModelGetWord;
            _viewModel.TimeIsUp -= OnViewModelTimeIsUp;
            _viewModel.AnnouncedNextPair -= OnViewModelAnnouncedNextPair;
            _viewModel.ScoresUpdated -= OnViewModelScoresUpdated;
            _viewModel.GameOver -= OnViewModelGameOver;
            _canceledTimer = true;
        }
    }
}