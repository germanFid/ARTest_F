using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

public class ImageAttributes
{
    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}

public class ImageData
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("attributes")]
    public ImageAttributes ImageAttributes { get; set; }
}

public class ImageTarget
{
    [JsonProperty("data")]
    public ImageData Data { get; set; }
}

 
public class Attributes
{
    [JsonProperty("image")]
    public string Image { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
}

public class Datum
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("attributes")]
    public Attributes Attributes { get; set; }
}

public class Targets
{
    [JsonProperty("data")]
    public List<Datum> Data { get; set; }
}

public interface IImageTargetsServer
{
    public List<string> GETTargets();
    public Dictionary<string, string> GETTargetInfo(string id);
}

public class ImageTargetsServer : MonoBehaviour, IImageTargetsServer
{
    public UiController _ui;
    
    public string internetAddress;
    public string jwtToken;

    private string targetsJson = "";
    public Dictionary<string, Datum> ServerTargets = new Dictionary<string, Datum>();

    public InputField targetDescription;
    
    IEnumerator GetTargets(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Setting headers
            webRequest.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            webRequest.SetRequestHeader("Accept", "application/json");
            webRequest.SetRequestHeader("Content-Type","application/json");
            
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    targetsJson = webRequest.downloadHandler.text;
                    break;
            }
        }
    }

    public List<string> GETTargets()
    {
        // string json = "{'data':[{'type':'img1','id':'1','attributes':{image:'...',description:'Описание метки, заданное пользователем'}},{'type':'img2','id':'2','attributes':{image:'...',description:'Описание метки, заданное пользователем'}}]}";
        // targetsJson = File.ReadAllText(@"input.txt");
        
        var t = GetTargets(internetAddress + "/api/targets");

        Targets serverTargets = JsonConvert.DeserializeObject<Targets>(targetsJson);
        List<string> idList = new List<string>();

        Debug.Log("Loaded " + serverTargets.Data.Count.ToString() + " targets");

        foreach (Datum i in serverTargets.Data)
        {
            Debug.Log(i.Id);
            Debug.Log(i.Attributes.Description);
            
            ServerTargets.Add(i.Id, i);
            idList.Add(i.Id);
        }

        return idList;
    }

    public Dictionary<string, string> GETTargetInfo(string id)
    {
        Dictionary<string, string> targetProperties = new Dictionary<string, string>();
        targetProperties.Add("id", ServerTargets[id].Id);
        targetProperties.Add("image", ServerTargets[id].Attributes.Image);
        targetProperties.Add("description", ServerTargets[id].Attributes.Description);

        return targetProperties;
    }
    
    IEnumerator Upload(string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("myField", "myData");

        using (UnityWebRequest www = UnityWebRequest.Post(internetAddress + "/api/targets", json))
        {
            // Setting headers
            www.SetRequestHeader("Authorization", "Bearer " + jwtToken);
            www.SetRequestHeader("Accept", "application/json");
            www.SetRequestHeader("Content-Type","application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
    }

    public void UploadTarget()
    {
        if (_ui.tex != null && targetDescription.text != null)
        {
            ImageTarget imageTarget = new ImageTarget();
            
            imageTarget.Data.Type = "imagetarget";
            imageTarget.Data.ImageAttributes.Description = targetDescription.text;
            
            // Encoding _ui.tex to base64
            byte[] bytes;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, _ui.tex);
                bytes = ms.ToArray();
            }
 
            string enc = Convert.ToBase64String(bytes);
            imageTarget.Data.ImageAttributes.Image = enc;

            StartCoroutine(Upload(JsonConvert.SerializeObject(imageTarget)));
            _ui.ToMenu();
        }
    }
}
