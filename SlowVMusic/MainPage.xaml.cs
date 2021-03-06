﻿using Newtonsoft.Json;
using SlowVMusic.Entity;
using SlowVMusic.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using Microsoft.Data.Sqlite;
using TextBlock = Windows.UI.Xaml.Controls.TextBlock;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SlowVMusic
{
     
    public sealed partial class MainPage : Page
    {
        private static Member currentLogin;
        private Member currentMember;
        private static StorageFile file;
        private static string UploadUrl;
        public static bool isLogin = false;
        private static List<Song> lstSong;

        public MainPage()
        {
            GetUploadUrl();
            this.currentMember = new Member();
            this.InitializeComponent();
            DispatcherTimer _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ChangeUI;
            _timer.Start();
        }

        private void ChangeUI(object sender, object e)
        {
            if (GlobalFlySong._isLogin)
            {
                ShowUserInfo.Visibility = Visibility.Visible;
                ShowLoginButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowUserInfo.Visibility = Visibility.Collapsed;
                ShowLoginButton.Visibility = Visibility.Visible;
            }
        }

        ContentDialog dialogLogin = new ContentDialog();
        ContentDialog dialogRegister;

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
                currentLogin = new Member();
                ContentDialogButtonClickDeferral deferral = args.GetDeferral();

                var httpResponseMessage = APIHandle.Sign_In(textBoxName.Text, textBoxPass.Password).Result;

                var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    // save file...
                    Debug.WriteLine(responseContent);
                    // Doc token
                    TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(responseContent);

                    // Luu token
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.CreateFileAsync("token.txt", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, responseContent);

                    await DoLogin();
                    //Đi thẳng vào MainPage và thích móc gì thì móc
                    var frame = Window.Current.Content as Frame;
                    var currentPage = frame.Content as Page;
                    var appbarlogin = currentPage.FindName("ShowLoginButton");
                    var appbarinfo = currentPage.FindName("ShowUserInfo");
                    AppBarButton app1 = appbarlogin as AppBarButton;
                    AppBarButton app2 = appbarinfo as AppBarButton;
                    app1.Visibility = Visibility.Collapsed;
                    app2.Visibility = Visibility.Visible;

                    GlobalFlySong._isLogin = true;
                }
                else
                {
                    // Xu ly loi.
                    ErrorResponse errorObject = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                    //if (errorObject != null && errorObject.error.Count > 0)
                    //{
                    //    foreach (var key in errorObject.error.Keys)
                    //    {
                    //        var textMessage = FindName(key);
                    //        if (textMessage == null)
                    //        {
                    //            continue;
                    //        }
                    //        TextBlock textBlock = textMessage as TextBlock;
                    //        textBlock.Text = errorObject.error[key];
                    //        textBlock.Visibility = Visibility.Visible;
                    //    }
                    //}
                    Debug.WriteLine(errorObject);
                }


                deferral.Complete();
            };
            await dialogLogin.ShowAsync();
        }


        //Dialog Register
        Image avaterRegister;
        TextBox url_ImageRegister;

        internal static Member CurrentLogin { get => currentLogin; set => currentLogin = value; }

        private async void LinkSignUp_Click(object sender, RoutedEventArgs e)
        {
            dialogRegister = new ContentDialog();
            var stackPanel = new StackPanel();
            avaterRegister = new Image();
            url_ImageRegister = new TextBox();
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
            // Tạo input tài khoản. 
            var emailValid = new TextBlock
            {
                Name = "email",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "Email không được để trống hoặc phải lớn hơn 5 ký tự!"
            };

            // Tạo input pass. 
            var textBoxPass = new PasswordBox
            {
                Header = "Mật khẩu",
                Name = "Password",
                Margin = new Thickness(50, 5, 50, 10),
                PlaceholderText = "Mật khẩu của bạn.",
            };

            var passValid = new TextBlock
            {
                Name = "password",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "Mật khẩu không được để trống hoặc phải lớn hơn 5 ký tự!"
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
            var firstnameValid = new TextBlock
            {
                Name = "firstName",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "Họ không được để trống "
            };

            // Tạo input lastName. 
            var lastName = new TextBox
            {
                Header = "Tên",
                Name = "LastName",
                PlaceholderText = "Tên của bạn.",
                Margin = new Thickness(50, 5, 50, 10),
            };

            var lastnameValid = new TextBlock
            {
                Name = "lastName",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "tên không được để trống"
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
            avaterRegister.Margin = new Thickness(60, 0, 0, 0);
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

            var phoneValid = new TextBlock
            {
                Name = "phone",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "Số điện thoại không được để trống!"
            };

            var address = new TextBox
            {
                Header = "Địa chỉ",
                Margin = new Thickness(50, 5, 50, 10),
                Name = "Address",
                AcceptsReturn = true,
                PlaceholderText = "Địa chỉ của bạn."
            };

            var addressValid = new TextBlock
            {
                Name = "address",
                Margin = new Thickness(50, 5, 50, 10),
                Visibility = Visibility.Collapsed,
                FontSize = 12,
                Text = "Địa chỉ không được để trống!"
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
                Tag = 0,
            };

            var Other = new RadioButton
            {
                Content = "Khác",
                Tag = 2,
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
            stackPanel.Children.Add(emailValid);
            stackPanel.Children.Add(textBoxPass);
            stackPanel.Children.Add(passValid);
            stackPanel.Children.Add(firstName);
            stackPanel.Children.Add(firstnameValid);
            stackPanel.Children.Add(lastName);
            stackPanel.Children.Add(lastnameValid);
            stackPanel.Children.Add(url_ImageRegister);
            stackPanel.Children.Add(stackPanelAvatar);
            stackPanel.Children.Add(phone);
            stackPanel.Children.Add(phoneValid);
            stackPanel.Children.Add(address);
            stackPanel.Children.Add(addressValid);
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
                        if (emailValid.Name == errorField)
                        {
                            emailValid.Text = errResponse.error[errorField];
                            emailValid.Visibility = Visibility.Visible;
                        }
                        else if (passValid.Name == errorField)
                        {
                            passValid.Text = errResponse.error[errorField];
                            passValid.Visibility = Visibility.Visible;
                        }
                        else if (firstnameValid.Name == errorField)
                        {
                            firstnameValid.Text = errResponse.error[errorField];
                            firstnameValid.Visibility = Visibility.Visible;
                        }
                        else if (lastName.Name == errorField)
                        {
                            lastName.Text = errResponse.error[errorField];
                            lastName.Visibility = Visibility.Visible;
                        }
                        else if (addressValid.Name == errorField)
                        {
                            addressValid.Text = errResponse.error[errorField];
                            addressValid.Visibility = Visibility.Visible;
                        }
                        else if (phoneValid.Name == errorField)
                        {
                            phoneValid.Text = errResponse.error[errorField];
                            phoneValid.Visibility = Visibility.Visible;
                        }

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



        public static async Task DoLogin()
        {
            // Auto login nếu tồn tại file token 
            currentLogin = new Member();
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            if (await folder.TryGetItemAsync("token.txt") != null)
            {
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

                currentLogin.firstName = userInfoJson.firstName;
                currentLogin.lastName = userInfoJson.lastName;
                currentLogin.avatar = userInfoJson.avatar;
                currentLogin.phone = userInfoJson.phone;
                currentLogin.address = userInfoJson.address;
                currentLogin.introduction = userInfoJson.introduction;
                currentLogin.gender = userInfoJson.gender;
                currentLogin.birthday = userInfoJson.birthday;
                currentLogin.email = userInfoJson.email;
                currentLogin.password = userInfoJson.password;
                currentLogin.id = userInfoJson.id;
                currentLogin.salt = userInfoJson.salt;
                currentLogin.createdAt = userInfoJson.createdAt;
                currentLogin.updatedAt = userInfoJson.updatedAt;
                currentLogin.status = userInfoJson.status;

                GlobalFlySong._isLogin = true;
            }
            else
            {
                Debug.WriteLine("File doesn't exist");
                isLogin = false;
            };
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            await DoLogin();
            //Change UI nếu Logined
            if (isLogin)
            {
                if (currentLogin.id == 1538886332539 && currentLogin.email == "quocviet.hn98@gmail.com")
                {
                    SaveDB.Visibility = Visibility.Visible;
                }
                else
                {
                    SaveDB.Visibility = Visibility.Collapsed;
                }
                this.ShowLoginButton.Visibility = Visibility.Collapsed;
                this.ShowUserInfo.Visibility = Visibility.Visible;
                InfoUser.Text = currentLogin.firstName + " " + currentLogin.lastName;
                var dialog = new Windows.UI.Popups.MessageDialog("Chào mừng quay " + currentLogin.firstName + " " + currentLogin.lastName + " quay trở lại!");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Đóng") { Id = 1 });
                dialog.CancelCommandIndex = 1;
                await dialog.ShowAsync();
            }
            else
            {

            }

            // Đi thẳng vào MainPage và thích móc gì thì móc
            //var frame = Window.Current.Content as Frame;
            //var currentPage = frame.Content as Page;
            //var appbar = currentPage.FindName("ShowUserInfo");
            //AppBarButton app = appbar as AppBarButton;
            //app.Label = "slowV";

            if (GlobalFlySong.GlobalSong != null)
            {
                Debug.WriteLine(GlobalFlySong.GlobalSong);
            }
            else
            {
                Debug.WriteLine("GlobalSong Chưa có gì");
            }

        }

        private async void LogoutUser(object sender, RoutedEventArgs e)
        {
            currentLogin = null;
            StorageFile filed = await ApplicationData.Current.LocalFolder.GetFileAsync("token.txt");
            if (filed != null)
            {
                await filed.DeleteAsync();
                Debug.WriteLine("Xoa thanh cong");
                ShowLoginButton.Visibility = Visibility.Visible;
                ShowUserInfo.Visibility = Visibility.Collapsed;
                var dialog = new Windows.UI.Popups.MessageDialog("Đăng xuất thành công");
                dialog.Commands.Add(new Windows.UI.Popups.UICommand("Đóng") { Id = 1 });
                dialog.CancelCommandIndex = 1;
                await dialog.ShowAsync();
            }
            else
            {

            }
        }

        private readonly IList<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("Home_Page", typeof(Views.HomePage)),
            ("Bxh_Page", typeof(Views.HomePage)),
            ("MusicCPOP_Page", typeof(Views.HomePage)),
            ("MusicUK_Page", typeof(Views.HomePage)),
            ("MusicVPOP_Page", typeof(Views.HomePage)),
        };

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // You can also add items in code behind
            NavView.MenuItems.Add(new NavigationViewItemSeparator());
            NavView.MenuItems.Add(new NavigationViewItem
            {
                Content = "My content",
                Icon = new SymbolIcon(Symbol.Folder),
                Tag = "content"
            });
            _pages.Add(("content", typeof(Views.HomePage)));

            //ContentFrame.Navigated += On_Navigated;

            // NavView doesn't load any page by default: you need to specify it
            NavView_Navigate("Home_Page");

            // Add keyboard accelerators for backwards navigation
            var goBack = new KeyboardAccelerator { Key = VirtualKey.GoBack };
            goBack.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(goBack);

            // ALT routes here
            var altLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left,
                Modifiers = VirtualKeyModifiers.Menu
            };
            altLeft.Invoked += BackInvoked;
            this.KeyboardAccelerators.Add(altLeft);
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {

            if (args.IsSettingsInvoked)
                ContentFrame.Navigate(typeof(MainPage));
            else
            {
                // Getting the Tag from Content (args.InvokedItem is the content of NavigationViewItem)
                var navItemTag = NavView.MenuItems
                    .OfType<NavigationViewItem>()
                    .First(i => args.InvokedItem.Equals(i.Content))
                    .Tag.ToString();

                NavView_Navigate(navItemTag);
            }
        }

        private void NavView_Navigate(string navItemTag)
        {
            var item = _pages.First(p => p.Tag.Equals(navItemTag));
            ContentFrame.Navigate(item.Page);
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            On_BackRequested();
        }

        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private bool On_BackRequested()
        {
            if (!ContentFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed
            if (NavView.IsPaneOpen &&
                (NavView.DisplayMode == NavigationViewDisplayMode.Compact ||
                NavView.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            ContentFrame.GoBack();
            return true;
        }

        //private void On_Navigated(object sender, NavigationEventArgs e)
        //{
        //    NavView.IsBackEnabled = ContentFrame.CanGoBack;

        //    if (ContentFrame.SourcePageType == typeof(Views.HomePage))
        //    {
        //        // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag
        //        NavView.SelectedItem = (NavigationViewItem)NavView.SettingsItem;
        //    }
        //    else
        //    {
        //        var item = _pages.First(p => p.Page == e.SourcePageType);

        //        NavView.SelectedItem = NavView.MenuItems
        //            .OfType<NavigationViewItem>()
        //            .First(n => n.Tag.Equals(item.Tag));
        //    }
        //}


        private async void inforUser(object sender, RoutedEventArgs e)
        {
            dialogRegister = new ContentDialog();
            var stackPanel = new StackPanel();
            avaterRegister = new Image();
            var sv = new ScrollViewer()
            {
                Content = stackPanel
            };

            var email = new TextBlock
            {
                Margin = new Thickness(50, 5, 50, 10),
                Text = "Email: " + currentLogin.email
            };

            // Tạo input tài khoản. 
            var textBoxName = new TextBlock
            {
                Margin = new Thickness(50, 5, 50, 10),
                Text = "Họ và tên: " + currentLogin.firstName + " " + currentLogin.lastName
            };



            var stackPanelAvatar = new StackPanel();
            stackPanelAvatar.Orientation = Orientation.Horizontal;

            //thẻ Image để nhét Avatar khi chụp ảnh.
            avaterRegister.Name = "MyAvatar";
            avaterRegister.Width = 150;
            avaterRegister.Height = 150;
            avaterRegister.Margin = new Thickness(60, 0, 0, 0);
            avaterRegister.HorizontalAlignment = HorizontalAlignment.Right;
            avaterRegister.VerticalAlignment = VerticalAlignment.Center;
            avaterRegister.Source = new BitmapImage(new Uri(currentLogin.avatar));

            stackPanelAvatar.Children.Add(avaterRegister);

            var phone = new TextBlock
            {
                Margin = new Thickness(50, 5, 50, 10),
                Text = "Số điện thoại: " + currentLogin.phone
            };

            var address = new TextBlock
            {
                Margin = new Thickness(50, 5, 50, 10),
                Text = "Địa chỉ: " + currentLogin.address
            };

            var introduction = new TextBlock
            {
                Margin = new Thickness(50, 5, 50, 10),
                Text = "Giới thiệu: " + currentLogin.introduction
            };

            var stackPanelGender = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(50, 5, 50, 10),
            };

            int parGender = currentLogin.gender;

            var gender = new TextBlock();

            if (parGender == 1)
            {
                gender.Text = "Giới tính: Nam";
            }
            else if (parGender == 2)
            {
                gender.Text = "Giới tính: Nữ";
            }
            else
            {
                gender.Text = "Giới tính: Khác";
            }

            stackPanelGender.Children.Add(gender);
            //End Ui giới tính.


            // add giao diện vào layout.
            stackPanel.Children.Add(textBoxName);
            stackPanel.Children.Add(email);
            stackPanel.Children.Add(stackPanelAvatar);
            stackPanel.Children.Add(phone);
            stackPanel.Children.Add(address);
            stackPanel.Children.Add(stackPanelGender);
            stackPanel.Children.Add(introduction);

            stackPanel.MinWidth = Window.Current.Bounds.Width - 600;
            stackPanel.MaxWidth = Window.Current.Bounds.Height;
            dialogRegister.MinWidth = Window.Current.Bounds.Width;
            dialogRegister.MaxWidth = Window.Current.Bounds.Height;

            Style btnPrimaryStyle = (Style)App.Current.Resources["myButtonPrimary"];
            dialogRegister.Title = "Thông tin ";
            dialogRegister.Content = sv;
            dialogRegister.CloseButtonText = "Đóng";
            dialogRegister.CloseButtonStyle = btnPrimaryStyle;
            await dialogRegister.ShowAsync();
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

        private async void SaveSong(object sender, RoutedEventArgs e)
        {
            Song Song = new Song();
            List<Song> i = await Get_List_Song();
            foreach (var item in i)
            {
                if (item.description == null)
                {
                    Song.id = item.id;
                    Song.name = item.name;
                    Song.description = "None";
                    Song.singer = item.singer;
                    Song.author = item.author;
                    Song.thumbnail = item.thumbnail;
                    Song.link = item.link;
                }
                else
                {
                    Song.id = item.id;
                    Song.name = item.name;
                    Song.description = item.description;
                    Song.singer = item.singer;
                    Song.author = item.author;
                    Song.thumbnail = item.thumbnail;
                    Song.link = item.link;
                }
                Db_Table_Song(Song);
            };
        }

        private static void Db_Table_Song(Song song)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=slowvmusic.db"))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand();
                insertCommand.Connection = db;

                // Use parameterized query to prevent SQL injection attacks
                insertCommand.CommandText = "INSERT OR REPLACE INTO Song VALUES ( COALESCE((SELECT id FROM Song WHERE id = @id), @id), @name, @description, @singer, @author, @thumbnail, @link);";
                insertCommand.Parameters.AddWithValue("@id", song.id);
                insertCommand.Parameters.AddWithValue("@name", song.name);
                insertCommand.Parameters.AddWithValue("@description", song.description);
                insertCommand.Parameters.AddWithValue("@singer", song.singer);
                insertCommand.Parameters.AddWithValue("@author", song.author);
                insertCommand.Parameters.AddWithValue("@thumbnail", song.thumbnail);
                insertCommand.Parameters.AddWithValue("@link", song.link);
                insertCommand.ExecuteReader();
                db.Close();
            }
        }

        public void KeyDownSearch(object sender, KeyRoutedEventArgs e)
        {
            List<Song> i = Model.SongModel.Search(search.Text);
            ContentFrame.Navigate(typeof(Views.SearchPage), i);
        }

        // Check UI Main
    }
}
