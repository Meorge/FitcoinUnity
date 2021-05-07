using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;

#nullable enable

namespace Fitcoin {
    public class FitcoinService : MonoBehaviour {
        public static readonly string baseURL = "http://192.168.0.32:5000/api";

        public string? AccessToken {
            get {
                return _accessToken;
            }

            set {
                _accessToken = value;
            }
        }

        public string? UserID {
            get {
                return _userID;
            }

            set {
                _userID = value;
            }
        }

        public string? LinkRequestID {
            get {
                return _linkRequestID;
            }

            set {
                _linkRequestID = value;
            }
        }

        private string? _accessToken = null;
        private string? _userID = null;

        private string? _linkRequestID = null;

        public void CreateLinkRequest(Action<string>? onInternalError = null, Action<long, string?>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            Debug.Log($"Create link request, with access token {_accessToken}");

            // Construct the URL
            var url = baseURL + "/service/link/create";

            // Create form data
            var formData = new WWWForm();
            formData.AddField("access_token", _accessToken);

            StartCoroutine(Post(
                url,
                formData,
                onInternalError: onInternalError,
                onResponse: (code, data) => {
                    _linkRequestID = null;
                    if (code == 200) {
                        var response = JsonConvert.DeserializeObject<FitcoinResponse<string>>(data);
                        _linkRequestID = response?.data;

                    }
                    onResponse?.Invoke(code, _linkRequestID);
                }
            ));
        }

        public void GetQRCodeForLinkRequest(Action<string>? onInternalError = null, Action<long, Texture>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            var url = baseURL + "/service/link/qr";

            url += $"?link_request_id={_linkRequestID}";

            StartCoroutine(GetImage(
                url,
                onInternalError: onInternalError,
                onResponse: onResponse
            ));
        }

        public void GetLinkRequestStatus(Action<string>? onInternalError = null, Action<long, FitcoinLinkRequestStatus?>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            var url = baseURL + "/service/link/status";
            url += $"?link_request_id={_linkRequestID}";

            StartCoroutine(Get(
                url,
                onInternalError: onInternalError,
                onResponse: (code, data) => {
                    if (code != 200) {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message;
                        if (errorResponse == null) errorResponse = "No error message";
                        onInternalError?.Invoke(errorResponse);
                        return;
                    }

                    var response = JsonConvert.DeserializeObject<FitcoinResponse<FitcoinLinkRequestStatus>>(data);
                    
                    onResponse?.Invoke(code, response?.data);
                }
            ));
        }

        public void DeleteLinkRequest() {}

        public void MakePurchase(int amount) {}



        private IEnumerator Post(
            string uri,
            WWWForm formData,
            Action<string>? onInternalError,
            Action<long, string>? onResponse
        ) {
            Debug.Log($"POST request to {uri} with data {formData}");

            var request = UnityWebRequest.Post(uri, formData);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                onInternalError?.Invoke(request.error);
            else
                onResponse?.Invoke(request.responseCode, request.downloadHandler.text);
        }

        private IEnumerator Get(
            string uri,
            Action<string>? onInternalError,
            Action<long, string>? onResponse
        ) {
            var request = UnityWebRequest.Get(uri);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                onInternalError?.Invoke(request.error);
            }
            else {
                onResponse?.Invoke(request.responseCode, request.downloadHandler.text);
            }
        }

        private IEnumerator GetImage(
            string uri,
            Action<string>? onInternalError,
            Action<long, Texture>? onResponse
        ) {
            var request = UnityWebRequestTexture.GetTexture(uri);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                onInternalError?.Invoke(request.error);
            }
            else {
                Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                onResponse?.Invoke(request.responseCode, texture);
            }
        }
    }
}