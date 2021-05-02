using System;
using System.IO;
using System.Net;


namespace BloggerImageMove
{
    partial class Program
    {
        /// <summary>
        /// returns true if the extension of the filename f is a known image extension, otherwise false.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        static bool IsImageFileName(string f)
        {
            bool res = false;
            if (f == null)
            {
                f = "";
            }
            f = f.ToLower(); // we want consistent case to compare

            string ThisExtension = Path.GetExtension(f); // reminder this includes the leading period

            switch (ThisExtension)
            {
                case null:
                case "": // can't use string.Empty: see https://stackoverflow.com/questions/2701314/cannot-use-string-empty-as-a-default-value-for-an-optional-parameter
                    res = false;
                    break;

                case ".png":
                case ".bmp":
                case ".jpg":
                case ".jpeg":
                    res = true;
                    break;

                default:
                    res = false;
                    break;
            }

            return res;
        }

        /// <summary>
        /// return a clean file name, replacing special characters with underscore
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        static string CleanImageFileName(string f)
        {
            f = f.Replace("%2B", "_");
            f = f.Replace("_-_", "_");
            f = f.Replace("+-+", "_");
            f = f.Replace("++", "_");
            f = f.Replace("+", "_");
            f = f.Replace("__", "_");
            return f;
        }

        /// <summary>
        /// given an imageURL, check to see if we already have a file by that name, fetch from web if needed, save it locally. returns a new URL to use
        /// </summary>
        /// <param name="ImageUrl"></param>
        static string SaveImageFile(string ImageUrl)
        {
            string NewImageURL = ImageUrl; // keep the value unless known to need a change
            // a blogger image path will typically looks like:
            // https://3.bp.blogspot.com/-Ir9dz7Zdbk0/XF9XWwR6uNI/AAAAAAAAB4k/41-TnDNyZIcgHhPiXrxNhvGccXShUxWCQCLcBGAs/s400/ULX3S-libusbK.PNG
            // https://3.bp.blogspot.com/-Ir9dz7Zdbk0/XF9XWwR6uNI/AAAAAAAAB4k/41-TnDNyZIcgHhPiXrxNhvGccXShUxWCQCLcBGAs/s1600/ULX3S-libusbK.PNG
            // we are interested in that last part: ULX3S-libusbK.PNG
            // the smaller image (e.g. ./s400/ULX3S-libusbK.PNG) is typically displayed on the web page, wit ha link to the larger image when clicked on to zoom (e.g. ./s1600/ULX3S-libusbK.PNG)

            //if (!ImageUrl.ToLower().StartsWith("http")) {
            //    Console.WriteLine("oops");
            //    return;
            //}

            ImageUrl = ImageUrl.Replace(@"\", @"/"); // ensure we are only using forward slashes
            string[] UrlPathSegments = ImageUrl.Split(@"/");
            if (UrlPathSegments.Length < 2)
            {
                Console.WriteLine("ERROR: no URL path delimters found! (bad HTML image src tag?) see: {0}", ImageUrl);
            }
            else if (UrlPathSegments[0].StartsWith(@"/") || UrlPathSegments[0].StartsWith(@"..") || UrlPathSegments[0].StartsWith(@"../"))
            {
                Console.WriteLine("Skipping image that appears to have already beend converted: {0}", ImageUrl);
            }
            else
            {
                Console.WriteLine("Found image src={0}", ImageUrl);

                string ImageFileName = UrlPathSegments[UrlPathSegments.Length - 1]; // the name of the file; e.g. ULX3S-libusbK.PNG
                string ImageSizeDirectory = UrlPathSegments[UrlPathSegments.Length - 2]; // blogspot will typically group images sizes (default widths) into subdirectories such as "s400" with otherwise the same filename
                string ImageSaveDirectory = SaveToDirectory + "/" + ImageSizeDirectory;

                string OrginalImagePath = ImageSaveDirectory + "/" + ImageFileName; // the full path to write image files; e.g. C:\\workspace\\gojimmypi.github.io\\gridster-jekyll-theme/images/s400/ULX3S-libusbK.PNG

                string ProperImageFileName = CleanImageFileName(ImageFileName);
                string ProperImagePath = ImageSaveDirectory + "/" + ProperImageFileName;

                if (ProperImageFileName == ImageFileName)
                {
                    Console.WriteLine("Filname not does need to be cleaned.");
                }
                else
                {
                    if (File.Exists(OrginalImagePath))
                    {
                        Console.WriteLine("Found file to rename: {0}", OrginalImagePath);
                        if (File.Exists(ProperImagePath))
                        {
                            Console.WriteLine("Warning, file to rename already exists! See {0}", ProperImagePath);
                        }
                        else
                        {
                            File.Move(OrginalImagePath, ProperImagePath);
                        }
                    }
                }


                if (File.Exists(ProperImagePath))
                {
                    Console.WriteLine("Skipping web fetch for file that already exists: {0}.", ProperImageFileName);
                }
                else
                {
                    // only download and save files we don't already have
                    byte[] imageAsByteArray;
                    using (var webClient = new WebClient())
                    {
                        Console.WriteLine("Downloading image: {0}", ImageUrl);
                        // TODO: sometimes this takes a LONG time, why??
                        imageAsByteArray = webClient.DownloadData(ImageUrl);
                    }
                    if (!Directory.Exists(ImageSaveDirectory))
                    {
                        Directory.CreateDirectory(ImageSaveDirectory);
                    }
                    Console.WriteLine("Saving image: {0}", ProperImagePath);
                    File.WriteAllBytes(ProperImagePath, imageAsByteArray);
                }

                NewImageURL = ".." + "/" + Program.NewImagePath + "/" + ImageSizeDirectory + "/" + ProperImageFileName; // e.g. ../images/s400/ULX3S-libusbK.PNG
                NewImageURL = NewImageURL.Replace("//", "/");
                HtmlSource = HtmlSource.Replace(ImageUrl, NewImageURL);
            }

            return NewImageURL;
        }


    }
}
