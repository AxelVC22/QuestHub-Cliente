using Google.Protobuf;
using Grpc.Net.Client;
using Multimedia;
using System.IO;
using System.Net.Http;
using QuestHubClient.Models;

namespace QuestHubClient.Services
{
    public class MultimediaUploadService : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly Multimedia.MultimediaService.MultimediaServiceClient _client;
        private readonly int _chunkSize;

        public MultimediaUploadService(string serverAddress = "http://localhost:50052", int chunkSize = 64 * 1024) // 64KB chunks
        {
            _chunkSize = chunkSize;
            var httpHandler = new HttpClientHandler();
            _channel = GrpcChannel.ForAddress(serverAddress, new GrpcChannelOptions
            {
                HttpHandler = httpHandler,
                MaxReceiveMessageSize = 20 * 1024 * 1024, // 20 MB
                MaxSendMessageSize = 20 * 1024 * 1024     // 20 MB
            });

            _client = new Multimedia.MultimediaService.MultimediaServiceClient(_channel);
        }

        public async Task<string> UploadFileAsync(string postId, string filePath, string contentType)
        {
            if (string.IsNullOrEmpty(postId))
                throw new ArgumentException("PostId no puede estar vacío", nameof(postId));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Archivo no encontrado: {filePath}");

            var fileName = Path.GetFileName(filePath);
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            return await UploadFileAsync(postId, fileName, fileBytes, contentType);
        }

