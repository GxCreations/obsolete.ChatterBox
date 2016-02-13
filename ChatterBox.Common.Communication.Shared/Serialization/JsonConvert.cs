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
using System.Runtime.Serialization.Json;
using System.Text;

namespace ChatterBox.Common.Communication.Serialization
{
    public static class JsonConvert
    {
        public static object Deserialize(string json, Type type)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
            {
                return (new DataContractJsonSerializer(type)).ReadObject(stream);
            }
        }

        public static string Serialize(object instance)
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(instance.GetType());
                serializer.WriteObject(stream, instance);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}