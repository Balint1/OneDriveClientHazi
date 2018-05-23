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
using System.ComponentModel;
using Windows.Storage;
using System.IO;

namespace OneDriveClientHazi.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {

        public NotifyData CurrentFolder { get; set; } = new NotifyData();
        public NotifyData SelectedItem { get; set; } = new NotifyData();
        public ObservableCollection<MyDriveItem> DriveItems { get; set; } = new ObservableCollection<MyDriveItem>();

        private  GraphServiceClient graphClient;

        private  DriveItem rootFolder;

        private ClientType clientType { get; set; }

        private Stack<string> folderHistory = new Stack<string>();

        GraphService graphService = new GraphService();

        /// <summary>
        /// Egy ekem betöltése ha rákattintottunk
        /// </summary>
        /// <param name="driveItem"></param>
        public async void ItemClicked(DriveItem driveItem)
        {
            SelectedItem.Item = driveItem;
            if (driveItem.Folder != null)
            {
                folderHistory.Push(CurrentFolder.Item.Id);
                await LoadFolderFromId(driveItem.Id);
            }
        }

        /// <summary>
        /// Fálj feltöltése megadott mappába
        /// </summary>
        /// <param name="file">Feltöltendő file</param>
        /// <param name="fodlerId">Mappa ID-ja</param>
        /// <returns></returns>
        internal async Task UploadFileAsync(StorageFile file, string fodlerId)
        {
            try
            {
                var uploadedFile = await graphService.uploadFile(file, fodlerId);
                await LoadFolderFromId(CurrentFolder.Item.Id);
            }
            catch (Exception e)
            {
                PresentServiceException(e);
            }
        }

        /// <summary>
        /// Az aktuális mappa szülőjének betöltése
        /// </summary>
        public async void ParentFolder()
        {
            if (folderHistory.Count == 0)
                return;
            
            await LoadFolderFromId(folderHistory.Pop());
        }

        /// <summary>
        /// Kijelentkezés a bejelentkezett accountokból
        /// </summary>
        public async void SingOut()
        {
            AuthenticationService.SignOut();
            DriveItems.Clear();
            SelectedItem.Item = null;
            CurrentFolder.Item = null;
            graphClient = null;
        }

        /// <summary>
        /// Bejelentkezés és bejelentkezőablak feldobása
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SignIn()
        {
            try
            {
                graphClient = AuthenticationService.GetAuthenticatedClient();
                graphService.GraphClient = graphClient;
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
            }

            try
            {
                await LoadFolderFromPath();
            }
            catch (ServiceException exception)
            {
                PresentServiceException(exception);
                graphClient = null;
            }
            return true;
        }

        private async Task LoadFolderFromPath(string path = null)
        {
            if (null == graphClient) return;

            try
            {
                DriveItem folder;

                var expandValue = this.clientType == ClientType.Consumer
                    ? "thumbnails,children($expand=thumbnails)"
                    : "thumbnails,children";

                if (path == null)
                {
                    folder = await graphClient.Drive.Root.Request().Expand(expandValue).GetAsync();
                    rootFolder = folder;
                }
                else
                {
                    folder =
                        await
                            graphClient.Drive.Root.ItemWithPath("/" + path)
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
                CurrentFolder.Item = folder;
                SelectedItem.Item = folder;

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
                MyDriveItem myDriveItem = new MyDriveItem
                {
                    DriveItem = obj
                };
                try
                {
                    var thumb = await graphClient.Drive.Items[obj.Id].Thumbnails["0"]["large"].Request().GetAsync();
                    myDriveItem.ThumbnailUrl = thumb.Url;
                }
                catch
                {

                }
                
                DriveItems.Add(myDriveItem);
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
            if (null == graphClient) return;

            LoadChildren(new DriveItem[0]);
            
            try
            {
                var expandString = this.clientType == ClientType.Consumer
                    ? "thumbnails,children($expand=thumbnails)"
                    : "thumbnails,children";

                var folder =
                    await graphClient.Drive.Items[id].Request().Expand(expandString).GetAsync();

                //SelectedItem.Item = folder;
                ProcessFolder(folder);
            }
            catch (Exception exception)
            {
                PresentServiceException(exception);
            }
            
        }

        /// <summary>
        /// Fálj letöltése megadott névvel
        /// </summary>
        /// <param name="driveItem">Letöltendő file</param>
        /// <param name="fileName">Path névvel együtt</param>
        /// <returns></returns>
        public async Task DownloadFile(DriveItem driveItem, StorageFile fileName)
        {
            if (graphClient == null)
                return;
            try
            {
                await graphService.saveFile(driveItem, fileName);
            }
            catch (Exception e)
            {
                PresentServiceException(e);
            }
        }

    }
}
