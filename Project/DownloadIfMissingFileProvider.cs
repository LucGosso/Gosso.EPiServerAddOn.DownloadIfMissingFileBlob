using EPiServer.Core;
using EPiServer.Framework.Blobs;
using EPiServer.Web;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Routing;

namespace Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
{

    public class Provider : FileBlobProvider
    {

        public const string DefaultUrl = "modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob/{UrlResolver}";
        //important to override with an "action" (that can be anything), not to cause it to be a default route so MVC will use it default when used with @Ajax.ActionLink and sometimes xforms action url
 
        public Provider()
            : this("[appDataPath]\\blobs")
        {
        }

        /// <summary>
        /// Create a new FileBlobProvider with path
        /// </summary>
        /// <param name="path"></param>
        public Provider(String path) :
            base(path)
        {
        }
        public override Blob GetBlob(Uri id)
        {
            FileBlob b = base.GetBlob(id) as FileBlob;
            if (HttpContext.Current != null && Activated)
            { //then not interested

                if (!checkIfProdServer()) // check so it is NOT the PRODUCTION server, lack if multidomain.
                {
                    if (!File.Exists(b.FilePath)) // check if exist on disc
                    {
                        FileInfo fi = new FileInfo(b.FilePath);
                        if (this.RestrictedFileExt.ToLower().IndexOf(fi.Extension.ToLower()) == -1) // check if download this fileextention
                        {

                            string guid = id.Segments[1].Replace("/", "");
                            try
                            {
                                string url = GetBlobUrl(guid); // get friendly url to file

                                if (!String.IsNullOrEmpty(url) && url.IndexOf("error") == -1) //error if not configured in prodserver, just hope the url does'nt consist of "error"
                                    DownloadAndSave(b, url);
                            }
                            catch (WebException)
                            {
                                //nada
                            }
                        }
                    }
                }
            }
            return b;
        }

        /// <summary>
        /// todo: lack of check if multis
        /// </summary>
        /// <returns></returns>
        private bool checkIfProdServer()
        {
            return HttpContext.Current.Request.Url.ToString().ToLower().Replace("http://", "https://").StartsWith(ProdUrl.ToLower().Replace("http://", "https://"));
        }

        private string GetBlobUrl(string guid)
        {
            WebClient webclient = new WebClient(); // using a webrequest to production server since the database is locked in the request and it is a possible chance of eternal loop
            return webclient.DownloadString(ProdUrl + UrlResolverUrl + "?" + guid);

        }

        private void DownloadAndSave(FileBlob blob, string rawurl)
        {

            WebRequest request = WebRequest.Create(ProdUrl + rawurl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK) //yeah, sometimes not published or deleted, or wrong url //todo: display default image?
            {
                Stream dataStream = response.GetResponseStream();
                blob.Write(dataStream); //thats it
            }

        }


        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <param name="name">name of provider</param>
        /// <param name="config">provider settings</param>
        public override void Initialize(string name, NameValueCollection config)
        {

            if (config.Get("path") != null)
            {
                Path = VirtualPathUtilityEx.RebasePhysicalPath(config.Get("path"));
            }

            if (config.Get("Activated") != null)
            {
                Activated = bool.Parse(config.Get("Activated").ToLower());
            }
            else
                Activated = false;


            if (config.Get("UrlResolverUrl") != null)
            {
                UrlResolverUrl = config.Get("UrlResolverUrl");
            }
            else
                UrlResolverUrl = DefaultUrl.Replace("{UrlResolver}","urlresolver.ashx");

            if (Activated)
            {
                if (config.Get("ProdUrl") != null)
                {
                    ProdUrl = config.Get("ProdUrl");
                }
                else
                    EPiServer.Framework.Validator.ThrowIfNullOrEmpty("ProdUrl", ProdUrl);

                if (config.Get("RestrictedFileExt") != null)
                {
                    RestrictedFileExt = config.Get("RestrictedFileExt");
                }
            }

            //
            base.Initialize(name, config);
        }

        /// <summary>
        /// Path to blob repository, default is "[appDataPath]\\blobs"
        /// </summary>
        public new string Path
        {
            get;
            internal set;
        }

        /// <summary>
        /// Url to Production Server where blob should be downloaded
        /// </summary>
        public string ProdUrl
        {
            get;
            internal set;
        }

        /// <summary>
        /// Absolute Url to UrlResolver.ashx on Production Server, default is Provider.DefaultUrl = "modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob/UrlResolver.ashx";
        /// </summary>
        public string UrlResolverUrl
        {
            get;
            internal set;
        }

        /// <summary>
        /// If the AddOn is activated or not.
        /// </summary>
        public bool Activated
        {
            get;
            set;
        }


        /// <summary>
        /// Restiction to fileextentions NOT do be downloaded. eg ".doc.docx.html.exe"
        /// </summary>
        public string RestrictedFileExt
        {
            get;
            internal set;
        }

    }
}
