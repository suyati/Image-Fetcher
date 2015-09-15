using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
namespace Suyati.ImageFetcher
{
    /// <summary>
    /// The Image Fetcher
    /// </summary>
    public class ImageFetcher
    {

        #region Public Methods

        /// <summary>
        /// To Get  Images from a URL
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Jpeg"></param>
        /// <param name="Png"></param>
        /// <param name="OgImage"></param>
        /// <param name="MaxImageCount"></param>
        /// <returns></returns>
        public IList<string> GetImages(string Url, bool Jpeg = true, bool Png = false, bool OgImage = false, int MaxImageCount = 2)
        {

            // Add http to a non-http url
            Url = AddHttpToUrl(Url);

            if (!IsValidUrl(Url))
            {
                throw new ArgumentException("Invalid Url");
            }

            var client = new RestClient { BaseUrl = new System.Uri(Url) };

            var request = new RestRequest { DateFormat = DataFormat.Xml.ToString(), Method = Method.GET };

            //ServicePointManager.Expect100Continue = true;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Rest response for the given url
            IRestResponse response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new WebException(response.ErrorMessage, response.ErrorException);
            }


            // response content is loaded into HtmlDocument
            var content = response.Content;
            var byteOrderMark = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            // Removing Byte Order Mark from Content
            if (content.StartsWith(byteOrderMark))
            {
                content = content.Replace(byteOrderMark, string.Empty);
            }
            HtmlDocument webhtml = new HtmlDocument();
            webhtml.LoadHtml(content);

            string imageUrl = string.Empty;
            ISet<string> imageUrls = new HashSet<string>();
            Func<string, bool> predicate = null;
            if (Png && Jpeg)
            {
                predicate = new Func<string, bool>(url => url.IndexOf(".jpg", StringComparison.InvariantCultureIgnoreCase) != -1 || url.IndexOf(".png", StringComparison.InvariantCultureIgnoreCase) != -1);
            }
            else if (Png)
            {
                predicate = new Func<string, bool>(url => url.IndexOf(".png", StringComparison.InvariantCultureIgnoreCase) != -1);
            }
            else if (Jpeg)
            {
                predicate = new Func<string, bool>(url => url.IndexOf(".jpg", StringComparison.InvariantCultureIgnoreCase) != -1);
            }

            if (predicate != null)
            {
                imageUrls = GetTags(content, "img");
            }
            //Fetching og image 
            if (OgImage)
            {
                imageUrl = GetOGImageUrl(webhtml, response.ResponseUri);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    imageUrls.Add(imageUrl);
                }
            }
            var imageUrlList = imageUrls.Where(predicate).Take(MaxImageCount).ToList();
            // Resolving relative Urls 
            for (int i = 0; i < imageUrlList.Count; i++)
            {
                imageUrlList[i] = CompleteRelativeUrl(imageUrlList[i], response.ResponseUri);
            }

            return imageUrlList;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// To check whether the url is valid or not
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>        
        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            if (!url.StartsWith(@"http://") && !url.StartsWith(@"https://"))
            {
                return false;
            }
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        public ISet<string> GetTags(string content, string tagName)
        {
            ISet<string> matches = new HashSet<string>();
            var regex = "<img[^>]+src ?= ?(\\\\?\"|')([^\"']+)(\\\\?\"|')[^>]+>";

            if (Regex.IsMatch(content, regex))
            {
                foreach (Match m in Regex.Matches(content, regex, RegexOptions.Multiline))
                {
                    if (m.Success && m.Groups != null && m.Groups.Count > 2 && m.Groups[2].Success)
                    {
                        matches.Add(m.Groups[2].Value.TrimEnd('\\'));
                    }
                }
            }

            return matches;

        }
        /// <summary>
        /// Appends http to a non-http url
        /// </summary>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string AddHttpToUrl(string url)
        {
            var linkParser = new Regex(@"(http|https)://?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Check wheather the url has an http
            if (!linkParser.IsMatch(url))
            {
                // Append url with
                url = string.Format("{0}{1}", "http://", url);
            }

            return url;
        }

        /// <summary>
        /// To get the blurb image url frm a html document
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetOGImageUrl(HtmlDocument html, Uri url)
        {
            // checking whether the html document exists
            if (html != null && html.DocumentNode != null)
            {
                // getting the image node
                HtmlNode imageNode = html.DocumentNode.SelectSingleNode("//meta[@property='og:image'] | //meta[@name='image'] | //meta[@name='Image']");
                if (imageNode != null)
                {
                    // getting the content attribute
                    var contentAttribute = imageNode.Attributes["content"];
                    if (contentAttribute != null && !string.IsNullOrWhiteSpace(contentAttribute.Value))
                    {
                        if (!string.IsNullOrWhiteSpace(contentAttribute.Value))
                        {
                            if (url != null)
                            {
                                return CompleteRelativeUrl(contentAttribute.Value, url);
                            }
                        }
                    }
                }

                imageNode = html.DocumentNode.SelectSingleNode("//link[@rel='image_src']");
                if (imageNode != null)
                {
                    // getting the href attribute
                    var hrefAttribute = imageNode.Attributes["href"];
                    if (hrefAttribute != null && !string.IsNullOrWhiteSpace(hrefAttribute.Value))
                    {
                        if (!string.IsNullOrWhiteSpace(hrefAttribute.Value))
                        {
                            if (url != null)
                            {
                                return CompleteRelativeUrl(hrefAttribute.Value, url);
                            }
                        }
                    }
                }
            }

            // if nothing found
            return string.Empty;
        }

        /// <summary>
        /// To  Complete Relative Url
        /// </summary>
        /// <param name="relativeSrc"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string CompleteRelativeUrl(string relativeSrc, Uri url)
        {
            if (!relativeSrc.StartsWith(@"http:") && !relativeSrc.StartsWith(@"https:"))
            {
                var append = string.Empty;
                if (relativeSrc.StartsWith(@"//"))
                {
                    append = url.Scheme + ":";
                }
                else if (relativeSrc.StartsWith(url.Authority))
                {
                    append = url.Scheme + @"://";
                }
                else if (relativeSrc.StartsWith(@"/"))
                {
                    append = url.Scheme + @"://" + url.Authority;
                }
                else
                {
                    append = url.Scheme + @"://" + url.Authority + @"/";
                }
                return relativeSrc.Insert(0, append);
            }
            return relativeSrc;
        }

        /// <summary>
        /// To get selected image format
        /// </summary>
        /// <param name="imageFetchingInputs"></param>
        /// <returns></returns>
        private string GetSelectedImageFormat(bool Jpeg, bool Png)
        {
            string selectedImageFormat = string.Empty;

            if (Jpeg)
                selectedImageFormat = selectedImageFormat + "//img/@src[contains(@src,'.jpg')] |";
            if (Png)
                selectedImageFormat = selectedImageFormat + "//img/@src[contains(@src,'.png')] |";

            //Removing "|" from the selected format string 
            if ((Jpeg || Png) && (selectedImageFormat.Substring(selectedImageFormat.Length - 1, 1) == "|"))
            {
                selectedImageFormat = selectedImageFormat.Remove(selectedImageFormat.Length - 1);
            }

            return selectedImageFormat;
        }

        #endregion Private Methods
    }

}