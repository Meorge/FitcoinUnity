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

        public bool MonitoringLinkRequest {
            get {
                return _monitoringLinkRequest;
            }
        }

        public FitcoinUserInfo? CurrentUserInfo {
            get {
                return _currentUserInfo;
            }
        }

        public delegate void FitcoinUserInfoUpdateHandler(FitcoinUserInfo? newInfo);

        public event FitcoinUserInfoUpdateHandler? onUserInfoUpdated;

        private string? _accessToken = null;
        private string? _userID = null;

        private string? _linkRequestID = null;

        private bool _monitoringLinkRequest = false;

        private FitcoinUserInfo? _currentUserInfo = null;

        public void CreateLinkRequest(Action<string>? onError = null, Action<string?>? onResponse = null) {
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
                onInternalError: onError,
                onResponse: (code, data) => {
                    _linkRequestID = null;
                    if (code == 200) {
                        _linkRequestID = JsonConvert.DeserializeObject<FitcoinResponse<string>>(data)?.data;
                        onResponse?.Invoke(_linkRequestID);
                    } else {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message ?? "No error message";
                        onError?.Invoke(errorResponse);
                    }
                }
            ));
        }

        public void GetQRCodeForLinkRequest(Action<string>? onError = null, Action<Texture>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            var url = baseURL + "/service/link/qr";

            url += $"?link_request_id={_linkRequestID}";

            StartCoroutine(GetImage(
                url,
                onInternalError: onError,
                onResponse: (code, texture) => {
                    if (code == 200) {
                        onResponse?.Invoke(texture);
                    } else {
                        onError?.Invoke($"Error code {code}");
                    }
                }
            ));
        }

        public void GetLinkRequestStatus(Action<string>? onError = null, Action<FitcoinLinkRequestStatus?>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            var url = baseURL + "/service/link/status";
            url += $"?link_request_id={_linkRequestID}";

            StartCoroutine(Get(
                url,
                onInternalError: onError,
                onResponse: (code, data) => {
                    if (code == 200) {
                        var response = JsonConvert.DeserializeObject<FitcoinResponse<FitcoinLinkRequestStatus>>(data);
                        onResponse?.Invoke(response?.data);
                    } else {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message ?? "No error message";
                        onError?.Invoke(errorResponse);
                    }


                }
            ));
        }

        public void MonitorLinkRequestStatus(float queryInterval = 5f, Action<string>? onError = null, Action<FitcoinLinkRequestStatus?>? onResponse = null) {
            // TODO: Throw exception if access token is not granted

            StartCoroutine(_MonitorLinkRequestStatus(queryInterval, onError, onResponse));
        }

        public void StopMonitoringLinkRequestStatus() {
            _monitoringLinkRequest = false;
        }

        private IEnumerator _MonitorLinkRequestStatus(float queryInterval, Action<string>? onError = null, Action<FitcoinLinkRequestStatus?>? onResponse = null) {
            float timer = 0f;
            
            _monitoringLinkRequest = true;

            while (_monitoringLinkRequest) {
                if (timer < queryInterval) {
                    timer += Time.deltaTime;
                    yield return null;
                    continue;
                }

                GetLinkRequestStatus(
                    onError,
                    onResponse: (status) => {
                        onResponse?.Invoke(status);
                        timer = 0f;
                    }
                );

                while (timer >= queryInterval) {
                    yield return null;
                }
            }

            Debug.Log("Stop monitoring link request");
        }

        public void DeleteLinkRequest(Action<string>? onError = null, Action? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            if (_linkRequestID == null || _linkRequestID == "") {
                Debug.Log("Link request ID is empty so don't try to delete it");
                return;
            }
            
            var url = baseURL + "/service/link/clear";
            url += $"?link_request_id={_linkRequestID}";

            StartCoroutine(Post(
                url,
                new WWWForm(),
                onInternalError: onError,
                onResponse: (code, data) => {
                    _linkRequestID = null;

                    if (code == 200) {
                        onResponse?.Invoke();
                    } else {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message ?? "No error message";
                        onError?.Invoke(errorResponse);
                    }
                    
                }
            ));
        }

        public void GetUserInfo(Action<string>? onError = null, Action<FitcoinUserInfo?>? onResponse = null) {
            // TODO: Throw exception if access token is not specified

            var url = baseURL + "/service/user_info";
            url += $"?access_token={_accessToken}";
            url += $"&user_id={_userID}";

            StartCoroutine(Get(
                url,
                onInternalError: onError,
                onResponse: (code, data) => {
                    if (code == 200) {
                        _currentUserInfo = JsonConvert.DeserializeObject<FitcoinResponse<FitcoinUserInfo>>(data)?.data;
                        onResponse?.Invoke(_currentUserInfo);
                    } else {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message ?? "No error message";
                        _currentUserInfo = null;
                        onError?.Invoke(errorResponse);
                    }
                    onUserInfoUpdated?.Invoke(_currentUserInfo);
                }
            ));

        }

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