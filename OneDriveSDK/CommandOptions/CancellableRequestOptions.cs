using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDrive
{
    public class CancellableRequestOptions : RequestOptions
    {
        /// <summary>
        /// Allows the request to be cancelled before a response is returned.
        /// </summary>
        public System.Threading.CancellationToken CancelToken { get; set; }

        /// <summary>
        /// Provide a delegate that can be called data is transfered.
        /// </summary>
        public ProgressReportDelegate ProgressDelegate { get; set; }
    }
}
