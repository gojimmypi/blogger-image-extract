using HtmlAgilityPack; // NuGet package; see https://github.com/zzzprojects
using System;
using System.IO;
using System.Net;

namespace BloggerImageMove
{
    class Program
    {

        static string HtmlSource = "";
        static string SaveToDirectory = "./";
        static string SourceDirectory = "./";
        static string NewImagePath = "/images/";

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
        /// given an imageURL, save it locally
        /// </summary>
        /// <param name="ImageUrl"></param>
        static void SaveImageFile(string ImageUrl)
        {
            // a blogger image path will typically looks like:
            // https://3.bp.blogspot.com/-Ir9dz7Zdbk0/XF9XWwR6uNI/AAAAAAAAB4k/41-TnDNyZIcgHhPiXrxNhvGccXShUxWCQCLcBGAs/s400/ULX3S-libusbK.PNG
            // https://3.bp.blogspot.com/-Ir9dz7Zdbk0/XF9XWwR6uNI/AAAAAAAAB4k/41-TnDNyZIcgHhPiXrxNhvGccXShUxWCQCLcBGAs/s1600/ULX3S-libusbK.PNG
            // we are interested in that last part: ULX3S-libusbK.PNG
            // the smaller image (e.g. ./s400/ULX3S-libusbK.PNG) is typically displayed on the web page, wit ha link to the larger image when clicked on to zoom (e.g. ./s1600/ULX3S-libusbK.PNG)
            ImageUrl = ImageUrl.Replace(@"\", @"/"); // ensure we are only using forward slashes
            string[] UrlPathSegments = ImageUrl.Split(@"/");
            if (UrlPathSegments.Length < 1)
            {
                Console.WriteLine("ERROR: no URL path delimters found! (bad HTML image src tag?)");
            }
            else if (UrlPathSegments[0].StartsWith("/") || UrlPathSegments[0].StartsWith("../")) {
                Console.WriteLine("Skipping image that appears to have already beend converted: {0}", ImageUrl);
            }
            else
            {
                Console.WriteLine("Found image src={0}", ImageUrl);

                string ImageFileName = UrlPathSegments[UrlPathSegments.Length - 1]; // the name of the file; e.g. ULX3S-libusbK.PNG
                string ImageSizeDirectory = UrlPathSegments[UrlPathSegments.Length - 2]; // blogspot will typically group images sizes (default widths) into subdirectories such as "s400" with otherwise the same filename
                string ImageSaveDirectory = SaveToDirectory + "/" + ImageSizeDirectory;
                string NewImagePath = ImageSaveDirectory + "/" + ImageFileName; // the full path to write image files; e.g. C:\\workspace\\gojimmypi.github.io\\gridster-jekyll-theme/images/s400/ULX3S-libusbK.PNG

                if (File.Exists(NewImagePath))
                {
                    Console.WriteLine("Skipping file that already exists: {0}.", ImageFileName);
                }
                else
                {
                    // only download and save files we don't already have
                    byte[] imageAsByteArray;
                    using (var webClient = new WebClient())
                    {
                        // TODO: sometimes this takes a LONG time, why??
                        imageAsByteArray = webClient.DownloadData(ImageUrl);
                    }
                    if (!Directory.Exists(ImageSaveDirectory))
                    {
                        Directory.CreateDirectory(ImageSaveDirectory);
                    }
                    Console.WriteLine("Saving image: {0}", NewImagePath);
                    File.WriteAllBytes(NewImagePath, imageAsByteArray);
                }

                string NewImageURL = ".." + Program.NewImagePath + ImageFileName; // e.g. ../images/s400/ULX3S-libusbK.PNG
                HtmlSource = HtmlSource.Replace(ImageUrl, NewImageURL);
            }
        }

        /// <summary>
        /// find all images in the supplied HtmlNodeCollection and save them
        /// </summary>
        /// <param name="c"></param>
        static void FindImages(HtmlNodeCollection c)
        {
            foreach (HtmlNode ThisNode in c)
            {
                // if ThisNode has children, we'll first recursively call to process them
                if (ThisNode.ChildNodes.Count > 0)
                {
                    FindImages(ThisNode.ChildNodes);
                }

                // if ThisNode is an image tag, then we'll save the image found in the src attribute 
                if (ThisNode.Name.ToLower() == "img")
                {
                    foreach (HtmlAttribute ThisAttribute in ThisNode.Attributes)
                    {
                        // show all attribute name/value pairs
                        // Console.WriteLine("Found Attribute: {0}={1}.", ThisAttribute.Name, ThisAttribute.Value);

                        if (ThisAttribute.Name == "src")
                        {
                            SaveImageFile(ThisAttribute.Value);
                        }
                    }
                }

                // if ThisNode is an anchor tag, it may or may not be an image
                if (ThisNode.Name.ToLower() == "a")
                {
                    foreach (HtmlAttribute ThisAttribute in ThisNode.Attributes)
                    {
                        // show all attribute name/value pairs
                        // Console.WriteLine("Found Attribute: {0}={1}.", ThisAttribute.Name, ThisAttribute.Value);

                        if (ThisAttribute.Name == "href" && IsImageFileName(ThisAttribute.Value))
                        {
                            SaveImageFile(ThisAttribute.Value);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// process the file f, extracting all images and saving them
        /// </summary>
        /// <param name="f"></param>
        static void ProcessFile(string f)
        {
            Console.WriteLine("Processing file: {0} ...",f);

            HtmlSource = File.ReadAllText(f);
            var document = new HtmlDocument();
            document.LoadHtml(HtmlSource);

            FindImages(document.DocumentNode.ChildNodes);

            // HtmlSource raw will have been modified to replace all instances of olf image path with new, so we need to save the file
            File.WriteAllText(f, HtmlSource);
        }

        /// <summary>
        /// The main app; pass a parameter for the source to the files to convert; Images saved in respective ..\images\
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string param = args[0];

            if (param != "")
            {
                SourceDirectory = param;
            }
            SaveToDirectory = Directory.GetParent(Path.GetDirectoryName(SourceDirectory)).ToString() + NewImagePath;

            Console.WriteLine("Looking for HTML files in {0}", SourceDirectory); // for a jekyll conversion, typically the "_posts" directory; e.g. C:\\workspace\\gojimmypi.github.io\\gridster-jekyll-theme\\_posts\\
            Console.WriteLine("Will save images files in {0}", SaveToDirectory); // for a jekyll conversion, typically the /images/ directory; e.g. C:\\workspace\\gojimmypi.github.io\\gridster-jekyll-theme/images/

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(SourceDirectory,"*ULX3S*");
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);
        }
    }
}
