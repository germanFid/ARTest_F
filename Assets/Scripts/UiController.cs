using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public Canvas arCanvas;
    public Canvas menuCanvas;
    
    public ArController controller;
    public InputField _InputFieldImgSize;
    
    public Texture2D tex; // Texture (image) to track
    private float imageSize = 0.21f;
    
    private void PickImage( int maxSize ) // opens a native image picker and returns the location of chosen pic
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
        {
            Debug.Log( "Image path: " + path );
            if (path != null)
            {
                Texture2D textureTpm = NativeGallery.LoadImageAtPath( path, maxSize, false);
                if(textureTpm == null)
                {
                    Debug.Log( "Couldn't load texture from " + path );
                    return;
                }
                
                tex = textureTpm;
            }
        } );
        
        Debug.Log( "Permission result: " + permission );
        return;
    }
    
    // initializes AR image tracking, using a gallery photo with picking capability
    public void ImagePick()
    {
        PickImage(1000);
    }
    public void InitARGallery()
    {
        menuCanvas.enabled = false;
        arCanvas.enabled = true;
        
        controller.StartAR(tex, imageSize); // Starts AR session with chosen Texture2D tracking
    }

    public void ToMenu()
    {
        controller.StopAR();
        
        menuCanvas.enabled = true;
        arCanvas.enabled = false;
    }

    public void OnImageSizeChanged()
    {
        Debug.Log("imageSize set to " + _InputFieldImgSize.text);
        imageSize = float.Parse(_InputFieldImgSize.text);
    }

    private void Start()
    {
        if (NativeGallery.CheckPermission(NativeGallery.PermissionType.Read) == NativeGallery.Permission.Denied ||
            NativeGallery.CheckPermission(NativeGallery.PermissionType.Read) == NativeGallery.Permission.ShouldAsk )
        {
            NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);
        }
    }
}
