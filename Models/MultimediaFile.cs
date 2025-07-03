using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace QuestHubClient.Models
{
    public partial class MultimediaFile : ObservableObject
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string FileIcon { get; set; } = "📁";
        public string UploadedAt { get; set; } = string.Empty;
        public byte[]? BinaryData { get; set; }
        public ImageSource? ImageSource { get; set; }

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

        public bool IsImage
        {
            get
            {
                var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                return imageTypes.Contains(ContentType?.ToLowerInvariant());
            }
        }

        public bool IsVideo
        {
            get
            {
                var videoTypes = new[] { "video/mp4", "video/avi" };
                return videoTypes.Contains(ContentType?.ToLowerInvariant());
            }
        }

        public bool HasBinaryData => BinaryData != null && BinaryData.Length > 0;

        public bool CanShowPreview => IsImage && ImageSource != null;
        public long GetActualSize()
        {
            return BinaryData?.Length ?? FileSize;
        }
        public async Task<string> SaveToTempFileAsync()
        {
            if (BinaryData == null || BinaryData.Length == 0)
                throw new InvalidOperationException("No hay datos binarios disponibles para guardar");

            var tempPath = System.IO.Path.GetTempPath();
            var tempFileName = $"{Guid.NewGuid()}_{FileName}";
            var fullPath = System.IO.Path.Combine(tempPath, tempFileName);

            await System.IO.File.WriteAllBytesAsync(fullPath, BinaryData);
            return fullPath;
        }
    }
}