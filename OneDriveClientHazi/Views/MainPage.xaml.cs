using Microsoft.Graph;
using OneDriveClientHazi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
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

        private async void AppBarButton_Upload(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentFolder?.Item == null)
            {
                var dialog = new MessageDialog("A feltöltéshez először jelentkezz be!");
                await dialog.ShowAsync();
                return;
            }

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await ViewModel.UploadFileAsync(file, ViewModel.CurrentFolder.Item.Id);
            }
        }

        private async void AppBarButton_Download(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CurrentFolder?.Item == null)
            {
                var dialog = new MessageDialog("A Letöltéshez először jelentkezz be!");
                await dialog.ShowAsync();
                return;
            }
            else
            {
                if (ViewModel.SelectedItem.Item.Folder != null)
                {
                    var dialog = new MessageDialog("A letöltéshez válassz ki egy fájlt!");
                    await dialog.ShowAsync();
                    return;
                }
            }


            MyDriveItem item = new MyDriveItem { DriveItem = ViewModel.SelectedItem.Item };

            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.FileTypeChoices.Add("", new List<string>() { $".{item.Extension}" });
            picker.SuggestedFileName = item.DriveItem.Name.Substring(0, item.DriveItem.Name.LastIndexOf("."));

            var folder = await picker.PickSaveFileAsync();
            if (folder != null)
            {
               
                    // read/write jog a mappára
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                        await ViewModel.DownloadFile(ViewModel.SelectedItem.Item, folder);
            }

        }

        private async void AppBarButton_Back(object sender, RoutedEventArgs e)
        {
            ViewModel.ParentFolder();
        }

        private async void AppBarButton_SignIn(object sender, RoutedEventArgs e)
        {
            await ViewModel.SignIn();
        }

        private void AppBarButton_SignOut(object sender, RoutedEventArgs e)
        {
            ViewModel.SingOut();
        }

        private void Drive_ItemClick(object sender, ItemClickEventArgs e)
        {
            var driveItem = (MyDriveItem)e.ClickedItem;
            ViewModel.ItemClicked(driveItem.DriveItem);
            SizeTB.Text = driveItem.SizeString;
            ExtensionTB.Text = driveItem.Extension;
            LastModifiedTB.Text = driveItem.LastModifiedShortString;
        }
    }
}