# Image-Fetcher
Fetching images from a particular url

This nuget package will help you to fetch images (JPEG and PNG) from a given url .You can configure the image type and maximum number of images.

##Installation
To install Image Fetcher, run the following command in the Package Manager Console

```sh
PM> Install-Package ImageFetcher
```

##Quick Start
It is recommended that you install ImageFetcher via NuGet.

Add a reference to ImageFetcher.dll

1)	Create a instance for ImageFetcher

```cs
ImageFetcher imageFetcher = new ImageFetcher();
```

2)	Call GeImages() method for fetching images from a given url

```cs
var imagesUrls = imageFetcher.GetImages("www.sampleweburl.com”);
```

3)	Parameters

JPEG: Boolean (by default true), set true for fetching only JPEG images  
PNG:  Boolean (by default false), set true for fetching only PNG images             
OgImage: Boolean (by default false) ,set true for fetching Og Image                   
MaxImageCount:  Integer (by default 2), set the maximum number of images needs to fetched.
