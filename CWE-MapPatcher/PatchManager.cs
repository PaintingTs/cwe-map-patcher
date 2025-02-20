﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CWE_MapPatcher
{
    class PatchManager
    {
        Utils.ArchiveTool _at;
        string _unzippedDir = string.Empty;
        string _filePath;


        public bool Prepare(string filePath)
        {
            _at = new Utils.ArchiveTool();

            _unzippedDir = _at.UnPack(filePath);
            _filePath = filePath;

            if (Clans.AlreadyPatched(_unzippedDir))
                return false;

            foreach (string fileName in Directory.GetFiles(_unzippedDir, "*", SearchOption.AllDirectories))
                Utils.FileAttributesTool.RemoveReadOnly(fileName);


            return true;
        }

        public void Patch(Clans clans = null)
        {
            string xdbFile = Directory.GetFiles(_unzippedDir, "map.xdb", SearchOption.AllDirectories).FirstOrDefault();
            var xdbFileInfo = new FileInfo(xdbFile);

            var xdbPatcher = new XdbPatcher();

            if (clans == null)
                xdbPatcher.OnlyEnableMapScripts(xdbFile);
            else
            {
                xdbPatcher.Patch(xdbFile, clans); //TODO: there could be several 'map.xdb' files?'

                clans.SaveClanScript(xdbFileInfo.DirectoryName);
            }

            CheckMapScriptFiles(xdbFileInfo.DirectoryName);

            // CopyMapObjects(_unzippedDir); // Not used in 2.0 for now. Also it is a bad practice to add global mods as map mods!

            MakeUniqueFolderName(xdbFileInfo.Directory);

            var fileInfo = new FileInfo(_filePath);
            _at.Pack(Path.Combine(fileInfo.DirectoryName, "CWE_" + fileInfo.Name));

            _at.CleanGarbage();
        }


        private static void CopyMapObjects(string rootDirectory)
        {
            string mapObjectsDir = Path.Combine(rootDirectory, "MapObjects");

            if (!Directory.Exists(mapObjectsDir))
                Directory.CreateDirectory(mapObjectsDir);

            foreach(var fileInfo in new DirectoryInfo(@".\resources\MapObjects").GetFiles())
            {
                fileInfo.CopyTo(Path.Combine(mapObjectsDir, fileInfo.Name), false);
            }
        }


        private static void CheckMapScriptFiles(string mapDirectory)
        {
            var xdbScriptFilePath = Path.Combine(mapDirectory, "MapScript.xdb");

            if (!File.Exists(xdbScriptFilePath))
                File.Copy(@".\resources\MapScript\MapScript.xdb", xdbScriptFilePath);

            var luaScriptFilePath = Path.Combine(mapDirectory, "MapScript.lua");

            if (!File.Exists(luaScriptFilePath))
                File.Copy(@".\resources\MapScript\MapScript.lua", luaScriptFilePath);
        }

        private static void MakeUniqueFolderName(DirectoryInfo orignalDir)
        {
            // TODO:
            // non-RMG maps use actual map name as a folder name (instead of GUIDs)
            // check if this code will work fine for non-RMG maps
            // Alternative solution: to use h5m filename as a directory name - this will ensure uniqueness

            string uniqueFolderName = Guid.NewGuid().ToString().ToUpper();
            string destDir = Path.Combine(orignalDir.Parent.FullName, uniqueFolderName);

            Directory.Move(orignalDir.FullName, destDir);
        }
    }
}
