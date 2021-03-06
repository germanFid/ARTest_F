using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public interface IArController 
{
    public void StartAR(Texture2D imageTarget, float imageSize);
    public void StopAR();
}


public class ArController : MonoBehaviour, IArController
{
    [SerializeField]
    private XRReferenceImageLibrary runtimeImageLibrary;
    
    [SerializeField]
    public ARTrackedImageManager imageManager;
    
    public ARSession session;

    public Text TestTxt;
    public Text TestTxt2;
    void AddImage(Texture2D imageToAdd, float imageSize)
    {
        try
        {
            if (imageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
            {
                AddReferenceImageJobState _jobState;
                _jobState = mutableLibrary.ScheduleAddImageWithValidationJob(
                    imageToAdd,
                    "img", imageSize);
                TestTxt.text = _jobState.status.ToString();
            }
            else
            {
                TestTxt.text = "Lib isn't mutable !";
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            TestTxt.text = e.ToString();
            throw;
        }
    }
    
    public void StartAR(Texture2D imageTarget, float imageSize)
    {
        TestTxt2.text = "StartAR Started";
        // imageManager.referenceLibrary = imageManager.CreateRuntimeLibrary(runtimeImageLibrary);
        imageManager.referenceLibrary = imageManager.CreateRuntimeLibrary(); // Should create a new RuntimeLib
        TestTxt2.text = "Lib created";

        AddImage(imageTarget, imageSize);
        
        TestTxt2.text = "Lib created and Images added";
        
        session.enabled = true;
        imageManager.enabled = true;

        TestTxt2.text = "imageSize: " + imageSize;
    }

    public void StopAR()
    {
        session.Reset();
        session.enabled = false;
        imageManager.enabled = false;
    }
}
