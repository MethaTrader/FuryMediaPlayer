using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuryMediaPlayer_framework
{
    public sealed class MediaContent
    {
        //имя файла
        public string Filename { get; set; }
        //заголовок
        public string Title { get; set; }
        // аудио или видео
        public string Type { get;set; }
        
        public TimeSpan Duraction { get; set; }

        public MediaContent(string filename, string title, string type, TimeSpan duraction)
        {
            Filename = filename;
            Title = title;
            Type = type;
            Duraction = duraction;
        }
    }
}
