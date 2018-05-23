using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneDriveClientHazi.Services
{
    public class GraphService
    {
        //private static GraphClient

        public GraphServiceClient GraphClient { get; set; }

        /// <summary>
        /// Fálj letöltése a one drive szervereiről
        /// </summary>
        /// <param name="driveItem"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task saveFile(DriveItem driveItem, StorageFile file)
        {
            if (null == driveItem)
                return ;

            using (var stream = await GraphClient.Drive.Items[driveItem.Id].Content.Request().GetAsync())
            using (BinaryReader br = new BinaryReader(stream))
            {
                byte[] content;
                content = br.ReadBytes((int)stream.Length);
                
                    await FileIO.WriteBytesAsync(file, content);
                 
            }
            
        }

        /// <summary>
        /// Fálj feltöltése a one drive szervereire
        /// </summary>
        /// <param name="file"></param>
        /// <param name="fodlerId"></param>
        /// <returns></returns>
        internal async Task<DriveItem> uploadFile(StorageFile file, string fodlerId)
        {
            using(Stream stream = await file.OpenStreamForReadAsync())
            {
                try
                {
                    var uploadedItem =
                        await
                            GraphClient.Drive.Items[fodlerId].ItemWithPath(file.Name).Content.Request()
                                .PutAsync<DriveItem>(stream);

                    return uploadedItem;
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
        }
    }
}
