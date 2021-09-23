using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public Canvas arCanvas;
    public Canvas menuCanvas;
    public Image testImg;

    public Texture2D tex; // Texture (image) to track

    public ArController controller;
    private void PickImage( int maxSize ) // opens a native image picker and returns the location of chosen pic
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery( ( path ) =>
        {
            Debug.Log( "Image path: " + path );
            if (path != null)
            {
                Texture2D textureTpm = NativeGallery.LoadImageAtPath( path, maxSize );
                if(textureTpm == null)
                {
                    Debug.Log( "Couldn't load texture from " + path );
                    return;
                }
                
                tex = textureTpm;
            }
        } );
        
        Debug.Log( "Permission result: " + permission );
        testImg.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);;
        return;
    }
    
    // initializes AR image tracking, using a gallery photo with picking capability
    public void ImagePick()
    {
        PickImage(1000);
    }
    public void InitARGallery()
    {
        // PickImage(1000);
        menuCanvas.enabled = false;
        arCanvas.enabled = true;
        
        controller.StartAR(tex); // Starts AR session with chosen Texture2D tracking
    }

    public void ToMenu()
    {
        controller.StopAR();
        
        menuCanvas.enabled = true;
        arCanvas.enabled = false;
    }

    public void FixPermissions()
    {
        NativeGallery.RequestPermission(NativeGallery.PermissionType.Read);
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
