using System;
using Newtonsoft.Json;

namespace Fitcoin {

    public class FitcoinResponseSimple {
        public string message;
    }
    
    public class FitcoinResponse<T> {
        public string message;
        public T data;
    }

    public class FitcoinLinkRequestStatus {
        public DateTime creation_date;
        public string status;
        public string user_id = null;
    }
}