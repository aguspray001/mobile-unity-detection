using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Message;
using HelperUtilities;

namespace APIControllers
{
    public class APIController : MonoBehaviour
    {
        public void PostData(string url, WWWForm data, System.Action<PredictResponse> cb) => StartCoroutine(PostDataCoRoutine(url, data, cb));
        public void PutData(string url, byte[] data) => StartCoroutine(PutDataCoroutine(url, data));
        public void GettData(string url) => StartCoroutine(GetDataCoroutine(url));

        public IEnumerator PostDataCoRoutine(string url, WWWForm data, System.Action<PredictResponse> cb)
        {

            UnityWebRequest request = UnityWebRequest.Post(url, data);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                cb(new PredictResponse{
                    error = request.error
                });
            }
            if(request.isDone){
                string resp = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                JSONNode wsInfo = JSON.Parse(resp);
                string[] dataClass = Helper.getNestedDataJsonString(wsInfo, "classes");
                float[] dataConfidence = Helper.getNestedDataJsonFloat(wsInfo, "confidences");
                float[,] dataBoxes = Helper.getDoubleNestedDataJsonFloat(wsInfo, "boxes");
                // string resp = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                // var jsonData = JsonUtility.ToJson(resp);
                cb(JsonUtility.FromJson<PredictResponse>(resp));
            }
        }

        public IEnumerator PutDataCoroutine(string url, byte[] data)
        {
            UnityWebRequest request = UnityWebRequest.Put(url, data);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("success put the data!");
            }
        }

        public IEnumerator GetDataCoroutine(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    Debug.Log("get data success" + request.downloadHandler.text);
                }
            }
        }
    }
}