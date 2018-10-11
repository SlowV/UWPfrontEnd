using Newtonsoft.Json;
using SlowVMusic.Entity;
using SlowVMusic.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using MUXC = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlowVMusic
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Member currentMember;
        private static StorageFile file;
        private static string UploadUrl;

        public MainPage()
        {
            GetUploadUrl();
            this.currentMember = new Member();
            this.InitializeComponent();
            
        }



        ContentDialog dialogLogin = new ContentDialog();
        ContentDialog dialogRegister = new ContentDialog();

        //Dialog login
        private async void PageLogin(object sender, RoutedEventArgs e)
        {
            var stackPanel = new StackPanel();

            // Tạo input tài khoản 
            var textBoxName = new TextBox
            {
                Header = "Email",
                Name = "Email",
                Margin = new Thickness(0, 5, 0, 10),
            };

            // Tạo input pass 
            var textBoxPass = new PasswordBox
            {
                Header = "Mật khẩu",
                Name = "Password",
            };

            // tạo mới link sang page đăng ký
            var linkSignUp = new HyperlinkButton
            {
                Content = "Đăng ký",
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
            // add sự kiện Click vào hyperlink
            linkSignUp.Click += LinkSignUp_Click;

            stackPanel.Children.Add(textBoxName);
            stackPanel.Children.Add(textBoxPass);
            stackPanel.Children.Add(linkSignUp);

            Style btnPrimaryStyle = (Style)App.Current.Resources["myButtonPrimary"];
            Style btnCloseStyle = (Style)App.Current.Resources["myButtonClose"];
            dialogLogin.Title = "Đăng nhập";
            dialogLogin.Content = stackPanel;
            dialogLogin.PrimaryButtonText = "Đăng nhập";
            dialogLogin.CloseButtonText = "Đóng";
            dialogLogin.PrimaryButtonStyle = btnPrimaryStyle;
            dialogLogin.CloseButtonStyle = btnCloseStyle;

            dialogLogin.PrimaryButtonClick += async (s, args) =>
            {
                ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                //await Task.Delay(3000);  //Here I just wait 3 seconds

                // Xu ly nut dang nhap ben trong nay.
                Debug.WriteLine("SlowV");
                deferral.Complete();
            };
            await dialogLogin.ShowAsync();
        }



        //Dialog Register
        Image avaterRegister = new Image();
        TextBox url_ImageRegister = new TextBox();
        private async void LinkSignUp_Click(object sender, RoutedEventArgs e)
        {
            var stackPanel = new StackPanel();
            var sv = new ScrollViewer()
            {
                Content = stackPanel
            };


            // Tạo input tài khoản. 
            var textBoxName = new TextBox
            {
                Header = "Email",
                Name = "Email",
                Margin = new Thickness(50, 5, 50, 10),
                PlaceholderText = "Email của bạn.",
            };

            // Tạo input pass. 
            var textBoxPass = new PasswordBox
            {
                Header = "Mật khẩu",
                Name = "Password",
                Margin = new Thickness(50, 5, 50, 10),
                PlaceholderText = "Mật khẩu của bạn.",
            };

            // Tạo mới link sang page đăng nhập
            var linkSignIn = new HyperlinkButton
            {
                Content = "Quay lại đăng nhập",
                Margin = new Thickness(50, 5, 50, 10),
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 13,
            };
            // Sự kiện click của hyperLink.
            linkSignIn.Click += LinkSignIn_Click;


            // Tạo input firstName. 
            var firstName = new TextBox
            {
                Header = "Họ",
                Name = "FirstName",
                PlaceholderText = "Họ của bạn.",
                Margin = new Thickness(50, 5, 50, 10),
            };

            // Tạo input lastName. 
            var lastName = new TextBox
            {
                Header = "Tên",
                Name = "LastName",
                PlaceholderText = "Tên của bạn.",
                Margin = new Thickness(50, 5, 50, 10),
            };

            //input image Url.

            url_ImageRegister.Header = "Url Ảnh";
            url_ImageRegister.Name = "ImageUrl";
            url_ImageRegister.PlaceholderText = "Url Ảnh";
            url_ImageRegister.Margin = new Thickness(50, 5, 50, 10);


            var stackPanelAvatar = new StackPanel();
            stackPanelAvatar.Orientation = Orientation.Horizontal;

            //thẻ Image để nhét Avatar khi chụp ảnh.
            avaterRegister.Name = "MyAvatar";
            avaterRegister.Width = 120;
            avaterRegister.Height = 120;
            avaterRegister.Margin = new Thickness(60,0,0,0);
            avaterRegister.HorizontalAlignment = HorizontalAlignment.Right;
            avaterRegister.VerticalAlignment = VerticalAlignment.Center;
            avaterRegister.Source = new BitmapImage(new Uri(@"http://stc.zuni.vn/resource/module/default/layout/image/default_avatar.png"));

            // nút Capture_photo chụp ảnh
            var btn_CapturePhoto = new Button
            {
                Content = "Chụp ảnh",
                Style = (Style)App.Current.Resources["ButtonPink"],
                Margin = new Thickness(50, 5, 50, 10),
                Height = 34,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            btn_CapturePhoto.Click += Capture_PhotoAsync;

            stackPanelAvatar.Children.Add(btn_CapturePhoto);
            stackPanelAvatar.Children.Add(avaterRegister);

            var phone = new TextBox
            {
                Header = "Số điện thoại",
                Margin = new Thickness(50, 5, 50, 10),
                Name = "Phone",
                AcceptsReturn = true,
                PlaceholderText = "Số điện thoại của bạn."
            };

            var address = new TextBox
            {
                Header = "Địa chỉ",
                Margin = new Thickness(50, 5, 50, 10),
                Name = "Address",
                AcceptsReturn = true,
                PlaceholderText = "Địa chỉ của bạn."
            };

            var introduction = new TextBox
            {
                Header = "Giới thiệu",
                Margin = new Thickness(50, 5, 50, 10),
                Name = "Introduction",
                Height = 100,
                AcceptsReturn = true,
                PlaceholderText = "Giới thiệu chút ít về bản thân."
            };

            // UI Giới tính
            var textBlockGender = new TextBlock
            {
                Text = "Giới tính",
                Margin = new Thickness(50, 5, 50, 10),
            };

            var stackPanelGender = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(50, 5, 50, 10),
            };

            var Male = new RadioButton
            {
                Content = "Nam",
                Tag = 1,
            };

            var Female = new RadioButton
            {
                Content = "Nữ",
                Tag = 2,
            };

            var Other = new RadioButton
            {
                Content = "Khác",
                Tag = 3,
                IsChecked = true
            };

            Female.Checked += Select_Gender;
            Male.Checked += Select_Gender;
            Other.Checked += Select_Gender;

            stackPanelGender.Children.Add(Male);
            stackPanelGender.Children.Add(Female);
            stackPanelGender.Children.Add(Other);
            //End Ui giới tính.

            var birthDay = new CalendarDatePicker
            {
                Name = "BirthDay",
                Header = "Ngày sinh",
                Margin = new Thickness(50, 5, 50, 10),
            };
            birthDay.DateChanged += BirthDay_DateChanged;


            // add giao diện vào layout.
            stackPanel.Children.Add(textBoxName);
            stackPanel.Children.Add(textBoxPass);
            stackPanel.Children.Add(firstName);
            stackPanel.Children.Add(lastName);
            stackPanel.Children.Add(url_ImageRegister);
            stackPanel.Children.Add(stackPanelAvatar);
            stackPanel.Children.Add(phone);
            stackPanel.Children.Add(address);
            stackPanel.Children.Add(textBlockGender);
            stackPanel.Children.Add(stackPanelGender);
            stackPanel.Children.Add(birthDay);
            stackPanel.Children.Add(introduction);
            stackPanel.Children.Add(linkSignIn);

            stackPanel.MinWidth = Window.Current.Bounds.Width - 600;
            stackPanel.MaxWidth = Window.Current.Bounds.Height;
            dialogRegister.MinWidth = Window.Current.Bounds.Width;
            dialogRegister.MaxWidth = Window.Current.Bounds.Height;

            Style btnPrimaryStyle = (Style)App.Current.Resources["myButtonPrimary"];
            Style btnCloseStyle = (Style)App.Current.Resources["myButtonClose"];
            dialogRegister.Title = "Đăng ký";
            dialogRegister.Content = sv;
            dialogRegister.CloseButtonText = "Đóng";
            dialogRegister.PrimaryButtonText = "Đăng ký";
            dialogRegister.PrimaryButtonStyle = btnPrimaryStyle;
            dialogRegister.CloseButtonStyle = btnCloseStyle;
            // Ẩn dialog SignIn.
            dialogLogin.Hide();
            dialogRegister.PrimaryButtonClick += async (s, args) =>
            {
                ContentDialogButtonClickDeferral deferral = args.GetDeferral();

                this.currentMember.email = textBoxName.Text;
                this.currentMember.password = textBoxPass.Password;
                this.currentMember.introduction = introduction.Text;
                this.currentMember.firstName = firstName.Text;
                this.currentMember.lastName = lastName.Text;
                this.currentMember.avatar = url_ImageRegister.Text;
                this.currentMember.phone = phone.Text;
                this.currentMember.address = address.Text;

                var httpResponseMessage = APIHandle.Sign_Up(this.currentMember);
                if (httpResponseMessage.Result.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Debug.WriteLine("Success");
                }
                else
                {
                    var errorJson = await httpResponseMessage.Result.Content.ReadAsStringAsync();
                    ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorJson);
                    foreach (var errorField in errResponse.error.Keys)
                    {
                        TextBlock textBlock = this.FindName(errorField) as TextBlock;
                        textBlock.Text = errResponse.error[errorField];
                    }
                }
                await Task.Delay(3000);  //Here I just wait 3 seconds
                // Xu ly nut dang nhap ben trong nay.
                deferral.Complete();
            };

            await dialogRegister.ShowAsync();
        }

        private void BirthDay_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.currentMember.birthday = sender.Date.Value.ToString("yyyy-MM-dd");
        }

        private void Select_Gender(object sender, RoutedEventArgs e)
        {
            RadioButton radioGender = sender as RadioButton;
            this.currentMember.gender = Int32.Parse(radioGender.Tag.ToString());
            Debug.WriteLine(Int32.Parse(radioGender.Tag.ToString()));
        }

        private async void Capture_PhotoAsync(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            file = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file == null)
            {
                // User cancelled photo capture
                return;
            }
            HttpUploadFile(UploadUrl, "myFile", "image/png");
        }

        private static async void GetUploadUrl()
        {
            HttpClient httpClient = new HttpClient();
            Uri requestUri = new Uri("https://2-dot-backup-server-002.appspot.com/get-upload-token");
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            Debug.WriteLine(httpResponseBody);
            UploadUrl = httpResponseBody;
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            Debug.WriteLine(url);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await file.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //Debug.WriteLine(string.Format("File uploaded, server response is: @{0}@", reader2.ReadToEnd()));
                //string imgUrl = reader2.ReadToEnd();
                Uri u = new Uri(reader2.ReadToEnd(), UriKind.Absolute);
                Debug.WriteLine(u.AbsoluteUri);
                url_ImageRegister.Text = u.AbsoluteUri;
                avaterRegister.Source = new BitmapImage(u);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }


        //Show Dialog Đăng ký.
        private async void LinkSignIn_Click(object sender, RoutedEventArgs e)
        {
            dialogRegister.Hide();
            await dialogLogin.ShowAsync();
        }

        private void nvTopLevelNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {

        }

        private void nvTopLevelNav_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            TextBlock ItemContent = args.InvokedItem as TextBlock;
            if (ItemContent != null)
            {
                switch (ItemContent.Tag)
                {
                    case "Nav_Home":
                        contentFrame.Navigate(typeof(Views.HomePage), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "Nav_Shop":
                        contentFrame.Navigate(typeof(Views.PageLogin), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "Nav_ShopCart":
                        contentFrame.Navigate(typeof(Views.PageLogin), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "Nav_Message":
                        contentFrame.Navigate(typeof(Views.PageLogin), null, new EntranceNavigationTransitionInfo());
                        break;

                    case "Nav_Print":
                        contentFrame.Navigate(typeof(Views.PageLogin), null, new EntranceNavigationTransitionInfo());
                        break;
                }
            }
        }

        private void nvTopLevelNav_Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem
            foreach (NavigationViewItemBase item in nvTopLevelNav.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "Home_Page")
                {
                    nvTopLevelNav.SelectedItem = item;
                    break;
                }
            }
            contentFrame.Navigate(typeof(Views.HomePage));
        }
    }
}