        public async Task<string> UploadFileAsync(string postId, string fileName, byte[] fileData, string contentType)
        {
            if (string.IsNullOrEmpty(postId))
                throw new ArgumentException("PostId no puede estar vacío", nameof(postId));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("FileName no puede estar vacío", nameof(fileName));

            if (fileData == null || fileData.Length == 0)
                throw new ArgumentException("FileData no puede estar vacío", nameof(fileData));

            try
            {
                using var call = _client.Upload();

                var chunks = CreateChunks(postId, fileName, fileData, contentType);

                foreach (var chunk in chunks)
                {
                    await call.RequestStream.WriteAsync(chunk);
                }

                await call.RequestStream.CompleteAsync();

                var response = await call.ResponseAsync;
                return response.Url;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al subir archivo: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFileAsync(string postId, MultimediaFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!File.Exists(file.FilePath))
                throw new FileNotFoundException($"Archivo no encontrado: {file.FilePath}");

            var fileBytes = await File.ReadAllBytesAsync(file.FilePath);
            return await UploadFileAsync(postId, file.FileName, fileBytes, file.ContentType);
        }

        public async Task<List<string>> UploadFilesAsync(string postId, List<MultimediaFile> files)
        {
            var urls = new List<string>();

            foreach (var file in files)
            {
                var url = await UploadFileAsync(postId, file);
                urls.Add(url);
            }

            return urls;
        }

        public async Task<List<string>> UploadFilesAsync(string postId, List<FileUploadInfo> files)
        {
            var urls = new List<string>();

            foreach (var file in files)
            {
                var url = await UploadFileAsync(postId, file.FileName, file.Data, file.ContentType);
                urls.Add(url);
            }

            return urls;
        }

        public async Task<string> UploadFileAsync(string postId, string fileName, Stream fileStream, string contentType)
        {
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            return await UploadFileAsync(postId, fileName, fileData, contentType);
        }

        public async Task<List<MultimediaFile>> GetMultimediaAsync(string postId)
        {
            if (string.IsNullOrEmpty(postId))
                throw new ArgumentException("PostId no puede estar vacío", nameof(postId));

            try
            {
                var request = new GetMultimediaRequest
                {
                    PostId = postId
                };

                var response = await _client.GetMultimediaAsync(request);

                var multimediaFiles = new List<MultimediaFile>();

                foreach (var item in response.MultimediaItems)
                {
                    var multimediaFile = new MultimediaFile
                    {
                        FileName = item.OriginalFilename,
                        FilePath = item.FileUrl,
                        ContentType = item.ContentType,
                        FileSize = item.Data?.Length ?? 0,
                        FileIcon = GetFileIcon(item.ContentType),
                        UploadedAt = item.UploadedAt
                    };

                    if (item.Data != null && item.Data.Length > 0)
                    {
                        multimediaFile.BinaryData = item.Data.ToByteArray();

                        if (multimediaFile.IsImage)
                        {
                            multimediaFile.ImageSource = CreateImageSourceFromBytes(multimediaFile.BinaryData);
                        }
                    }

                    multimediaFiles.Add(multimediaFile);
                }

                return multimediaFiles;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener multimedia del post {postId}: {ex.Message}", ex);
            }
        }

        public async Task<MultimediaFile?> GetMultimediaByFilenameAsync(string postId, string filename)
        {
            var allMultimedia = await GetMultimediaAsync(postId);
            return allMultimedia.FirstOrDefault(m =>
                m.FileName.Equals(filename, StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileNameWithoutExtension(m.FileName).Equals(Path.GetFileNameWithoutExtension(filename), StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<MultimediaFile>> GetMultimediaByTypeAsync(string postId, string contentTypePrefix)
        {
            var allMultimedia = await GetMultimediaAsync(postId);
            return allMultimedia.Where(m =>
                m.ContentType.StartsWith(contentTypePrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<List<MultimediaFile>> GetImagesAsync(string postId)
        {
            var allMultimedia = await GetMultimediaAsync(postId);
            return allMultimedia.Where(m => m.IsImage).ToList();
        }

        public async Task<List<MultimediaFile>> GetVideosAsync(string postId)
        {
            var allMultimedia = await GetMultimediaAsync(postId);
            return allMultimedia.Where(m => m.IsVideo).ToList();
        }

        public async Task<bool> HasMultimediaAsync(string postId)
        {
            var multimedia = await GetMultimediaAsync(postId);
            return multimedia.Any();
        }

        public async Task<Dictionary<string, int>> GetMultimediaCountByTypeAsync(string postId)
        {
            var multimedia = await GetMultimediaAsync(postId);
            return multimedia
                .GroupBy(m => m.ContentType.Split('/')[0])
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<MultimediaStats> GetMultimediaStatsAsync(string postId)
        {
            var multimedia = await GetMultimediaAsync(postId);

            return new MultimediaStats
            {
                TotalFiles = multimedia.Count,
                TotalSize = multimedia.Sum(m => m.FileSize),
                ImageCount = multimedia.Count(m => m.IsImage),
                VideoCount = multimedia.Count(m => m.IsVideo),
                OtherCount = multimedia.Count(m => !m.IsImage && !m.IsVideo),
                AverageFileSize = multimedia.Any() ? multimedia.Average(m => m.FileSize) : 0,
                LargestFile = multimedia.OrderByDescending(m => m.FileSize).FirstOrDefault(),
                SmallestFile = multimedia.OrderBy(m => m.FileSize).FirstOrDefault()
            };
        }

        public static MultimediaFile CreateFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Archivo no encontrado: {filePath}");

            var fileInfo = new FileInfo(filePath);
            var contentType = GetContentType(fileInfo.Name);

            return new MultimediaFile
            {
                FileName = fileInfo.Name,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                ContentType = contentType,
                FileIcon = GetFileIcon(contentType)
            };
        }

        public static List<MultimediaFile> CreateFromFiles(IEnumerable<string> filePaths)
        {
            return filePaths.Select(CreateFromFile).ToList();
        }

        private System.Windows.Media.ImageSource CreateImageSourceFromBytes(byte[] imageData)
        {
            try
            {
                using var stream = new MemoryStream(imageData);
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<UploadRequest> CreateChunks(string postId, string fileName, byte[] fileData, string contentType)
        {
            var totalBytes = fileData.Length;
            var offset = 0;

            while (offset < totalBytes)
            {
                var chunkSize = Math.Min(_chunkSize, totalBytes - offset);
                var chunk = new byte[chunkSize];
                Array.Copy(fileData, offset, chunk, 0, chunkSize);

                yield return new UploadRequest
                {
                    PostId = postId,
                    Filename = fileName,
                    Data = ByteString.CopyFrom(chunk),
                    ContentType = contentType
                };

                offset += chunkSize;
            }
        }

        private static string GetFileIcon(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return "📁";

            return contentType.ToLowerInvariant() switch
            {
                var ct when ct.StartsWith("image/") => "🖼️",
                var ct when ct.StartsWith("video/") => "🎥",
                _ => "📁"
            };
        }

        public static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                _ => "application/octet-stream"
            };
        }

        public void Dispose()
        {
            _channel?.Dispose();
        }
    }

    public class FileUploadInfo
    {
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public string ContentType { get; set; }

        public FileUploadInfo(string fileName, byte[] data, string contentType = null)
        {
            FileName = fileName;
            Data = data;
            ContentType = contentType ?? MultimediaUploadService.GetContentType(fileName);
        }
    }

    public class MultimediaStats
    {
        public int TotalFiles { get; set; }
        public long TotalSize { get; set; }
        public int ImageCount { get; set; }
        public int VideoCount { get; set; }
        public int OtherCount { get; set; }
        public double AverageFileSize { get; set; }
        public MultimediaFile? LargestFile { get; set; }
        public MultimediaFile? SmallestFile { get; set; }

        public string TotalSizeFormatted
        {
            get
            {
                if (TotalSize < 1024)
                    return $"{TotalSize} B";
                else if (TotalSize < 1024 * 1024)
                    return $"{TotalSize / 1024.0:F1} KB";
                else
                    return $"{TotalSize / (1024.0 * 1024.0):F1} MB";
            }
        }

        public string AverageFileSizeFormatted
        {
            get
            {
                if (AverageFileSize < 1024)
                    return $"{AverageFileSize:F1} B";
                else if (AverageFileSize < 1024 * 1024)
                    return $"{AverageFileSize / 1024.0:F1} KB";
                else
                    return $"{AverageFileSize / (1024.0 * 1024.0):F1} MB";
            }
        }
    }
}