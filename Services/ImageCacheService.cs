using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IImageCacheService
    {
        Task<string> SaveImageToCacheAsync(byte[] imageData, string fileName, string userId);
        Task<byte[]> GetImageFromCacheAsync(string cachedPath);
        Task DeleteImageFromCacheAsync(string cachedPath);
        string GetCachedImagePath(string serverUrl, string userId);
    }

    public class ImageCacheService : IImageCacheService
    {
        private readonly string _cacheDirectory;

        public ImageCacheService()
        {
            _cacheDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QuestHub", "ProfileImages");
            Directory.CreateDirectory(_cacheDirectory);
        }

        public async Task<string> SaveImageToCacheAsync(byte[] imageData, string fileName, string userId)
        {
            try
            {
                var extension = Path.GetExtension(fileName);
                var cachedFileName = $"{userId}_profile{extension}";
                var cachedPath = Path.Combine(_cacheDirectory, cachedFileName);

                await File.WriteAllBytesAsync(cachedPath, imageData);
                return cachedPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar imagen en caché: {ex.Message}");
            }
        }

        public async Task<byte[]> GetImageFromCacheAsync(string cachedPath)
        {
            if (File.Exists(cachedPath))
            {
                return await File.ReadAllBytesAsync(cachedPath);
            }
            return null;
        }

        public async Task DeleteImageFromCacheAsync(string cachedPath)
        {
            if (File.Exists(cachedPath))
            {
                await Task.Run(() => File.Delete(cachedPath));
            }
        }

        public string GetCachedImagePath(string serverUrl, string userId)
        {
            // Buscar archivos que coincidan con el patrón del usuario
            var files = Directory.GetFiles(_cacheDirectory, $"{userId}_profile.*");
            return files.FirstOrDefault();
        }
    }
}
