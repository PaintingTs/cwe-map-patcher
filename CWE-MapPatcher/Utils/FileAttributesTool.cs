using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CWE_MapPatcher.Utils
{
    class FileAttributesTool
    {
        public static void RemoveReadOnly(string fileName)
        {
            var attributes = File.GetAttributes(fileName);

            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // Make the file RW
                attributes = attributes & ~FileAttributes.ReadOnly;

                File.SetAttributes(fileName, attributes);
                Console.WriteLine("The {0} file is no longer RO.", fileName);
            } 
        } 
    }
}
