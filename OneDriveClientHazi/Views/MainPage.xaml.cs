using Microsoft.Graph;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OneDriveClientHazi.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }


        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AppBarButton_Upload(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppBarButton_Download(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppBarButton_Back(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void AppBarButton_SignIn(object sender, RoutedEventArgs e)
        {
            await ViewModel.SignIn();
        }

        private void AppBarButton_Help(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppBarButton_About(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppBarButton_SignOut(object sender, RoutedEventArgs e)
        {
            ViewModel.SingOut();
        }

        private void Drive_ItemClick(object sender, ItemClickEventArgs e)
        {
            var driveItem = (DriveItem)e.ClickedItem;
            ViewModel.ItemClicked(driveItem);
        }
    }
}