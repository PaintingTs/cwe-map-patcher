using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CWE_MapPatcher
{
    class Logger
    {
        public static void Debug(string message)
        {
        #if DEBUG
            System.Windows.Forms.MessageBox.Show(message);
        #endif
        }
    }
}
