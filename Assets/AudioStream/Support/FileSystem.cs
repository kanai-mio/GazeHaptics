// (c) 2016-2024 Martin Cvengros. All rights reserved. Redistribution of source code without permission not allowed.

using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AudioStreamSupport
{
    public static class FileSystem
    {
        // ========================================================================================================================================
        #region DL cache
        /// <summary>
        /// Returns complete path of SHA512 unique hash of given (url + uniqueCacheId) in temp cache file path
        /// Appends 'extension' as file name extension
        /// </summary>
        /// <param name="fromUrl">Base url/filename</param>
        /// <param name="uniqueCacheId">Optional unique id which will be appended to url for having more than one cached downloads from a single source</param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string TempFilePath(string fromUrl, string uniqueCacheId, string extension)
        {
            // Unity cycle/reload
            if (string.IsNullOrWhiteSpace(RuntimeSettings.temporaryDirectoryPath))
                return "";

            var fileName = FileSystem.EscapedBase64Hash(fromUrl + uniqueCacheId);
            return Path.Combine(RuntimeSettings.temporaryDirectoryPath, fileName + extension);
        }
        /// <summary>
        /// Returns complete filesystem path of url + extension in download cache directory
        /// Due to OS max. path length limit on Windows the result is truncated to first ~200 characters
        /// </summary>
        /// <param name="fromUrl"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string DownloadCacheFilePath(string fromUrl, string extension)
        {
            // Unity cycle/reload
            if (string.IsNullOrWhiteSpace(RuntimeSettings.downloadCachePath))
                return "";

            var filename = new string(FileSystem.ReplaceInvalidFilesystemCharacters(fromUrl, '_').Take(100).ToArray());
            var filepath = Path.Combine(RuntimeSettings.downloadCachePath, filename);
            var result = filepath.Substring(0, Math.Min(filepath.Length, 200));
            return result + extension;
        }
        public static string EscapedBase64Hash(string ofUri)
        {
            var byteArray = ofUri.ToCharArray().Select(s => (byte)s).ToArray<byte>();

            using (var sha = System.Security.Cryptography.SHA512.Create())
            {
                var hash = sha.ComputeHash(byteArray);

                return Uri.EscapeDataString(
                    Convert.ToBase64String(hash)
                    );
            }
        }
        #endregion
        // ========================================================================================================================================
        #region filesystem
        public static (long, long) DirectoryInfo(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (0, 0);

            var di = new System.IO.DirectoryInfo(path);
            if (!di.Exists)
                return (0, 0);

            var files = di.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly);

            var result = (files.Sum(f => f.Length), files.Count());
            return result;
        }
        /// <summary>
        /// just directly replaces any non filesystem character as considered by mono/.net by supplied one
        /// </summary>
        /// <param name="ofString"></param>
        /// <param name="withCharacter"></param>
        /// <returns></returns>
        public static string ReplaceInvalidFilesystemCharacters(string ofString, char withCharacter)
        {
            // "http://mydomain/_layouts/test/MyLinksEdit.aspx?auto=true&source=http://vtss-sp2010hh:8088/AdminReports/helloworld.aspx?pdfid=193&url=http://vtss-sp2010hh:8088/AdminReports/helloworld.aspx?pdfid=193%26pdfname=5.6%20Upgrade&title=5.6 Upgrade"
            var invalidFilenameChars = Path.GetInvalidFileNameChars();
            var invalidPathChars = Path.GetInvalidPathChars();

            var result = ofString.ToCharArray();
            for (var i = 0; i < result.Length; ++i)
            {
                var ch = result[i];
                if (invalidFilenameChars.Contains(ch)
                    || invalidPathChars.Contains(ch)
                    )
                    result[i] = withCharacter;
            }

            return new string(result);
        }
        #endregion
        // ========================================================================================================================================
        #region filesystem demo assets
        /// <summary>
        /// On Android copies a file out of application archive StreamingAssets into external storage directory and returns its new file path
        /// On all other platforms just returns StreamingAssets location directly
        /// </summary>
        /// <param name="filename">file name in StreamingAssets</param>
        /// <param name="newDestination">called with new file path destination once file is copied out</param>
        /// <returns></returns>
        public static IEnumerator GetFilenameFromStreamingAssets(string filename, string inSubFolder, System.Action<string> newDestination)
        {
            var sourceFilepath = System.IO.Path.Combine(System.IO.Path.Combine(Application.streamingAssetsPath, inSubFolder) , filename);

            if (Application.platform == RuntimePlatform.Android)
            {
                using (AndroidJavaClass jcEnvironment = new AndroidJavaClass("android.os.Environment"))
                {
                    using (AndroidJavaObject joExDir = jcEnvironment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory"))
                    {
                        var destinationDirectory = joExDir.Call<string>("toString");
                        var destinationPath = System.IO.Path.Combine(destinationDirectory, filename);

                        // 2018_3 has first deprecation warning
    #if UNITY_2018_3_OR_NEWER
                        using (var www = UnityEngine.Networking.UnityWebRequest.Get(sourceFilepath))
                        {
                            yield return www.SendWebRequest();

                            if (!string.IsNullOrWhiteSpace(www.error)
    #if UNITY_2020_2_OR_NEWER
                                || www.result != UnityEngine.Networking.UnityWebRequest.Result.Success
    #else
                                || www.isNetworkError
                                || www.isHttpError
    #endif
                                )
                            {
                                Debug.LogErrorFormat("Can't find {0} in StreamingAssets ({1}): {2}", filename, sourceFilepath, www.error);

                                yield break;
                            }

                            while (!www.downloadHandler.isDone)
                                yield return null;

                            Debug.LogFormat("Copying streaming asset, {0}b", www.downloadHandler.data.Length);

                            System.IO.File.WriteAllBytes(destinationPath, www.downloadHandler.data);
                        }
    #else
                        using (WWW www = new WWW(sourceFilepath))
                        {
                            yield return www;

                            if (!string.IsNullOrWhiteSpace(www.error))
                            {
                                Debug.LogErrorFormat("Can't find {0} in StreamingAssets ({1}): {2}", filename, sourceFilepath, www.error);

                                yield break;
                            }

                            System.IO.File.WriteAllBytes(destinationPath, www.bytes);
                        }
    #endif
                        sourceFilepath = destinationPath;
                    }
                }
            }

            newDestination.Invoke(sourceFilepath);
        }
        #endregion
    }
}