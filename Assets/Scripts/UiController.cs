using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public Canvas arCanvas;
    public Canvas menuCanvas;
    public Canvas serverCanvas;
    
    public ArController controller;
    public InputField inputFieldImgSize;
    public ImageTargetsServer imageTargetsServer;

    public Dropdown drop;
    public Text targetId;
    public Text targetDescription;
    public Image targetImage;
    public InputField inputFieldImgSizeServer;
    
    public Texture2D tex; // Texture (image) to track
    private float _imageSize = 0.21f;
    
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
        serverCanvas.enabled = false;
        arCanvas.enabled = true;
        
        controller.StartAR(tex, _imageSize); // Starts AR session with chosen Texture2D tracking
    }

    public void ToMenu()
    {
        controller.StopAR();
        
        arCanvas.enabled = false;
        serverCanvas.enabled = false;
        menuCanvas.enabled = true;
    }

    public void OnImageSizeChanged()
    {
        string size;

        if (menuCanvas.enabled)
            size = inputFieldImgSize.text;
        else
            size = inputFieldImgSizeServer.text;

        Debug.Log("imageSize set to " + size);
        _imageSize = float.Parse(size);
    }

    public void ToServerGallery()
    {
        menuCanvas.enabled = false;
        serverCanvas.enabled = true;

        List<string> IDs = imageTargetsServer.GETTargets();
        drop.AddOptions(IDs);
    }

    public void OnDropdownOptionSelected()
    {
        Dictionary<string, string> data = imageTargetsServer.GETTargetInfo(drop.options[drop.value].text);

        targetId.text = data["id"];
        targetDescription.text = data["description"];

        Texture2D tmpTexture = new Texture2D(1, 1);
        tmpTexture.LoadImage(Convert.FromBase64String(data["image"]));
        tmpTexture.Apply();
        
        targetImage.sprite = Sprite.Create(tmpTexture, new Rect(0.0f, 0.0f, tmpTexture.width, tmpTexture.height), new Vector2(0.5f, 0.5f), 100.0f);

        tex = tmpTexture;
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
