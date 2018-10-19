using SlowVMusic.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace SlowVMusic.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        private ObservableCollection<Song> ListSong = new ObservableCollection<Song>();

        private bool isPlaying = false;

        int onPlay = 0;

        TimeSpan _position;

        DispatcherTimer _timer = new DispatcherTimer();
        public SearchPage()
        {
            this.InitializeComponent();
            this.VolumeSlider.Value = 100;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ticktock;
            _timer.Start();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<Song> p = e.Parameter as List<Song>;
            if (p != null)
            {
                foreach (var item in p)
                {
                    ListSong.Add(new Song
                    {
                        id = item.id,
                        name = item.name,
                        description = item.description,
                        singer = item.singer,
                        author = item.author,
                        thumbnail = item.thumbnail,
                        link = item.link
                    });
                }
            }

            CountSearch.Text = "Tìm thấy: " + ListSong.Count + " bài hát.";
        }
        private void ticktock(object sender, object e)
        {
            MinDuration.Text = MediaPlayer.Position.Minutes + ":" + MediaPlayer.Position.Seconds;
            Progress.Minimum = 0;
            Progress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            MaxDuration.Text = MediaPlayer.NaturalDuration.TimeSpan.Minutes + ":" + MediaPlayer.NaturalDuration.TimeSpan.Seconds;
            Progress.Value = MediaPlayer.Position.TotalSeconds;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            Song selectedSong = panel.Tag as Song;
            Debug.WriteLine(ListSong[0].name);
            onPlay = listViewSearch.SelectedIndex;
            LoadSong(selectedSong);
            PlaySong();
        }
        private void PlaySong()
        {
            MediaPlayer.Play();
            PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            isPlaying = true;
        }
        private void PauseSong()
        {
            MediaPlayer.Pause();
            PlayButton.Icon = new SymbolIcon(Symbol.Play);
            isPlaying = false;
        }
        private void PlayClick(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                PauseSong();
            }
            else
            {
                PlaySong();
            }
        }
        private void PlayBack(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            if (onPlay > 0)
            {
                onPlay = onPlay - 1;
            }
            else
            {
                onPlay = ListSong.Count - 1;
            }
            LoadSong(ListSong[onPlay]);
            PlaySong();
            listViewSearch.SelectedIndex = onPlay;
        }

        private void PlayNext(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            if (onPlay < ListSong.Count - 1)
            {
                onPlay = onPlay + 1;
            }
            else
            {
                onPlay = 0;
            }
            LoadSong(ListSong[onPlay]);
            PlaySong();
            listViewSearch.SelectedIndex = onPlay;
        }
        private void LoadSong(Entity.Song currentSong)
        {
            this.NowPlaying.Text = "Loading";
            MediaPlayer.Source = new Uri(currentSong.link);
            Debug.WriteLine(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            this.NowPlaying.Text = currentSong.name + " - " + currentSong.singer;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Progress.Minimum = 0;
                Progress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Progress.Value = MediaPlayer.Position.TotalSeconds;

            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider vol = sender as Slider;
            if (vol != null)
            {
                MediaPlayer.Volume = vol.Value / 100;
                this.volume.Text = vol.Value.ToString();
            }
        }
    }
}
