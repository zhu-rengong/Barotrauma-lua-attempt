using Steamworks;
using Steamworks.Data;
using Barotrauma.Steam;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Barotrauma
{
	partial class LuaCsSteam
	{
        private struct WorkshopItemDownload
        {
            public ulong ID;
            public string Destination;
            public LuaCsAction Callback;
        }

        List<WorkshopItemDownload> itemsBeingDownloaded = new List<WorkshopItemDownload>();

        public LuaCsSteam()
        {
            
        }

        private async void DownloadWorkshopItemAsync(WorkshopItemDownload download)
        {
            Steamworks.Ugc.Item? item = await SteamManager.Workshop.GetItem(download.ID);
            
            if (!item.HasValue) { return; }

            if (item.Value.IsInstalled)
            {
                if (download.Callback != null)
                {
                    Directory.Move(item.Value.Directory, download.Destination);
                    download.Callback(item);
                }
                
                itemsBeingDownloaded.Remove(download);
                return;
            }

            SteamUGC.Download(item.Value.Id, true);

            if (!itemsBeingDownloaded.Contains(download))
            {
                itemsBeingDownloaded.Add(download);
            }
		}


        public void DownloadWorkshopItem(ulong id, string destination, LuaCsAction callback)
        {
            if (!LuaCsFile.IsPathAllowedException(destination)) { return; }

            DownloadWorkshopItemAsync(new WorkshopItemDownload()
            {
                ID = id,
                Destination = destination,
                Callback = callback
            });
        }

        public void DownloadWorkshopItem(Steamworks.Ugc.Item item, string destination, LuaCsAction callback)
        {
            DownloadWorkshopItem(item.Id.Value, destination, callback);
        }

        public async void GetWorkshopItem(UInt64 id, LuaCsAction callback)
        {
            Steamworks.Ugc.Item? item = await SteamManager.Workshop.GetItem(id);
            callback(item);
        }

        public void Update()
        {
            if (itemsBeingDownloaded.Count > 0) // SteamUGC.OnDownloadItemResult for some reason doesn't work, so i need to do this stupid thing.
            {
                foreach (var item in itemsBeingDownloaded)
                {
                    DownloadWorkshopItemAsync(item);
                }
            }
        }
    }
}