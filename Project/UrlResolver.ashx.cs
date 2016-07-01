using EPiServer.Core;
using EPiServer.Web;
using EPiServer.Web.Routing;
using System;
using System.Web;
using System.Web.Routing;

namespace Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
{
    /// <summary>
    ///  Returning the friendly url as string, used by the DownloadIfMissingFileBlob Provider.
    /// </summary>
    public class UrlResolverHelper : IHttpHandler, IRouteHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string guid = context.Request.QueryString.ToString();

            if (!string.IsNullOrEmpty(guid))
            {

                Guid g = new Guid(guid);
                ContentReference cr = PermanentLinkUtility.FindContentReference(g);
                string url = UrlResolver.Current.GetUrl(cr);
                context.Response.Write(url);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return this;
        }
    }
}
