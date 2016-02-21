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
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace ChatterBox.Client.Common.Settings
{
    public static class SettingsExtensions
    {
        private const int _MaxLength = 4096;

        public static void AddOrUpdate(this IPropertySet propertySet, string key, object value)
        {
            if (value is string)
            {
                string temp = (string)value;
                if (temp.Length > _MaxLength)
                {
                    AddOrUpdateBig(propertySet, key, temp);
                    return;
                }
            }

            if (propertySet.ContainsKey(key))
            {
                propertySet[key] = value;
            }
            else
            {
                propertySet.Add(key, value);
            }
        }


        public static void AddOrUpdateBig(this IPropertySet propertySet, string key, string value)
        {
            string fileName = System.Guid.NewGuid().ToString("D") + ".txt";

            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var options = Windows.Storage.CreationCollisionOption.ReplaceExisting;

            var taskCreate = localFolder.CreateFileAsync(
                fileName,
                options).AsTask();

            var taskStore = FileIO.WriteTextAsync(taskCreate.Result, value).AsTask();
            taskStore.Wait();

            propertySet[key + "_File"] = fileName;
        }

        public static string GetKeyStringValue(this IPropertySet propertySet, string key)
        {
            if (propertySet.ContainsKey(key + "_File"))
            {
                string fileName = (string)propertySet[key + "_File"];
                return GetKeyStringValueBig(propertySet, key, fileName);
            }

            object result = propertySet[key];
            if (null == result) return null;
            return (string)result;
        }

        public static string GetKeyStringValueBig(this IPropertySet propertySet, string key, string fileName)
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            var openTask = localFolder.GetFileAsync(fileName).AsTask();

            openTask.Wait();

            var readTask = FileIO.ReadTextAsync(openTask.Result).AsTask();
            readTask.Wait();

            return readTask.Result;
        }
    }
}
