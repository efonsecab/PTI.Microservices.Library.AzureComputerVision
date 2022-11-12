using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTI.Microservices.Library.Models.AzureComputerVision.DescribeImage
{

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DescribeImageResponse
    {
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Description
    {
        public string[] tags { get; set; }
        public Caption[] captions { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public float confidence { get; set; }
    }

    public class Metadata
    {
        public int height { get; set; }
        public int width { get; set; }
        public string format { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
