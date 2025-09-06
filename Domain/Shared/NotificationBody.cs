using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Shared
{
    public class NotificationBody
    {
        public NotificationBody()
        {
            this.Title = string.Empty;
            this.Body = string.Empty;
            this.FireBaseToken = string.Empty;
            this.PayLoad = new Dictionary<string, string>();
        }
        public string Title { get; set; }
        public string Body { get; set; }
        public Dictionary<string, string> PayLoad { get; set; }
        public string FireBaseToken { get; set; }
    }
}
