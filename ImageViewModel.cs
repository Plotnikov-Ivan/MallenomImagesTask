using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MallenomDesktop
{
    internal class ImageViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}
