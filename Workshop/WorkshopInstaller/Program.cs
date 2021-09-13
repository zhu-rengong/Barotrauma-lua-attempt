using System;
using System.IO;
using System.IO.Compression;


namespace WorkshopInstaller
{
	class Program
	{
		static void Main(string[] args)
		{
			if (File.Exists("Barotrauma.exe"))
			{
				Directory.SetCurrentDirectory("Mods/LuaForBarotrauma/");
			}

			ZipFile.ExtractToDirectory("blacklist_files.zip", ".");

			File.Move("DedicatedServer.exe", "WorkshopInstaller.exe");
			File.Move("DedicatedServer.exe.original", "DedicatedServer.exe");
			File.Create("DedicatedServer.exe.original");
		}
	}
}
