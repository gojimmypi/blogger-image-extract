using HtmlAgilityPack; // NuGet package; see https://github.com/zzzprojects
using System;
using System.IO;
using System.Net;

namespace BloggerImageMove
{
    partial class Program
    {

        static string NewLine = Environment.NewLine;
        static string appendCrLf = "";

        static string HtmlSource = "";
        static string SaveToDirectory = "./";
        static string SourceDirectory = "./";
        static string NewImagePath = "/images/";



        /// <summary>
        /// process all nodes in the supplied HtmlNodeCollection and edit them in place (passing by reference!)
        /// </summary>
        /// <param name="c"></param>
        static void ProcessNode(ref HtmlNode ThisNode)
        {
            for (int i = ThisNode.ChildNodes.Count -1; i >= 0 ; i--)
            {
                // we can't edit an entire collection, so we need to check each child node one at a time
                HtmlNode ReplaceNode = ThisNode.ChildNodes[i];
                ProcessNode(ref ReplaceNode);
                ThisNode.ReplaceChild(ReplaceNode, ThisNode.ChildNodes[i]);
            }

            switch (ThisNode.Name.ToLower())
            {
                case "a":
                    ReplaceAnchorImage(ref ThisNode);
                    break;

                case "img":
                    ReplaceImageSource(ref ThisNode);
                    break;

                case "pre":
                    EnsureNewLineAfter(ref ThisNode);
                    break;

                case "code":
                    CleanCodeSegment(ref ThisNode);
                    break;

                case "div":
                case "p":
                    //EnsureNewLineBefore(ref ThisNode);
                    EnsureNewLineAfter(ref ThisNode);

                    //appendCrLf = NewLine;
                    //if (ThisNode.InnerHtml.StartsWith(appendCrLf))
                    //{
                    //    // already has \n, don't addit
                    //}
                    //else
                    //{
                    //    ThisNode.InnerHtml = appendCrLf + ThisNode.InnerHtml;
                    //}
                    //if (ThisNode.InnerHtml.EndsWith(appendCrLf))
                    //{
                    //    // already has \n, don't addit
                    //}
                    //else
                    //{
                    //    ThisNode.InnerHtml =  ThisNode.InnerHtml + appendCrLf;
                    //}
                    //appendCrLf = "";
                    break;

                case "br":
                    appendCrLf = NewLine;
                    EnsureNewLineAfter(ref ThisNode);

                    // HtmlNode h = new HtmlNode(HtmlNodeType.Text, ThisNode.OwnerDocument, 0);
                    // ThisNode.InnerHtml = "\n";
                    // appendCrLf = "\n";
                    //HtmlNode ThisNext = ThisNode.NextSibling;
                    //if (ThisNext == null )
                    //{

                    //}
                    //else
                    //{
                    //    HtmlNode NewAFterNode = ThisNode.OwnerDocument.CreateTextNode(appendCrLf);
                    //    switch (ThisNode.NextSibling.Name.ToLower())
                    //    {
                    //        case "div":
                    //        case "p":
                    //        case "br":
                    //            ThisNode.ParentNode.InsertAfter(NewAFterNode, ThisNode);
                    //            break;

                    //        default:
                    //            break;
                    //    }
                    //}
                    break;


                case "#text":
                    // we may have determnined from a prior tag, that we wanted to add linefeed
                    if (ThisNode.InnerHtml.StartsWith(appendCrLf))
                    {
                        // already has \n, don't add it
                    }
                    else
                    {
                        ThisNode.InnerHtml = appendCrLf + ThisNode.InnerHtml;
                    }
                    appendCrLf = "";
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// ensure all new lines are using Environment.NewLine (typically Cr/Lf on Windows, Lf on Mac/Linux)
        /// </summary>
        /// <param name="forFile"></param>
        static void NormalizeLineEndings(string forFile)
        {
            string CrLfNormalize = File.ReadAllText(forFile);
            CrLfNormalize = CrLfNormalize.Replace("\r\n", "\n");
            CrLfNormalize = CrLfNormalize.Replace("\n\r", "\n");
            CrLfNormalize = CrLfNormalize.Replace("\r",  "\n");
            CrLfNormalize = CrLfNormalize.Replace("\n", Environment.NewLine);
            File.WriteAllText(forFile, CrLfNormalize);

        }
        /// <summary>
        /// process the file f, fixing HTML issues and extracting all images and saving them
        /// </summary>
        /// <param name="f"></param>
        static void ProcessFile(string f)
        {
            Console.WriteLine("Processing file: {0} ...",f);

            HtmlDocument document = new HtmlDocument();
            HtmlSource = File.ReadAllText(f);
            PreProcess(ref HtmlSource); // some things are simply easier with string replacements

            document = new HtmlDocument();
            document.LoadHtml(HtmlSource);

            HtmlNode n = document.DocumentNode;
            ProcessNode(ref n);
            File.WriteAllText(f, n.OuterHtml);

            NormalizeLineEndings(f);
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
            string[] fileEntries = Directory.GetFiles(SourceDirectory, "2020-11-09-goes17-satellite-image-reception-with.html");
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);
        }
    }
}
