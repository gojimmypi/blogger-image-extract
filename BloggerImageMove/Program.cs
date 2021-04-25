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
        /// given an imageURL, save it locally
        /// </summary>
        /// <param name="ImageUrl"></param>
        static void SaveImageFile(string ImageUrl)
        {
            // a blogger image path will typically looks like:
            // https://3.bp.blogspot.com/-Ir9dz7Zdbk0/XF9XWwR6uNI/AAAAAAAAB4k/41-TnDNyZIcgHhPiXrxNhvGccXShUxWCQCLcBGAs/s400/ULX3S-libusbK.PNG
            // we are interested in that last part: ULX3S-libusbK.PNG
            ImageUrl = ImageUrl.Replace(@"\", @"/"); // ensure we are only using forward slashes
            string[] UrlPathSegments = ImageUrl.Split(@"/");
            if (UrlPathSegments.Length < 1)
            {
                Console.WriteLine("ERROR: no URL path delimters found! (bad HTML image src tag?)");
            }
            else
            {
                Console.WriteLine("Found image src={0}", ImageUrl);

                string ImageFileName = UrlPathSegments[UrlPathSegments.Length - 1]; // the name of the file; e.g. ULX3S-libusbK.PNG
                string NewImageFile = SaveToDirectory + ImageFileName; // the full path to write image files; e.g. C:\\workspace\\gojimmypi.github.io\\gridster-jekyll-theme/images/ULX3S-libusbK.PNG

                if (File.Exists(NewImageFile))
                {
                    Console.WriteLine("Skipping file that already exists: {0}.", ImageFileName);
                }
                else
                {
                    // only download and save files we don't already have
                    byte[] imageAsByteArray;
                    using (var webClient = new WebClient())
                    {
                        imageAsByteArray = webClient.DownloadData(ImageUrl);
                    }
                    File.WriteAllBytes(NewImageFile, imageAsByteArray);
                }

                string NewImageURL = NewImagePath + ImageFileName;
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

                // if ThisNode is an image tag, then we'll save it 
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
