using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.IO;
using System.Text;

public class GeminiImageUploader : MonoBehaviour
{
    public Button uploadButton;
    public Button sendButton;
    public InputField injuryInput;

    private string base64Image = "";
    private string apiKey = "YOUR_GEMINI_API_KEY"; // <- Replace with your real Gemini API key

    void Start()
    {
        uploadButton.onClick.AddListener(UploadImage);
        sendButton.onClick.AddListener(() => StartCoroutine(SendToGemini()));
    }

    void UploadImage()
    {
        string path = UnityEditor.EditorUtility.OpenFilePanel("Select Animal Image", "", "png,jpg,jpeg");
        if (!string.IsNullOrEmpty(path))
        {
            byte[] imageBytes = File.ReadAllBytes(path);
            base64Image = Convert.ToBase64String(imageBytes);
            Debug.Log("Image loaded and encoded.");
        }
        else
        {
            Debug.LogWarning("No image selected.");
        }
    }

    IEnumerator SendToGemini()
    {
        if (string.IsNullOrEmpty(base64Image))
        {
            Debug.LogWarning("Please upload an image first.");
            yield break;
        }

        string injuryText = injuryInput.text;
        string prompt = $"Provide veterinary first aid for this animal with: {injuryText}. Be concise (max 150 words).";

        string jsonData = $@"
        {{
          ""contents"": [{{
            ""parts"": [
              {{
                ""inline_data"": {{
                  ""mime_type"": ""image/png"",
                  ""data"": ""{base64Image}""
                }}
              }},
              {{
                ""text"": ""{prompt}""
              }}
            ]
          }}]
        }}";

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Gemini Response: " + request.downloadHandler.text);
                // You can parse JSON here to extract the first aid advice
            }
            else
            {
                Debug.LogError("Error: " + request.error);
            }
        }
    }
}
