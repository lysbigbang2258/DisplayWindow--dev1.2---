using System.Net;

namespace ArrayDisplay.net {
    public struct StreamDataPack {
        public IPEndPoint Ip { get; set; }
        public byte[] DataBytes { get;set; }
    }
}
