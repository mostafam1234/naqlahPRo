using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class NotificationBodyForMultipleDevices
    {
        public NotificationBodyForMultipleDevices()
        {
            this.Title = string.Empty;
            this.Body = string.Empty;
            this.FireBaseTokens = new List<string>();
            this.PayLoad = new Dictionary<string, string>();
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> PayLoad { get; set; }
        public List<string> FireBaseTokens { get; set; }
    }
}
