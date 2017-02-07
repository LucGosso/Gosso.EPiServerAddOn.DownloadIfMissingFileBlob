# FileBlob Provider AddOn for EPiServer Developers
**Namespace: Gosso.EPiServerAddOn.DownloadIfMissingFileBlob** Version 1.3 (2017-02-04)

[![Platform](https://img.shields.io/badge/Episerver-%207.6+-orange.svg?style=flat)](http://world.episerver.com/cms/) [![Platform](https://img.shields.io/badge/Episerver-%2010.0-green.svg?style=flat)](http://world.episerver.com/cms/)

**Applicable to CMS >7.5 (MVC or Webforms) - tested with CMS9.6.1 and compiled with CMS 10.0.1.0** 

**NOT TESTED with AZURE File Storage or with ImageVault**

## Why?
Ever restored the Production database to your developer environment and got a website without images? This lightweight AddOn keeps your local environment blob directory up to date.
## What is it?
This AddOn provider copies (when page loads) the file blobs from production to your local environment, or to your staging/test environment.
## For whom?
This AddOn is heaven for developers, get rid of copying the production blobs to your local installations.
## How it works
When the EPiServer CMS page is binding the model to the page, the Provider GetBlob(URI id) is called for every Blob used on the loaded page. The Download If Missing File Blob Provider check if file exists on the local blob directory, if not it requests the Gosso.EpiserverAddOn.DownloadIfMissingFileBlob.UrlResolver.ashx* on the Production server and downloads the file with the friendly URL.

*this ashx webhandler is automaticly added on init

*The DownloadIfMissingFileBlobProvider is using a web request to the ashx on the production server since the local database is locked (because of possible risk of eternal loop) during request and we don’t have the possibility to find out the friendly URL to the file.

## Performance overload
Yes, initially on application load, it will take some time to download the loaded files.
## Configuring

**Activated** is needed to get it working. Set to true on DEV, och false in PROD.

**ProdUrl** is pointing to the production server and appends the friendly relative url when downloading the files. For example  ProdUrl + “globalassets/images/folder/name.png”

**UrlResolverUrl** is NOT mandatory, can be empty, it is used to get the friendly relative url of the blob.

**RestrictedFileExt** helps to restrict which files to not download

**Path** to the blob url (default "[appDataPath]\\blobs")

```
<episerver.framework>
	<blob defaultProvider="MissingFileBlobProvider">
		<providers>
			<add name="MissingFileBlobProvider"
			    Activated="true"
				ProdUrl="http://www.gosso.se/"
				UrlResolverUrl="modules/Gosso.EpiserverAddOn.DownloadIfMissingFileBlob/urlresolver.ashx"
				RestrictedFileExt=".docx.doc.pdf.exe.zip.mov.mp4"
				type="Gosso.EpiserverAddOn.DownloadIfMissingFileBlob.Provider, Gosso.EpiserverAddOn.DownloadIfMissingFileBlob" />
		</providers>
	</blob>
</episerver.framework>
```

## Important/troubleshooting 
1.	You will need release the addon (dll) on the production server before you give it a try.
  - The easy way, drop the dll in bin-folder!
2.	Do not configure the provider in web.config <episerver.framework>
    (or episerverframwork.config) on the production server (even though we have a smaller built in check if it is in production to prevent loops.) OR you may set Activated=false
3.	The production server must be active/public reachable
4.	The files must be public reachable thru a public url

## Installation
Under the release tab you may download the nuget package to your local feed for installation with package manager console in Visual Studio. It will install one file, the Gosso.EPiServerAddOn.MissingFileBlobProvider.dll into the bin folder. Also configure episerverframework.config/web.config with the MissingFileBlob.

In Package Manager Console i recommend 'install-package Gosso.EPiServerAddOn.DownloadIfMissingFileBlobProvider **-IgnoreDependencies**' if you have compatibility problems. It is dependent to EPiServer.

You can also download the source code project and add it to your solution, therefore you may easily debug it if needed.

## Customization

If you want to change the default URLResolver path, you may use this web.config and change the **UrlResolverUrl**
Also put the file urlresolver.ashx at that place
```
  <location path="modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob">
    <system.webServer>
      <handlers>
        <add name="DownloadIfMissingFileBlob" path="/modules/Gosso.EPiServerAddOn.DownloadIfMissingFileBlob/UrlResolver.ashx" verb="GET" type="Gosso.EPiServerAddOn.DownloadIfMissingFileBlob.UrlResolverHelper, Gosso.EpiserverAddOn.DownloadIfMissingFileBlob" />
      </handlers>
    </system.webServer>
  </location>
```

**If you have lower version then 10 add assemblyBindings:**

        <dependentAssembly>
        <assemblyIdentity name="EPiServer" publicKeyToken="8fe83dea738b45b7" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-10.0.1.0" newVersion="your version" />
        </dependentAssembly>

        <dependentAssembly>
        <assemblyIdentity name="EPiServer.Framework" publicKeyToken="8fe83dea738b45b7" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-10.0.1.0" newVersion="your version" />
        </dependentAssembly>

