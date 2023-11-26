//by RueianSu 2023/11 https://github.com/RueianSu
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace CallChatAPI
{
    public class ChatAPI : MonoBehaviour
    {
        public class ChatMessage
        {
            public string role { get; set; }
            public string content { get; set; }
        }

        /// <summary>
        /// 取得 ChatAPI 回答文字
        /// </summary>
        /// <param name="key">you api key</param>
        /// <param name="systemName">專家名稱</param>
        /// <param name="chatMessage">需要問答內容</param>
        /// <param name="resultCallback">返回內容</param>
        public static void PostChatAPI(string key, string systemName, List<string> chatMessage, Action<string> resultCallback = null)
        {
            ChatAPI.PostChatAPI(key, systemName, chatMessage, "gpt-3.5-turbo", resultCallback);
        }
        /// <summary>
        /// 取得 ChatAPI 回答文字
        /// </summary>
        /// <param name="key">you api key</param>
        /// <param name="systemName">專家名稱</param>
        /// <param name="chatMessage">需要問答內容</param>
        /// <param name="modelGPT">使用模型版本</param>
        /// <param name="resultCallback">返回內容</param>
        public static void PostChatAPI(string key, string systemName, List<string> chatMessage, string modelGPT = "gpt-3.5-turbo", Action<string> resultCallback = null)
        {
            var instance = new GameObject().AddComponent<ChatAPI>();
            instance.StartCoroutine(instance.CallOpenAIChatAPI(key, systemName, chatMessage, modelGPT, resultCallback));
        }

        private IEnumerator CallOpenAIChatAPI(string key, string systemName, List<string> chatMessage, string modelGPT, Action<string> resultCallback)
        {
            var apiKey = key; // Replace with your OpenAI API key
            var apiUrl = "https://api.openai.com/v1/chat/completions";

            var messagesList = MessagesToList(systemName, chatMessage);

            var requestData = new
            {
                model = modelGPT,
                messages = messagesList,
                temperature = 1,
                max_tokens = 256,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            //  var requestDataJson = JsonConvert.SerializeObject(requestData);
            var requestDataJson = JsonUtility.ToJson(requestData);

            print($"Api json - {requestDataJson}");

            var content = new StringContent(requestDataJson, Encoding.UTF8, "application/json");

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            //  API call
            var request = client.PostAsync(apiUrl, content);

            yield return new WaitUntil(() => request.IsCompleted);

            var response = request.Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                resultCallback(responseContent);
                Debug.Log(responseContent);
            }
            else
            {
                Debug.LogError($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                resultCallback($"Error: {response.StatusCode} - {response.ReasonPhrase}");
            }
            Destroy(this.gameObject);
        }

        /// <summary>
        /// 取的 Messages List
        /// </summary>
        /// <param name="systemName">訓練專家名稱</param>
        /// <param name="chatMessage">輸入內容</param>
        /// <returns></returns>
        private List<ChatMessage> MessagesToList(string systemName, List<string> chatMessage)
        {
            var newMessages = new List<ChatMessage>();

            for (int i = 0; i < chatMessage.Count; i++)
            {
                newMessages.Add(
                      new ChatMessage
                      {
                          role = "system",
                          content = systemName
                      });
                newMessages.Add(
                    new ChatMessage
                    {
                        role = "assistant",
                        content = chatMessage[i]
                    });
            }
            return newMessages;
        }
    }
}
