using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using OneDriveClientHazi.Services;
using Microsoft.Graph;
using Windows.UI.Popups;
using System.Collections.ObjectModel;
using OneDriveClientHazi.Models;

namespace OneDriveClientHazi.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {

        public NotifyData CurrentFolderName { get; set; } = new NotifyData();
        public ObservableCollection<DriveItem> DriveItems { get; set; } = new ObservableCollection<DriveItem>();

        private GraphServiceClient graphClient;

        private DriveItem currentFolder;

        private ClientType clientType { get; set; }

        public async void ItemClicked(DriveItem driveItem)
        {
            await LoadFolderFromId(driveItem.Id);
        }

        public async void SingOut()
        {
            AuthenticationService.SignOut();
            
        }

        public async Task<bool> SignIn()
        {

            try
            {
                this.graphClient = AuthenticationService.GetAuthenticatedClient();
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
            }

            try
            {
                await LoadFolderFromPath();

                //UpdateConnectedStateUx(true);
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
                this.graphClient = null;
            }
            return true;
        }

        private async Task LoadFolderFromPath(string path = null)
        {
            if (null == this.graphClient) return;

            // Update the UI for loading something new
            //LoadChildren(new DriveItem[0]);

            try
            {
                DriveItem folder;

                var expandValue = this.clientType == ClientType.Consumer
                    ? "thumbnails,children($expand=thumbnails)"
                    : "thumbnails,children";

                if (path == null)
                {
                    folder = await this.graphClient.Drive.Root.Request().Expand(expandValue).GetAsync();
                }
                else
                {
                    folder =
                        await
                            this.graphClient.Drive.Root.ItemWithPath("/" + path)
                                .Request()
                                .Expand(expandValue)
                                .GetAsync();
                }
                ProcessFolder(folder);
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }

        }

        private void ProcessFolder(DriveItem folder)
        {
            if (folder != null)
            {
                CurrentFolderName.Name = folder.Name;
                this.currentFolder = folder;

                //LoadProperties(folder);

                if (folder.Folder != null && folder.Children != null && folder.Children.CurrentPage != null)
                {
                    LoadChildren(folder.Children.CurrentPage);
                }
            }
        }

        private async  void LoadChildren(IList<DriveItem> items)
        {
            DriveItems.Clear();
            // Load the children
            foreach (var obj in items)
            {
                try
                {
                var thumb = await this.graphClient.Drive.Items[obj.Id].Thumbnails["0"]["large"].Request().GetAsync();
                obj.WebDavUrl = thumb.Url;

                }
                catch
                {

                }
                DriveItems.Add(obj);
            }

        }

        private async static void PresentServiceException(Exception exception)
        {
            string message = null;
            var oneDriveException = exception as ServiceException;
            if (oneDriveException == null)
            {
                message = exception.Message;
            }
            else
            {
                message = string.Format("{0}{1}", Environment.NewLine, oneDriveException.ToString());
            }

            var dialog = new MessageDialog(string.Format("OneDrive reported the following error: {0}", message));
            await dialog.ShowAsync();
        }

        private async Task LoadFolderFromId(string id)
        {
            if (null == this.graphClient) return;

            // Update the UI for loading something new
            //ShowWork(true);
            LoadChildren(new DriveItem[0]);

            try
            {
                var expandString = this.clientType == ClientType.Consumer
                    ? "thumbnails,children($expand=thumbnails)"
                    : "thumbnails,children";

                var folder =
                    await this.graphClient.Drive.Items[id].Request().Expand(expandString).GetAsync();

                ProcessFolder(folder);
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }

            //ShowWork(false);
        }
    }
}
