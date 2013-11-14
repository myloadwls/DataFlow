using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace PixelFlow
{
    public class Global : System.Web.HttpApplication
    {

        BatchBlock<Request> batch = new BatchBlock<Request>(10);

        ActionBlock<Request[]> saveAction = new ActionBlock<Request[]>(r =>
        { 
            //Save the batch    
        });

        protected void Application_Start(object sender, EventArgs e)
        {
            batch.LinkTo(saveAction);
        }
        
        protected void Application_End(object sender, EventArgs e)
        {
            batch.Complete();
        }
    }

    
}