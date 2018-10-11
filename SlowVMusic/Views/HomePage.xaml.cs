using SlowVMusic.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class HomePage : Page
    {

        private ObservableCollection<Song> listSong;

        internal ObservableCollection<Song> ListSong { get => listSong; set => listSong = value; }
        public HomePage()
        {
            this.ListSong = new ObservableCollection<Song>();
            this.ListSong.Add(new Song {
                name = "La tu em da tinh",
                thumbnail = "https://avatar-nct.nixcdn.com/playlist/2013/12/04/a/c/b/4/1386151059014_500.jpg"
            });
            this.ListSong.Add(new Song
            {
                name = "Con duong binh pham",
                thumbnail = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTD8NBZoVBDI2M2JPQIIrHeoV7kYwlkMTjjIBnSlnVDOSR9HtSf"
            });
            this.InitializeComponent();
        }
    }
}
