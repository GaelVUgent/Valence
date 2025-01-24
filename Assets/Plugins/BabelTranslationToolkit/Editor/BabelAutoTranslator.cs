using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class BabelAutoTranslator {

    private const string uri = "https://babelfish.com/";

    private const string postFormat = @"POST / babelfish / tr / HTTP / 1.0\n
Content-Type: application/x-www-form-urlencoded
Content-Length: 51
lp={1}_{2}&tt=urltext&intl=1&doit=done&urltext={0}";

    public BabelAutoTranslator(List<string> input, string sourceLanguage, string targetLanguage) {

        string post = string.Format(postFormat, "hello", "en", "nl");
        using(UnityWebRequest webRequest = UnityWebRequest.Post(uri, post)) {

            
            webRequest.SendWebRequest();
            while(!webRequest.isDone && !IsError(webRequest.result)) {
                Thread.Sleep(100);
            }

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if(IsError(webRequest.result)) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }
        }
    }

    private bool IsError(UnityWebRequest.Result result) {
        return result == UnityWebRequest.Result.ConnectionError;
    }

}
    