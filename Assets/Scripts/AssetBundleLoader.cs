﻿using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ASBundleWrapper
{
    public Object Asset { get; set; }
    public Object[] AllAsset { get; set; }

}

public class AssetBundleLoader : MonoBehaviour
{
    private static int m_loadMipmapLevel = 0;

    private GameObject teaportGo;

    //public Texture2D testInsteadABTex;
    public Texture2D anotherNullTex;
    public Material insteadABMat;
    private string folderPath = Path.Combine(Application.streamingAssetsPath, "mybytes");


    private void OnDestroy()
    {
        insteadABMat.mainTexture = null;
        //if (Directory.Exists(folderPath))
        //{
        //    string[] files = Directory.GetFiles(folderPath);
        //    foreach (string file in files)
        //    {
        //        File.Delete(file);
        //    }
        //}
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Q)) //runtime or build ab
        //{
        //    byte[] myBytes = testInsteadABTex.GetStreamedBinaryData(false); //save to myBytes 
        //    File.WriteAllBytes(folderPath + "/_ld", myBytes);

        //    byte[] myBytes2 = testInsteadABTex.GetStreamedBinaryData(true); //save to myBytes 
        //    File.WriteAllBytes(folderPath + "/_hd", myBytes2);
        //}

        if(Input.GetKeyDown(KeyCode.W))  //256、128、64、32、16、8、4、2、1共9部分，共43704 字节
        {
            byte[] myBytes = File.ReadAllBytes(folderPath + "/_ld");
            anotherNullTex = new Texture2D(512, 512);
            anotherNullTex.SetStreamedBinaryData(myBytes,0); //first?
            insteadABMat.mainTexture = anotherNullTex;
        }
        //bytes_h是后面的高清图，即2048、1024、512部分，共2752512 字节
        if (Input.GetKeyDown(KeyCode.E))
        {
            byte[] myBytes = File.ReadAllBytes(folderPath + "/_hd");
            anotherNullTex.SetStreamedBinaryData(myBytes,1); //append?
        }



        //return back to texture independence package
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AssetBundle texBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "alienTex"));
            AssetBundle prefabBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "teaport"));
            
            Object[] objects = prefabBundle.LoadAllAssets();
            teaportGo = Instantiate(objects[0] as GameObject);

            prefabBundle.Unload(false);
            texBundle.Unload(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Texture2D texture = teaportGo.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
            
            AssetBundle texBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "alienTex"));
            texture.ForceSetMipLevel(7, null);
            texBundle.Unload(true);  //why can do this? forceSetMip is async???
        }

        AnimationClip animationClip;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (teaportGo != null)
            {
                //Texture2D texture = teaportGo.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
                --m_loadMipmapLevel;
                string path = Path.Combine(Application.streamingAssetsPath, "teaport");
                StartCoroutine(ForceSetMipLevel(m_loadMipmapLevel, path));
                //texture.ForceSetMipLevel(m_loadMipmapLevel, Path.Combine(Application.streamingAssetsPath, "teaport"));
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (teaportGo != null)
            {
                //Texture2D texture = teaportGo.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;
                ++m_loadMipmapLevel;
                string path = Path.Combine(Application.streamingAssetsPath, "teaport");
                StartCoroutine(ForceSetMipLevel(m_loadMipmapLevel, path));
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            m_loadMipmapLevel = 0;
            DeCompressBundleAsync().Forget();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            m_loadMipmapLevel = 10;
            DeCompressBundleAsync().Forget();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (teaportGo)
            {                
                DestroyImmediate(teaportGo);
                teaportGo = null;
            }
            Resources.UnloadUnusedAssets();
        }
    }

    async UniTaskVoid DeCompressBundleAsync()
    {
        string assetBundlePath = Path.Combine(Application.streamingAssetsPath, "teaport");
        ASBundleWrapper aSBundleWrapper = new ASBundleWrapper();
        await DeCompressBundle(aSBundleWrapper, 0, assetBundlePath);
        teaportGo = Instantiate(aSBundleWrapper.Asset) as GameObject;
    }

    //模仿ASBundleWrapper Load assetbundle
    static async UniTask DeCompressBundle(ASBundleWrapper instance, int downResult, string filePath)
    {
        AssetBundleCreateRequest assetBundleCreateRequest = null;
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"not found {filePath},but downLoad file result is success.Uri:");
                return;
            }

            assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(filePath);
            await assetBundleCreateRequest;

            AssetBundleRequest bundleRequest = assetBundleCreateRequest.assetBundle.LoadAllAssetsAsync(new AssetLoadParameters(false, m_loadMipmapLevel));
            await bundleRequest;

            instance.Asset = bundleRequest.asset;
            instance.AllAsset = bundleRequest.allAssets;
        }
        catch
        { 
        }
        assetBundleCreateRequest.assetBundle.Unload(false);
    }

    IEnumerator ForceSetMipLevel(int mipmapLevel,string abPath)
    {
        Texture2D texture = teaportGo.GetComponent<MeshRenderer>().sharedMaterial.mainTexture as Texture2D;

        AssetBundleCreateRequest assetBundleCreateRequest = AssetBundle.LoadFromFileAsync(abPath);
        yield return assetBundleCreateRequest;

        //AssetBundle assetBundle = AssetBundle.LoadFromFile(abPath);
        texture.ForceSetMipLevel(m_loadMipmapLevel, "");
        //assetBundle.Unload(true);
        //yield return null;

        assetBundleCreateRequest.assetBundle.Unload(true);
    }
}
