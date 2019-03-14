using EPiServer.Framework.Blobs;
using EPiServer.Web;
using log4net;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
{

    public class Provider : FileBlobProvider
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string DefaultUrl = "modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob/{UrlResolver}";
        //important to override with an "action" (that can be anything), not to cause it to be a default route so MVC will use it default when used with @Ajax.ActionLink and sometimes xforms action url

        private HttpClient _httpClient;

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
            if (HttpContext.Current != null && Activated && b != null)
            {
                //then not interested

                if (!CheckIfProdServer()) // check so it is NOT the PRODUCTION server, lack if multidomain.
                {
                    if (!File.Exists(b.FilePath)) // check if exist on disc
                    {
                        FileInfo fi = new FileInfo(b.FilePath);
                        if (this.RestrictedFileExt.ToLower()
                                .IndexOf(fi.Extension.ToLower(), StringComparison.Ordinal) ==
                            -1) // check if download this fileextention
                        {

                            string guid = id.Segments[1].Replace("/", "");
                            try
                            {
                                string url = GetUrlAsync(guid).Result; // get friendly url to file

                                if (!String.IsNullOrEmpty(url) &&
                                    url.IndexOf("error", StringComparison.OrdinalIgnoreCase) == -1)
                                {
                                    string filename = fi.Name;
                                    int intIsSmallImage =
                                        filename.LastIndexOf("_",
                                            StringComparison.Ordinal); //check if a image thumbnail or variation
                                    if (intIsSmallImage > 0)
                                    {
                                        url += "/" + filename.ToLower()
                                                   .Substring(intIsSmallImage + 1,
                                                       filename.Length - intIsSmallImage - 1)
                                                   .Replace(fi.Extension.ToLower(), "");
                                    }
                                    Task.Run(() => DownloadAndSave(b, url));
                                }
                            }
                            catch (WebException ee)
                            {
                                //nada
                                Logger.Error("BlobFile could not be downloaded: " + id, ee);
                            }
                        }
                        else
                        {
                            Logger.Debug(
                                $"BlobFile type restriction to \"{this.RestrictedFileExt}\", file not downloaded: " +
                                id);
                        }
                    }
                }
                else
                {
                    Logger.Error(
                        $"Check configuration web.config, seems like the blob module {this.Name} is activated in production. Activated should be false!");
                }
            }
            return b;
        }

        /// <summary>
        /// todo: lack of check if multis
        /// </summary>
        /// <returns></returns>
        private bool CheckIfProdServer()
        {
            return HttpContext.Current.Request.Url.ToString().ToLower().Replace("http://", "https://")
                .StartsWith(ProdUrl.ToLower().Replace("http://", "https://"));
        }

        private async Task<string> GetUrlAsync(string guid)
        {
            var html = await _httpClient.GetAsync(ProdUrl + UrlResolverUrl + "?" + guid).Result.Content.ReadAsStringAsync();
            return html;
        }

        private async void DownloadAndSave(FileBlob blob, string rawurl)
        {   
            var response = await _httpClient.GetAsync(ProdUrl + rawurl);
            if (response.StatusCode == HttpStatusCode.OK
            ) //yeah, sometimes not published or deleted, or wrong url //todo: display default image? no, it may be a temporary error or internet is offline
            {
                Stream dataStream = await response.Content.ReadAsStreamAsync();
                blob.Write(dataStream); //thats it
                Logger.Debug("BlobFile downloaded: " + ProdUrl + rawurl);
            }
            else
            {
                Logger.Error("BlobFile could not be downloaded: " + ProdUrl + rawurl);
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
                UrlResolverUrl = config.Get("UrlResolverUrl").ToLower();
            }
            else
                UrlResolverUrl = DefaultUrl.Replace("{UrlResolver}", "urlresolver.ashx").ToLower();

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

            // Setup the shared httpClient
            var cookieContainer = new CookieContainer();            
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            _httpClient = new HttpClient(handler);

            if (config.Get("Cookies") != null)
            {
                var cookies = config.Get("Cookies").Split(';');

                foreach (var cookie in cookies)
                {
                    var splitCookie = cookie.Split('=');
                    if (splitCookie.Length == 2)
                    {
                        cookieContainer.Add(new Uri(ProdUrl), new Cookie(splitCookie[0], splitCookie[1]));
                        Logger.Debug($"Added Cookie {splitCookie[0]} with value {splitCookie[1]} to requests");
                    }
                }
            }
            
            base.Initialize(name, config);
        }

        /// <summary>
        /// Path to blob repository, default is "[appDataPath]\\blobs"
        /// </summary>
        public new string Path { get; internal set; }

        /// <summary>
        /// Url to Production Server where blob should be downloaded
        /// </summary>
        public string ProdUrl { get; internal set; }

        /// <summary>
        /// Absolute Url to UrlResolver.ashx on Production Server, default is Provider.DefaultUrl = "modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob/UrlResolver.ashx";
        /// </summary>
        public string UrlResolverUrl { get; internal set; }

        /// <summary>
        /// If the AddOn is activated or not.
        /// </summary>
        public bool Activated { get; set; }


        /// <summary>
        /// Restiction to fileextentions NOT do be downloaded. eg ".doc.docx.html.exe"
        /// </summary>
        public string RestrictedFileExt { get; internal set; }

    }
}
