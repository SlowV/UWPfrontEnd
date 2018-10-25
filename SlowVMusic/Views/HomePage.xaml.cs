using Microsoft.Data.Sqlite;
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
using Windows.Media.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
        private ObservableCollection<Song> testSong = new ObservableCollection<Song>();
        private static List<Song> lstSong;


        DispatcherTimer _timer = new DispatcherTimer();
        public HomePage()
        {
            currentSong = new Song();
            this.InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ChangeUI;
            _timer.Start();

            List<Song> i = Get_Song_InDB();

            if (i != null)
            {
                foreach (var item in i)
                {
                    testSong.Add(new Song
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


        }

        private void ChangeUI(object sender, object e)
        {
            if (GlobalFlySong._isLogin)
            {
                itemUploadSong.Visibility = Visibility.Visible;
            }
            else
            {
                itemUploadSong.Visibility = Visibility.Collapsed;
            }
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
            Debug.WriteLine(content);
            // đến đây là đã có mp3 đã lên rồi.
            //if (content.Result.StatusCode == System.Net.HttpStatusCode.Created)
            //{
            //    Debug.WriteLine("Success");
            //}
            //else
            //{
            //    var errorJson = await httpResponseMessage.Result.Content.ReadAsStringAsync();
            //    ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorJson);
            //    foreach (var errorField in errResponse.error.Keys)
            //    {
            //        if (emailValid.Name == errorField)
            //        {
            //            emailValid.Text = errResponse.error[errorField];
            //            emailValid.Visibility = Visibility.Visible;
            //        }
            //        else if (passValid.Name == errorField)
            //        {
            //            passValid.Text = errResponse.error[errorField];
            //            passValid.Visibility = Visibility.Visible;
            //        }
            //        else if (firstnameValid.Name == errorField)
            //        {
            //            firstnameValid.Text = errResponse.error[errorField];
            //            firstnameValid.Visibility = Visibility.Visible;
            //        }
            //        else if (lastName.Name == errorField)
            //        {
            //            lastName.Text = errResponse.error[errorField];
            //            lastName.Visibility = Visibility.Visible;
            //        }
            //        else if (addressValid.Name == errorField)
            //        {
            //            addressValid.Text = errResponse.error[errorField];
            //            addressValid.Visibility = Visibility.Visible;
            //        }
            //        else if (phoneValid.Name == errorField)
            //        {
            //            phoneValid.Text = errResponse.error[errorField];
            //            phoneValid.Visibility = Visibility.Visible;
            //        }

            //    }
            //}
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
            if (j != null)
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //StackPanel panel = sender as StackPanel;
            //Song selectedSong = panel.Tag as Song;
            //Debug.WriteLine("Đã tap vô nghe nhạc");
            //onPlay = MenuList.SelectedIndex;
            //LoadSong(selectedSong);
            //PlaySong();
            // Đi thẳng vào MainPage và thích móc gì thì móc


            var stackObject = sender as StackPanel;
            Song dataSong = stackObject.Tag as Song;

            var frame = Window.Current.Content as Frame;
            var currentPage = frame.Content as Page;
            var stackPanel = currentPage.FindName("PanelMedia");
            var imageFindName = currentPage.FindName("imageSong");
            var titleSongFindName = currentPage.FindName("titleSong");
            var myMediaFindName = currentPage.FindName("MyMedia");
            var myMedia = myMediaFindName as MediaPlayerElement;

            myMedia.Source = MediaSource.CreateFromUri(new Uri(dataSong.link));
            myMedia.AutoPlay = true;
            var stack = stackPanel as StackPanel;
            var imageBrush = imageFindName as ImageBrush;
            var titleSongTextBlock = titleSongFindName as TextBlock;
            titleSongTextBlock.Text = dataSong.name;
            
            if (stack != null)
            {
                stack.Visibility = Visibility.Visible;
            }
            imageBrush.ImageSource = new BitmapImage(new Uri(dataSong.thumbnail));

        }
        private static List<Song> Get_Song_InDB()
        {
            lstSong = new List<Song>();
            using (SqliteConnection db =
                new SqliteConnection("Filename=slowvmusic.db"))
            {
                db.Open();

                SqliteCommand selectCommand = new SqliteCommand
                    ("SELECT * from Song", db);

                SqliteDataReader query = selectCommand.ExecuteReader();

                while (query.Read())
                {
                    int id = query.GetInt32(0);
                    String name = query.GetString(1);
                    String description = query.GetString(2);
                    String singer = query.GetString(3);
                    String author = query.GetString(4);
                    String thumbnail = query.GetString(5);
                    String link = query.GetString(6);

                    lstSong.Add(new Song
                    {
                        id = id,
                        name = name,
                        description = description,
                        singer = singer,
                        author = author,
                        thumbnail = thumbnail,
                        link = link
                    });
                }

                db.Close();
            }
            return lstSong;
        }

    }

}
