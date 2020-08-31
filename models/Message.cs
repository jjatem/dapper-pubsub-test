using System.Collections.Generic;

namespace dapper_pubsub_test.Models
{
    public class Message
    {
        public string productCode { get; set; }
        public int accountConfigId { get; set; }
        public List<int> accountCollection { get; set; }

    }
}