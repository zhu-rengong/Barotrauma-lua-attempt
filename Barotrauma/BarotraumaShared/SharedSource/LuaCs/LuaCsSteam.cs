using Steamworks;
using Steamworks.Data;
using Barotrauma.Steam;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System;
using Steamworks.Ugc;

namespace Barotrauma
{
    partial class LuaCsSteam
    {
        private struct WorkshopItemDownload
        {
            public Steamworks.Ugc.Item Item;
            public string Destination;
            public LuaCsAction Callback;
        }

        double lastTimeChecked = 0;
        List<WorkshopItemDownload> itemsBeingDownloaded = new List<WorkshopItemDownload>();

        public LuaCsSteam()
        {
            
        }

        private async void DownloadWorkshopItemAsync(WorkshopItemDownload download, bool startDownload = false)
        {
            if (startDownload)
            {
                SteamManager.Workshop.NukeDownload(download.Item);
                SteamUGC.Download(download.Item.Id, true);
                itemsBeingDownloaded.Add(download);
            }

            if (download.Item.IsInstalled && Directory.Exists(download.Item.Directory))
            {
                if (download.Callback != null)
                {
                    download.Callback(download.Item);
                }
                
                itemsBeingDownloaded.Remove(download);
                SaveUtil.CopyFolder(download.Item.Directory, download.Destination, true, true);
                return;
            }
        }


        public async void DownloadWorkshopItem(ulong id, string destination, LuaCsAction callback)
        {
            if (!LuaCsFile.IsPathAllowedException(destination)) { return; }

            Steamworks.Ugc.Item? item = await SteamManager.Workshop.GetItem(id);

            if (item == null)
            {
                throw new Exception($"Tried to download invalid workshop item {id}.");
            }

            DownloadWorkshopItemAsync(new WorkshopItemDownload()
            {
                Item = item.Value,
                Destination = destination,
                Callback = callback
            }, true);
        }

        public void DownloadWorkshopItem(Steamworks.Ugc.Item item, string destination, LuaCsAction callback)
        {
            DownloadWorkshopItemAsync(new WorkshopItemDownload()
            {
                Item = item,
                Destination = destination,
                Callback = callback
            }, true);
        }

        public async void GetWorkshopItem(UInt64 id, LuaCsAction callback)
        {
            Steamworks.Ugc.Item? item = await SteamManager.Workshop.GetItem(id);
            callback(item);
        }

        public void Update()
        {
            if (itemsBeingDownloaded.Count > 0 && Timing.TotalTime > lastTimeChecked) // SteamUGC.OnDownloadItemResult for some reason doesn't work, so i need to do this stupid thing.
            {
                foreach (var item in itemsBeingDownloaded.ToArray())
                {
                    DownloadWorkshopItemAsync(item);
                }

                lastTimeChecked = Timing.TotalTime + 15;
            }
        }
    }
}
