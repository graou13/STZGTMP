using                       System;
using                       System.Collections.Generic;
using                       System.Linq;
using                       System.Text;
using                       System.Threading.Tasks;

namespace                   WindowsMediaPlayer
{
    class MWException :     System.Exception
    {
        public MWException(string reason) : base(reason)
        {}
    }
}
