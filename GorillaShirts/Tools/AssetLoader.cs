using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GorillaShirts.Tools
{
    internal class AssetLoader
    {
        private static AssetBundle loadedBundle;

        private static Task bundleLoadTask;

        private static readonly Dictionary<string, TaskCompletionSource<Object>> assetCache = [];

        private static readonly Dictionary<string, TaskCompletionSource<Texture2D>> textureCache = [];

        private static async Task LoadAssetBundle()
        {
            TaskCompletionSource<AssetBundle> completionSource = new();

            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream(Constants.AssetBundleName);

            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
            request.completed += _ => completionSource.SetResult(request.assetBundle);

            loadedBundle = await completionSource.Task;
            stream.Close();
        }

        public static async Task<T> LoadAsset<T>(string assetName) where T : Object
        {
            Logging.Message("LoadAsset");
            Logging.Info($"{assetName} of {typeof(T).FullName}");

            if (assetCache.TryGetValue(assetName, out TaskCompletionSource<Object> completionSource))
            {
                Object completedAsset = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
                return (T)completedAsset;
            }

            completionSource = new();
            assetCache.Add(assetName, completionSource);

            if (loadedBundle is null)
            {
                bundleLoadTask ??= LoadAssetBundle();
                await bundleLoadTask;
            }

            AssetBundleRequest request = loadedBundle.LoadAssetAsync<T>(assetName);
            request.completed += _ => completionSource.TrySetResult(request.asset);

            Object result = await completionSource.Task;
            return (T)result;
        }

        public static async Task<Texture2D> LoadTexture(string url)
        {
            Logging.Message("LoadTexture");
            Logging.Info(url);

            if (textureCache.TryGetValue(url, out TaskCompletionSource<Texture2D> completionSource))
            {
                Texture2D texture = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
                return texture;
            }

            completionSource = new();
            textureCache.Add(url, completionSource);

            Texture2D downloadedTexture = null;
            try
            {
                using UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = 15;
                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Logging.Warning($"Could not load pack preview: {request.error}");
                    return null;
                }

                downloadedTexture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!downloadedTexture.LoadImage(request.downloadHandler.data))
                {
                    Object.Destroy(downloadedTexture);
                    downloadedTexture = null;
                }
                return downloadedTexture;
            }
            catch (Exception ex)
            {
                if (downloadedTexture != null) Object.Destroy(downloadedTexture);
                downloadedTexture = null;
                Logging.Warning($"Could not load pack preview: {ex.Message}");
                return null;
            }
            finally
            {
                // Release all callers on failure and allow a later retry.
                completionSource.TrySetResult(downloadedTexture);
                if (downloadedTexture == null) textureCache.Remove(url);
            }
        }
    }
}
