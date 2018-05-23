using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveClientHazi.Models
{
    public class MyDriveItem 
    {
        public string ThumbnailUrl { get; set; }

        public DriveItem DriveItem { get; set; }

        /// <summary>
        /// A fálj kiterjesztésének lekérése
        /// </summary>
        public string Extension { get { return DriveItem.Folder == null ? DriveItem.Name.Substring(DriveItem.Name.LastIndexOf('.') + 1) : "Folder"; } }
        /// <summary>
        /// A fálj méretének lekérdezése olvasható formában
        /// </summary>
        public string SizeString { get { return OneDriveClientHazi.Helpers.StringHelper.SizeSuffix(DriveItem.Size ?? 0); } }
        /// <summary>
        /// Utolsó módosítás dátuma LongDate formában
        /// </summary>
        public string LastModifiedShortString { get { return DriveItem.LastModifiedDateTime.Value.ToString("yyyy.MM.dd HH:mm:ss"); } }

    }
}
