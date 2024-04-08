﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleBuilder
{
    [MenuItem("Custom/Build AssetBundle")]
    static void BuildAssetBundle()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";

        if (!System.IO.Directory.Exists(assetBundleDirectory))
        {
            System.IO.Directory.CreateDirectory(assetBundleDirectory);
        }

        //win
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        Debug.Log("Build ab finished");
        AssetDatabase.Refresh();
    }

    public static int splitMipLevel = 6;

    [MenuItem("Assets/Build Texture Binary", false, 0)]
    static void BuildBinary()
    {
        string folderPath = Path.Combine(Application.dataPath, "TextureBytes");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        else
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
        if (!(Selection.activeObject is Texture2D))
        {
            Debug.LogError("Selected object is not a Texture2D.");
            return;
        }

        Texture2D tex2D = Selection.activeObject as Texture2D;
        SerializationToBytes(tex2D, folderPath);
        Debug.Log("Build binary: " + tex2D.name + " finished.");

        // Clean up
        EditorUtility.UnloadUnusedAssetsImmediate();
        AssetDatabase.Refresh();
    }

    public static void SerializationToBytes(Texture2D texture2D, string folderPath)
    {
        byte[] lowResBytes = texture2D.GetStreamedBinaryData(false, splitMipLevel);// For low quality texture
        byte[] highResBytes = texture2D.GetStreamedBinaryData(true, splitMipLevel); // For high quality texture

        string lowResFilePath = Path.Combine(folderPath, texture2D.name + "_ld.bytes");
        string highResFilePath = Path.Combine(folderPath, texture2D.name + "_hd.bytes");

        File.WriteAllBytes(lowResFilePath, lowResBytes);
        File.WriteAllBytes(highResFilePath, highResBytes);
    }
}
