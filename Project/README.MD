# FileBlob Provider AddOn for EPiServer Developers
**Namespace: Gosso.EPiServerAddOn.DownloadIfMissingFileBlob**

**Applicable to CMS >7.5 (MVC or Webforms) - tested with CMS9.6.1**

## Why?
Ever restored the Production database to your developer environment and got a website without images? This lightweight AddOn keeps your local environment blob directory up to date.
## What is it?
This AddOn provider copies (when page loads) the file blobs from production to your local environment, or to your staging/test environment.
## For whom?
This AddOn is heaven for developers, get rid of copying the production blobs to your local installations.
## How it works
When the EPiServer CMS page is binding the model to the page, the Provider GetBlob(URI id) is called for every Blob used on the loaded page. The Download If Missing File Blob Provider check if file exists on the local blob directory, if not it requests the Gosso.EpiserverAddOn.DownloadIfMissingFileBlob.UrlResolver.ashx* on the Production server and downloads the file with the friendly URL.

*this ashx must exist on the production server

*The DownloadIfMissingFileBlobProvider is using a web request to the ashx on the production server since the local database is locked (because of possible chance of eternal loop) during request and we don’t have the possibility to find out the friendly URL to the file.

## Performance overload
Yes, initially on application load, it will take some time to download the loaded files.
## Configuring

**UrlResolverUrl** is needed to get the friendly relative url.

**ProdUrl** is pointing to the production server and appends the friendly relative url when downloading the files. For example  ProdUrl + “globalassets/images/folder/name.png”

**RestrictedFileExt** helps to restrict which files to not download

**Path** to the blob url (default "[appDataPath]\\blobs")

```
<episerver.framework>
<blob defaultProvider="MissingFileBlobProvider">
<providers>
<add name="MissingFileBlobProvider"
ProdUrl="http://www.gosso.se/"
UrlResolverUrl="http://www.gosso.se/modules/Gosso.EpiserverAddOn.DownloadIfMissingFileBlob/urlresolver.ashx"
RestrictedFileExt=".docx.doc.pdf.exe.zip.mov.mp4"
type="Gosso.EpiserverAddOn.DownloadIfMissingFileBlob.Provider, Gosso.EpiserverAddOn.DownloadIfMissingFileBlob" />
</providers>
</blob>
</episerver.framework>
```

## Important/troubleshooting 
1.	You will need release the addon on the production server (dll and urlresolver.ashx) before you give it a try.
  - The easy way, drop the urlResolver.ashx anywhere on prodserver, also put the addon dll in bin-folder
2.	Do not configure the provider in web.config <episerver.framework>
    (or episerverframwork.config) on the production server (even though we have a smaller built in check if it is in production to prevent loops.)
3.	The production server must be active/public reachable
4.	The files must be public reachable thru a public url

## Installation
Under the release tab you may download the nuget package to your local feed for installation with package manager console in Visual Studio. It will install two file, the Gosso.EPiServerAddOn.MissingFileBlobProvider.dll into the bin folder, and the UrlResolver.ashx, installation path /module/Gosso.EpiserverAddon.MissingFileBlobProvider/. Also configure episerverframework.config with the MissingFileBlob.

You can also download the source code project and add it to your solution, therefore you may easily debug it if needed.




