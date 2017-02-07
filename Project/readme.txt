## Remember the configuration

You will need to release the addon (dll) on the production server before you give it a try.
  - You may put it in the ~/modulesbin folder and run IISRESET. Recommended.
  - (OR drop the dll in bin-folder on prodserver. It won't affect more than an application restart)

Check out web.config <episerver.framework><blob><providers> 
1. Change **ProdUrl** to your public production server address.
2. Change **Activated** to true in dev.

Enjoy!  

More information https://github.com/LucGosso/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
