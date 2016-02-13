//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace ChatterBox.Client.Common.Avatars
{
    public static class AvatarLink
    {
        public static string EmbeddedLinkFor(int avatar)
        {
            if (avatar >= 1 && avatar <= 10)
            {
                return $"ms-appdata:///local/Avatars/{avatar}.jpg";
            }
            return "ms-appdata:///local/Avatars/0.jpg";
        }

        public static Uri CallCoordinatorUriFor(int avatar)
        {
            var fullPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Avatars");
            fullPath = Path.Combine(fullPath, $"{avatar}.jpg");
            return new Uri(fullPath, UriKind.Absolute);
        }


        public static IAsyncAction ExpandAvatarsToLocal()
        {
            return Task.Run(async () =>
            {
                var assembly = typeof(AvatarLink).GetTypeInfo().Assembly;
                var avatarFiles = assembly.GetManifestResourceNames()
                    .Where(s => s.ToUpper().Contains("AVATARS") && s.ToUpper().EndsWith(".JPG")).ToList();

                var avatarDirectory = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Avatars",
                    CreationCollisionOption.OpenIfExists);

                foreach (var avatarFile in avatarFiles)
                {
                    var filesInDirectory = await avatarDirectory.GetFilesAsync();
                    if (filesInDirectory.Any(s => avatarFile.ToUpper().Contains(s.Name.ToUpper()))) continue;

                    var fileName = avatarFile.Substring(avatarFile.ToUpper().LastIndexOf("AVATARS.") + 8);
                    var file = await avatarDirectory.CreateFileAsync(fileName);

                    using (var sourceStream = assembly.GetManifestResourceStream(avatarFile))
                    using (var targetStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    using (var writeStream = targetStream.AsStreamForWrite())
                    {
                        sourceStream.CopyTo(writeStream);
                        await targetStream.FlushAsync();
                    }
                }
            }).AsAsyncAction();
        }
    }
}