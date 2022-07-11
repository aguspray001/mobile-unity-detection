using System;
using System.Collections;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
// using UnityEngine.Windows;
// External library
using WebSocketSharp;
using Newtonsoft.Json;
// Internal Library
using SimpleJSON;
using HelperUtilities;
using APIControllers;
// UI library
using TMPro;

[RequireComponent(typeof(ARCameraManager))]
public class MobileCameraAccess : MonoBehaviour
{
    WebSocket ws;

    float currentTimer = 0;
    float targetTimer = 1.0f;

    bool writeImageToFileSystem = false;
    private static Texture2D m_Texture;

    public static Texture2D M_Texture { get => m_Texture; set => m_Texture = value; }

    private ARCameraManager cameraManager;

    [SerializeField]
    private bool enableDownsampling = true;
    [SerializeField]
    public static TextMeshProUGUI cameraAccessStateText;

    // private void Start()
    // {
    //     // string url = "https://pokeapi.co/api/v2/pokemon/151";
    //     // GettData(url);
    // }

    private void Awake()
    {
        cameraManager = GetComponent<ARCameraManager>();
    }

    private void Update()
    {
        if (M_Texture != null)
        {
            if (writeImageToFileSystem)
                File.WriteAllBytes(Application.dataPath + "/screencaptured.png", M_Texture.EncodeToJPG());
        }
        currentTimer += Time.deltaTime;
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
        cameraAccessStateText.text = "OnEnable";
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if (currentTimer >= targetTimer)
        {
            cameraAccessStateText.text = "Loading image...";
            if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            {
                using (image)
                {
                    var conversionParams = new XRCpuImage.ConversionParams
                    {
                        // get entire image
                        inputRect = new RectInt(0, 0, image.width, image.height),
                        // downsample by 2
                        outputDimensions = new Vector2Int(enableDownsampling ? image.width / 2 : image.width, enableDownsampling ? image.height / 2 : image.height),
                        // choose rgba format
                        outputFormat = TextureFormat.RGBA32,
                        // flip accross the vertical axis
                        transformation = XRCpuImage.Transformation.MirrorY
                    };

                    int size = image.GetConvertedDataSize(conversionParams);

                    var buffer = new NativeArray<byte>(size, Allocator.Temp);

                    // var bufferArray = buffer.ToArray();
                    // string json = JsonUtility.ToJson(bufferArray);
                    // PacketData packet = new PacketData("img event", "buffer");
                    // string dataBufferImg = JsonUtility.ToJson(packet);
                    // ws.Send(dataBufferImg);

                    // Extract the image data
                    image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);

                    // The image was converted to RGBA32 format and written into the provided buffer
                    // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
                    image.Dispose();

                    // At this point, you can process the image, pass it to a computer vision algorithm, etc.
                    // In this example, you apply it to a texture to visualize it.

                    // You've got the data; let's put it into a texture so you can visualize it.
                    M_Texture = new Texture2D(
                        conversionParams.outputDimensions.x,
                        conversionParams.outputDimensions.y,
                        conversionParams.outputFormat,
                        false);

                    M_Texture.LoadRawTextureData(buffer);
                    M_Texture.Apply();
                    cameraAccessStateText.text = "";

                    // Done with your temporary data, so you can dispose it.
                    buffer.Dispose();

                    currentTimer = 0;
                }
            }
            else { return; }
        }
    }
}
