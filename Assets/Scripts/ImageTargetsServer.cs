using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
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
    public string internetAddress;
    public string jwtToken;

    private string targetsJson = "";
    public Dictionary<string, Datum> ServerTargets = new Dictionary<string, Datum>();
    
    IEnumerator GetTargets(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
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
                    break;
            }
        }
    }

    public List<string> GETTargets()
    {
        // string json = "{'data':[{'type':'img1','id':'1','attributes':{image:'...',description:'Описание метки, заданное пользователем'}},{'type':'img2','id':'2','attributes':{image:'...',description:'Описание метки, заданное пользователем'}}]}";
        targetsJson = File.ReadAllText(@"input.txt");
        // var t = GetTargets(internetAddress + "/api/targets");
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
}
