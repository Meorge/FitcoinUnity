using System;
using Newtonsoft.Json;

namespace Fitcoin {

    /// <summary>
    /// A response from the Fitcoin server which consists of only a string message.
    /// Common when server-side errors occur.
    /// </summary>
    public class FitcoinResponseSimple {
        /// <summary>
        /// The message associated with the response.
        /// </summary>
        public string message;
    }
    
    /// <summary>
    /// A response from the Fitcoin server which consists of a string message and
    /// corresponding data.
    /// </summary>
    /// <typeparam name="T">The data type of the data in the response.</typeparam>
    public class FitcoinResponse<T> {
        /// <summary>
        /// The message associated with the response.
        /// </summary>
        public string message;

        /// <summary>
        /// The data associated with the response.
        /// </summary>
        public T data;
    }

    /// <summary>
    /// The status of a Fitcoin link request.
    /// </summary>
    public class FitcoinLinkRequestStatus {
        /// <summary>
        /// The date and time at which this link request was created.
        /// </summary>
        public DateTime creation_date;

        /// <summary>
        /// The current status of this link request. Potential values may include:
        /// <list>
        /// <item><description><c>"pending"</c> - The user has not yet approved nor denied
        /// this link request.</description></item>
        /// <item><description><c>"approved"</c> - The user has approved this link request.
        /// If this is the value of <c>status</c>, <c>user_id</c> will hold the ID of
        /// the user who approved the link request, suitable for assigning to 
        /// <c>FitcoinService.UserID</c>.</description></item>
        /// <item><description><c>"denied"</c> - The user has denied this link request.
        /// </description></item>
        /// </list>
        /// </summary>
        public string status;

        /// <summary>
        /// The ID of the user who approved the link request, if <c>status</c> is <c>"approved"</c>.
        /// Otherwise, this will be <c>null</c>.
        /// </summary>
        public string user_id = null;
    }

    /// <summary>
    /// Information about a Fitcoin user.
    /// </summary>
    public class FitcoinUserInfo {
        /// <summary>
        /// The username of this user.
        /// </summary>
        public string username;

        /// <summary>
        /// The current Fitcoin balance for this user.
        /// </summary>
        public int balance;
    }
}