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

        /// <summary>
        /// The service access token to use for querying the server. If this is not set,
        /// <c cref="AccessTokenNotProvidedException">AccessTokenNotProvidedException</c> will be thrown
        /// on server function calls.
        /// </summary>
        public string? AccessToken {
            get {
                return _accessToken;
            }

            set {
                _accessToken = value;
            }
        }

        /// <summary>
        /// The ID of the current active user. If this is not set,
        /// <c cref="UserIDNotProvidedException">UserIDNotProvidedException</c> will be thrown on server
        /// function calls that involve a specific user.
        /// </summary>
        public string? UserID {
            get {
                return _userID;
            }

            set {
                _userID = value;
            }
        }

        /// <summary>
        /// The ID of the current active link request. If this is not set,
        /// <c cref="NoActiveLinkRequestException">NoActiveLinkRequestException</c> will be thrown on server
        /// function calls that involve a specific link request.
        /// </summary>
        public string? LinkRequestID {
            get {
                return _linkRequestID;
            }

            set {
                _linkRequestID = value;
            }
        }

        /// <summary>
        /// Returns whether or not a link request is currently being monitored.
        /// </summary>
        public bool MonitoringLinkRequest {
            get {
                return _monitoringLinkRequest;
            }
        }

        /// <summary>
        /// Returns the last retrieved active user information.
        /// You may want to call <c cref="GetUserInfo">GetUserInfo</c> before getting this value.
        /// </summary>
        public FitcoinUserInfo? CurrentUserInfo {
            get {
                return _currentUserInfo;
            }
        }

        /// <summary>
        /// A delegate type that takes in the new information for a Fitcoin user.
        /// </summary>
        /// <param name="newInfo">The current user information (null if there is
        /// no current user).</param>
        public delegate void FitcoinUserInfoUpdateHandler(FitcoinUserInfo? newInfo);

        /// <summary>
        /// Fired whenever the current active user information is updated, passing
        /// the <c cref="FitcoinUserInfo"> as an argument.</c>
        /// </summary>
        public event FitcoinUserInfoUpdateHandler? onUserInfoUpdated;

        private string? _accessToken = null;
        private string? _userID = null;

        private string? _linkRequestID = null;

        private bool _monitoringLinkRequest = false;

        private FitcoinUserInfo? _currentUserInfo = null;


        /// <summary>
        /// Creates a new Fitcoin link request. If successful, the ID of the link request is passed to the
        /// <c>onResponse</c> callback function.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the link request has been created. The ID of
        /// the link request is passed into the callback function.</param>
        public void CreateLinkRequest(Action<string>? onError = null, Action<string?>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

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

        /// <summary>
        /// Retrieves a QR code for the current active link request. If successful, the <c>Texture</c> data for the
        /// QR code is passed to the <c>onResponse</c> callback function.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="NoActiveLinkRequestException"></exception>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the QR code has been generated. The <c>Texture</c>
        /// data of the QR code is passed into the callback function.</param>
        public void GetQRCodeForLinkRequest(Action<string>? onError = null, Action<Texture>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if there is no active link request
            if (_linkRequestID == null)
                throw new NoActiveLinkRequestException();

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

        /// <summary>
        /// Retrieves the current status of the current active link request. If successful,
        /// the status of the link request, as a <c>FitcoinLinkRequestStatus</c> object, is
        /// passed to the <c>onResponse</c> callback function.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="NoActiveLinkRequestException"></exception>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the link request data has been retrieved. An instance
        /// of <c>FitcoinLinkRequestStatus</c> is passed into the callback function.</param>
        public void GetLinkRequestStatus(Action<string>? onError = null, Action<FitcoinLinkRequestStatus?>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if there is no active link request
            if (_linkRequestID == null)
                throw new NoActiveLinkRequestException();
                
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


        /// <summary>
        /// Begins periodically querying the Fitcoin server to check the status of the current active link request, at
        /// the interval provided (default of 5 seconds). Every time the server is queried, either the
        /// <paramref>onError</paramref> or <paramref>onResponse</paramref> callback will be executed.
        /// NOTE: Once started, the server will continue to be queried until <c>StopMonitoringLinkRequestStatus()</c> is
        /// called. An error or success response will NOT stop the querying.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="NoActiveLinkRequestException"></exception>
        /// <param name="queryInterval">The amount of time to wait between queries to the server.</param>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the link request data has been retrieved. An instance
        /// of <c>FitcoinLinkRequestStatus</c> is passed into the callback function.</param>
        public void MonitorLinkRequestStatus(float queryInterval = 5f, Action<string>? onError = null, Action<FitcoinLinkRequestStatus?>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if there is no active link request
            if (_linkRequestID == null)
                throw new NoActiveLinkRequestException();

            StartCoroutine(_MonitorLinkRequestStatus(queryInterval, onError, onResponse));
        }

        /// <summary>
        /// Stops monitoring the current active link request.
        /// </summary>
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
                    if (!_monitoringLinkRequest) break;
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
        }

        /// <summary>
        /// Deletes the current active link request. Use this once you are done with a link request (i.e.,
        /// when it has been approved or denied by the user.)
        /// 
        /// If successful, the <c>onResponse</c> callback function is called with no arguments.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="NoActiveLinkRequestException"></exception>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the link request has been successfully deleted.
        /// </param>
        public void DeleteLinkRequest(Action<string>? onError = null, Action? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if there is no active link request
            if (_linkRequestID == null)
                throw new NoActiveLinkRequestException();
            
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

        /// <summary>
        /// Updates current user information without any callbacks.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="UserIDNotProvidedException"></exception>
        public void UpdateUserInfo() => GetUserInfo(null, null);

        /// <summary>
        /// Updates current user information. If successful, the updated user information, as
        /// a <c>FitcoinUserInfo</c> object, is passed to the <c>onResponse</c> callback function.
        /// Additionally, the <c>CurrentUserInfo</c> property is updated with the updated information.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="UserIDNotProvidedException"></exception>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when the current user information has been retrieved.
        /// An instance of <c>FitcoinUserInfo</c> is passed into the callback function.</param>
        public void GetUserInfo(Action<string>? onError = null, Action<FitcoinUserInfo?>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if user ID is not specified
            if (_userID == null)
                throw new UserIDNotProvidedException();

            var url = baseURL + "/service/user_info";
            url += $"?access_token={_accessToken}";
            url += $"&user_id={_userID}";

            StartCoroutine(Get(
                url,
                onInternalError: (message) => {
                    _currentUserInfo = null;
                    onUserInfoUpdated?.Invoke(_currentUserInfo);
                    onError?.Invoke(message);
                },
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

        /// <summary>
        /// Attempts to make a Fitcoin purchase on behalf of the current linked user. If successful, the user's
        /// new balance is passed to the <c>onResponse</c> callback function.
        /// </summary>
        /// <exception cref="AccessTokenNotProvidedException"></exception>
        /// <exception cref="UserIDNotProvidedException"></exception>
        /// <param name="amount">The cost of the purchase. This must be a non-negative integer.</param>
        /// <param name="onError">A callback to execute when an error occurs. A string describing the error
        /// is passed into the callback function.</param>
        /// <param name="onResponse">A callback to execute when purchase has completed successfully.
        /// The user's new balance is passed into the callback function.</param>
        public void MakePurchase(int amount, Action<string>? onError = null, Action<int>? onResponse = null) {
            // Throw exception if access token is not specified
            if (_accessToken == null)
                throw new AccessTokenNotProvidedException();

            // Throw exception if user ID is not specified
            if (_userID == null)
                throw new UserIDNotProvidedException();

            var url = baseURL + "/service/purchase";
            url += $"?access_token={_accessToken}";
            url += $"&user_id={_userID}";
            url += $"&amount={amount}";

            StartCoroutine(Post(
                url,
                new WWWForm(),
                onInternalError: (message) => {
                    onError?.Invoke(message);
                },
                onResponse: (code, data) => {
                    if (code == 200) {
                        var newBalance = JsonConvert.DeserializeObject<FitcoinResponse<int>>(data)?.data ?? 0;
                        onResponse?.Invoke(newBalance);
                    } else {
                        var errorResponse = JsonConvert.DeserializeObject<FitcoinResponseSimple>(data)?.message ?? "No error message";
                        onError?.Invoke(errorResponse);
                    }
                }
            ));
        }



        private IEnumerator Post(string uri, WWWForm formData, Action<string>? onInternalError, Action<long, string>? onResponse) {
            var request = UnityWebRequest.Post(uri, formData);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError)
                onInternalError?.Invoke(request.error);
            else
                onResponse?.Invoke(request.responseCode, request.downloadHandler.text);
        }

        private IEnumerator Get(string uri, Action<string>? onInternalError, Action<long, string>? onResponse) {
            var request = UnityWebRequest.Get(uri);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError) {
                onInternalError?.Invoke(request.error);
            }
            else {
                onResponse?.Invoke(request.responseCode, request.downloadHandler.text);
            }
        }

        private IEnumerator GetImage(string uri, Action<string>? onInternalError, Action<long, Texture>? onResponse) {
            var request = UnityWebRequestTexture.GetTexture(uri);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.DataProcessingError) {
                onInternalError?.Invoke(request.error);
            }
            else {
                Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                onResponse?.Invoke(request.responseCode, texture);
            }
        }
    }

    /// <summary>
    /// Thrown when a <c>FitcoinService</c> object attempts to interact with the server, but does not have
    /// a service access token set (via the property <c>FitcoinService.AccessToken</c>).
    /// </summary>
    public class AccessTokenNotProvidedException : Exception {}

    /// <summary>
    /// Thrown when a <c>FitcoinService</c> object attempts to perform operations involving an active user,
    /// but no active user ID is set (via the property <c>FitcoinService.UserID</c>).
    /// </summary>
    public class UserIDNotProvidedException : Exception {}

    /// <summary>
    /// Thrown when a <c>FitcoinService</c> object attempts to perform operations involving an active link
    /// request, but no active link request exists.
    /// </summary>
    public class NoActiveLinkRequestException : Exception {}
}