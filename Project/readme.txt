## Remember the configuration

You will need to release the addon (dll) on the production server before you give it a try.
  - The easy way, drop the dll in bin-folder on prodserver. It won't affect more than an application restart. Assembly bindings (EPiServer & EPiServer.Framework) in web.config must incorp version 10.0.1.0.
  - You may also put it in the modulesbin folder and run IISRESET. Assembly bindings (EPiServer & EPiServer.Framework) in web.config must incorp version 10.0.1.0.

Check out web.config <episerver.framework><blob><providers> 
1. Change **ProdUrl** to your public production server address.
2. Change **Activated** to true in dev.

Enjoy!  

More information https://github.com/Lucstar/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob
