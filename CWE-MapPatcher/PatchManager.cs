using System;
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

            if (_unzippedDir == null)
                return false;

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

            CopyResources(_unzippedDir);

            var fileInfo = new FileInfo(_filePath);
            _at.Pack(Path.Combine(fileInfo.DirectoryName, "CWE_" + fileInfo.Name));

            _at.CleanGarbage();
        }


        private static void CopyResources(string rootDirectory)
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
    }
}
