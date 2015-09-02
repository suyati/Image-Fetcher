# Image-Fetcher
[![Travis](https://travis-ci.org/suyati/Image-Fetcher.svg?branch=master)](https://travis-ci.org/suyati/Image-Fetcher)
[![NuGet](http://img.shields.io/nuget/v/ImageFetcher.svg)](https://www.nuget.org/packages/ImageFetcher/)
[![Downloads](http://img.shields.io/nuget/dt/ImageFetcher.svg)](https://www.nuget.org/packages/ImageFetcher/)

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

##Authors and Contributors
XML Extractor is developed by Suyati Technologies. It is written and maintained by their Product Development team.

####Author:

Anoop Malayil ([@t4anoop](https://twitter.com/t4anoop))

##Support or Contact
Have Suggestions? Want to give us something to do? Contact us at : support@suyati.com
