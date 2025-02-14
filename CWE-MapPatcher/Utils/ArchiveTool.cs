using System.IO;
using System.IO.Compression;

namespace CWE_MapPatcher.Utils
{
    class ArchiveTool
    {
        private string _temporaryDir;

        public ArchiveTool()
        {
            _temporaryDir = Path.Combine(Path.GetTempPath(), "CWE");

            if (!Directory.Exists(_temporaryDir))
                Directory.CreateDirectory(_temporaryDir);
        }

        public string ZipPath { get; private set; }

        public string LastOutput { get; protected set; }

        public string UnPack(string fileName)
        {
            _temporaryDir = Path.Combine(_temporaryDir, new FileInfo(fileName).Name);

            if (Directory.Exists(_temporaryDir))
                Directory.Delete(_temporaryDir, true);

            Directory.CreateDirectory(_temporaryDir);

            ZipFile.ExtractToDirectory(fileName, _temporaryDir);

            return _temporaryDir;
        }

        public void Pack(string outputFileName)
        {
            ZipFile.CreateFromDirectory(_temporaryDir, outputFileName);
        }

        public void CleanGarbage()
        {
            if (Directory.Exists(_temporaryDir))
                Directory.Delete(_temporaryDir, true);
        }
    }
}
