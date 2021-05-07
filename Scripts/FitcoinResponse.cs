using Newtonsoft.Json;

namespace Fitcoin {

    public class FitcoinResponseSimple {
        public string message;
    }
    
    public class FitcoinResponse<T> {
        public string message;
        public T data;
    }
}