## Remember the configuration

You will need to release the addon (dll) on the production server before you give it a try.
  - The easy way, drop the dll in bin-folder on prodserver.

Check out web.config <episerver.framework><blob><providers> 
1. Change **ProdUrl** to your public production server address.
2. Change **Activated** to true in dev.

Enjoy!  

More information https://github.com/Lucstar/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
