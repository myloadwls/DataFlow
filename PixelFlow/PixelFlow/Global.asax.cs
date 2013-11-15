using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Diagnostics;


namespace PixelFlow
{
    public class Global : System.Web.HttpApplication
    {

        private BatchBlock<Request> batch = new BatchBlock<Request>(8);

        private ActionBlock<Request[]> saveAction = new ActionBlock<Request[]>(r =>
        {
            foreach (var request in r)
            {
                Debug.WriteLine("This referer is: {0}", request.Referer);
            }    
        });

        public Global()
        {
            batch.LinkTo(saveAction);
        }

        public void SaveRequest(Request req)
        {
            batch.Post(req);
            Debug.WriteLine("Post");
        }

        protected void Application_End(object sender, EventArgs e)
        {
            batch.Complete(); // This might be a bad idea.
        }
    }
}