using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowResizer
{
    public class SavedProgram
    {
        public string ApplicationMainWindowTitle;
        public int x = 0;
        public int y = 0;

        public SavedProgram(string processMainWindowTitle, int x, int y)
        {
            this.ApplicationMainWindowTitle = processMainWindowTitle;
            this.x = x;
            this.y = y;
        }

    }
}
