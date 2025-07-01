using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public partial class MultimediaFile : ObservableObject
    {
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public string ContentType { get; set; } = string.Empty;

        public string FileIcon { get; set; } = "📁";

        public string FileSizeFormatted
        {
            get
            {
                if (FileSize < 1024)
                    return $"{FileSize} B";
                else if (FileSize < 1024 * 1024)
                    return $"{FileSize / 1024.0:F1} KB";
                else
                    return $"{FileSize / (1024.0 * 1024.0):F1} MB";
            }
        }
    }
}
