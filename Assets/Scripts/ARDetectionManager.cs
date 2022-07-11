using System;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using SimpleJSON;

// External library
using WebSocketSharp;
// Internal library
using Message;
using APIControllers;
using HelperUtilities;
// UI library
using TMPro;

public class ARDetectionManager : MonoBehaviour
{

    WebSocket ws;

    [SerializeField]
    private string PredictAPIUrl = "http://157.245.159.61:8000";
    // string[] dataClass = null;
    // float[] dataConfidence = null;
    // float[,] dataBoxes = null;

    bool predictMode = true;

    public bool PredictMode { get => this.predictMode; set => this.predictMode = value; }

    [SerializeField]
    private TextMeshProUGUI m_ClassText;

    [SerializeField]
    private TextMeshProUGUI m_ConfidenceText;

    [SerializeField]
    private TextMeshProUGUI m_BoxText;

    [SerializeField]
    private TextMeshProUGUI ModelName;

    [SerializeField]
    private GameObject m_ButtonCapture;

    float currentTimer = 0;
    float targetTimer = 2.0f;
    public class PacketData
    {
        public string eventMsg;
        public string dataPacket;
        public PacketData(string eventMessage, string dataMessage)
        {
            eventMsg = eventMessage;
            dataPacket = dataMessage;
        }
    }

    private void Awake()
    {
        // ws = new WebSocket("ws://localhost:8000/ws");
        // ws.Connect();
        MobileCameraAccess.cameraAccessStateText.text = "Awake...";
    }
    // Start is called before the first frame update
    void Start()
    {
        // ws.Send(""); //trigger websocket
        // ws.OnMessage += (sender, e) =>
        // {
        //     Debug.Log("ws data => " + e.Data);
        //     Debug.Log("ws data type => " + e.Data.GetType());
        //     JSONNode wsInfo = JSON.Parse(e.Data);
        //     dataClass = Helper.getNestedDataJsonString(wsInfo, "classes");
        //     dataConfidence = Helper.getNestedDataJsonFloat(wsInfo, "confidences");
        //     dataBoxes = Helper.getDoubleNestedDataJsonFloat(wsInfo, "boxes");
        //     // string[] dataClass = getNestedDataJson<string>(wsInfo, "classes");
        //     Debug.Log(dataClass[0]);
        //     Debug.Log(dataConfidence[0]);
        //     Debug.Log(dataBoxes[0, 1]);
        // };
        MobileCameraAccess.cameraAccessStateText.text = "started...";
        predictMode = false;
    }

    // Update is called once per frame
    void Update()
    {
        currentTimer += Time.deltaTime;
        MobileCameraAccess.cameraAccessStateText.text = "Updating...";
        if (currentTimer >= targetTimer)
        {
            // if (ws == null)
            // {
            //     MobileCameraAccess.cameraAccessStateText.text = "WS Null";
            //     return;
            // }
            // else
            // {
                var imageStream = MobileCameraAccess.M_Texture.EncodeToPNG();
                if (imageStream == null)
                {
                    MobileCameraAccess.cameraAccessStateText.text = "Null Image";
                    return;
                }
                else
                {
                    WWWForm form = new WWWForm();
                    APIController apiController = new APIController();
                    form.AddBinaryData("file", imageStream);

                    // ModelName.text = predictMode == true? "/predict" : "/predict-tiny";
                    ModelName.text = "/predict";

                    // apiController.PostData(PredictAPIUrl+"/predict", form, (resp) => Callback(resp));
                    PostData(PredictAPIUrl+"/predict", form, (resp) => Callback(resp));
                    currentTimer = 0;
                }
            // }

        }
    }

    void Callback(PredictResponse response)
    {
        m_ClassText.text = response.classes[0];
        m_ConfidenceText.text = response.confidences[0].ToString();
        
        MobileCameraAccess.cameraAccessStateText.text = "";
    }

    public void pressButton()
    {
        PredictMode = !predictMode;
        // PacketData packet = new PacketData("button event", "onclick");
        // string json = JsonUtility.ToJson(packet);
        // ws.Send(json);
        // if(dataBoxes != null || dataClass != null || dataConfidence != null){
        //     m_BoxText.text = dataBoxes[0, 1].ToString();
        //     m_ClassText.text = dataClass[0];
        //     m_ConfidenceText.text = dataConfidence[0].ToString();
        // }
    }

    public void PostData(string url, WWWForm data, System.Action<PredictResponse> cb) => StartCoroutine(PostDataCoRoutine(url, data, cb));
    public IEnumerator PostDataCoRoutine(string url, WWWForm data, System.Action<PredictResponse> cb)
    {
        UnityWebRequest request = UnityWebRequest.Post(url, data);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            // cb(new PredictResponse
            // {
            //     error = request.error
            // });
            MobileCameraAccess.cameraAccessStateText.text = request.error;

        }
        string resp = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
        m_BoxText.text = request.downloadHandler.text;
        JSONNode wsInfo = JSON.Parse(request.downloadHandler.text);
        string[] dataClass = Helper.getNestedDataJsonString(wsInfo, "classes");
        float[] dataConfidence = Helper.getNestedDataJsonFloat(wsInfo, "confidences");
        float[,] dataBoxes = Helper.getDoubleNestedDataJsonFloat(wsInfo, "boxes");
        MobileCameraAccess.cameraAccessStateText.text = "Image was sent to API...";

        cb(JsonUtility.FromJson<PredictResponse>(resp));
    }
}
