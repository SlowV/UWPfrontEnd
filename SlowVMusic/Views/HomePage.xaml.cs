using Newtonsoft.Json;
using SlowVMusic.Entity;
using SlowVMusic.Model;
using SlowVMusic.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
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
    public sealed partial class HomePage : Page
    {
        //private ObservableCollection<Song> listSong;
        private Song currentSong;
        //internal ObservableCollection<Song> ListSong { get => listSong; set => listSong = value; }
        private ObservableCollection<Song> ListSong = new ObservableCollection<Song>();
        private ObservableCollection<Song> mySong = new ObservableCollection<Song>();
        private static List<Song> lstSong;

        private bool isPlaying = false;

        int onPlay = 0;

        TimeSpan _position;

        DispatcherTimer _timer = new DispatcherTimer();
        public HomePage()
        {
            currentSong = new Song();
            this.InitializeComponent();
            this.VolumeSlider.Value = 100;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ticktock;
            _timer.Start();
        }

        private async void AddSong(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;

            StorageFile file = await folder.GetFileAsync("token.txt");
            var tokenContent = await FileIO.ReadTextAsync(file);

            TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(tokenContent);

            // Lay thong tin ca nhan bang token.
            HttpClient client2 = new HttpClient();
            client2.DefaultRequestHeaders.Add("Authorization", "Basic " + token.token);
            var resp = client2.GetAsync("http://2-dot-backup-server-002.appspot.com/_api/v2/members/information").Result;
            Debug.WriteLine(await resp.Content.ReadAsStringAsync());
            var userInfoContent = await resp.Content.ReadAsStringAsync();

            Member userInfoJson = JsonConvert.DeserializeObject<Member>(userInfoContent);

            currentSong.name = this.Name.Text;
            currentSong.description = Description.Text;
            currentSong.singer = Singer.Text;
            currentSong.author = Author.Text;
            currentSong.thumbnail = Thumbnail.Text;
            currentSong.link = Link.Text;
            currentSong.memberId = userInfoJson.id;

            string content = await APIHandle.Create_Song(this.currentSong);
            Song songContent = JsonConvert.DeserializeObject<Song>(content);
            mySong.Add(new Song
            {
                name = songContent.name,
                description = songContent.description,
                author = songContent.author,
                singer = songContent.singer,
                thumbnail = songContent.thumbnail,
                link = songContent.link
            });

            ListSong.Add(new Song
            {
                name = songContent.name,
                description = songContent.description,
                author = songContent.author,
                singer = songContent.singer,
                thumbnail = songContent.thumbnail,
                link = songContent.link
            });


            ProgressRing.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
            SongLoading.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            ProgressRing.IsActive = false;
            ProgressRing.Visibility = Visibility.Collapsed;
            SongLoading.Visibility = Visibility.Collapsed;
            var dialog = new Windows.UI.Popups.MessageDialog("Upload thành công");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK") { Id = 1 });
            dialog.CancelCommandIndex = 1;
            await dialog.ShowAsync();
            //reset Form Create Song
            Name.Text = "";
            Description.Text = "";
            Singer.Text = "";
            Author.Text = "";
            Thumbnail.Text = "";
            Link.Text = "";
        }

        private async Task<List<Song>> Get_List_Song()
        {

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("token.txt") != null)
            {
                HttpClient httpClient = new HttpClient();
                StorageFile file = await folder.GetFileAsync("token.txt");
                var tokenContent = await FileIO.ReadTextAsync(file);

                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(tokenContent);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + token.token);
                var response = httpClient.GetAsync(APIHandle.API_CREATE_SONG);
                var songContent = await response.Result.Content.ReadAsStringAsync();
                Debug.WriteLine(songContent);
                lstSong = JsonConvert.DeserializeObject<List<Song>>(songContent);
            }

            return lstSong;
        }

        private async Task<List<Song>> Get_My_Song()
        {
            lstSong = new List<Song>();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("token.txt") != null)
            {
                HttpClient httpClient = new HttpClient();
                StorageFile file = await folder.GetFileAsync("token.txt");
                var tokenContent = await FileIO.ReadTextAsync(file);

                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(tokenContent);
                httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + token.token);
                var response = httpClient.GetAsync(APIHandle.API_MY_SONG);
                var songContent = await response.Result.Content.ReadAsStringAsync();
                Debug.WriteLine(songContent);
                lstSong = JsonConvert.DeserializeObject<List<Song>>(songContent);
            }

            return lstSong;
        }

        private async void homePage(object sender, RoutedEventArgs e)
        {
            List<Song> i = await Get_List_Song();
            if (i != null)
            {
                foreach (var item in i)
                {
                    ListSong.Add(new Song
                    {
                        name = item.name,
                        description = item.description,
                        singer = item.singer,
                        author = item.author,
                        thumbnail = item.thumbnail,
                        link = item.link,
                    });
                }
            };
            List<Song> j = await Get_My_Song();
            if(j != null)
            {
                foreach (var item in j)
                {
                    mySong.Add(new Song
                    {
                        name = item.name,
                        description = item.description,
                        singer = item.singer,
                        author = item.author,
                        thumbnail = item.thumbnail,
                        link = item.link,
                    });
                }
            }
            if (isPlaying == false)
            {
                NameSongPlaying.Visibility = Visibility.Collapsed;
                ControlSongPlaying.Visibility = Visibility.Collapsed;
            }
            else
            {
                NameSongPlaying.Visibility = Visibility.Collapsed;
                ControlSongPlaying.Visibility = Visibility.Collapsed;
            }
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
            onPlay = MenuList.SelectedIndex;
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
            MenuList.SelectedIndex = onPlay;
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
            MenuList.SelectedIndex = onPlay;
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
