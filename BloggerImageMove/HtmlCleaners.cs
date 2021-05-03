using HtmlAgilityPack; // NuGet package; see https://github.com/zzzprojects
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BloggerImageMove
{
    partial class Program
    {

        /// <summary>
        /// HTML pro-processing for simple string replacements regardless of context or node position
        /// </summary>
        /// <param name="HtmlSource"></param>
        static void PreProcess(ref string HtmlSource)
        {
            // a bit lazy, we'll do some simple replacements with string substitution

            // we're not actually changing the HtmlSource, just using sement to count elements
            string[] segments = HtmlSource.Split("---");
            string ThisSegment = segments[segments.Length - 1]; // we're not interested in the leading, non-HTML segments
            while (ThisSegment.StartsWith("\n") || ThisSegment.StartsWith("\r"))
            {
                ThisSegment = ThisSegment.Substring(1);
            }
            while (ThisSegment.EndsWith("\n") || ThisSegment.EndsWith("\r"))
            {
                ThisSegment = ThisSegment.Substring(0, ThisSegment.Length - 1);
            }

            // we always want the <pre> wrapper as the parent node when seen with <code>
            HtmlSource = HtmlSource.Replace("<code><pre>", "<pre><code>",  StringComparison.CurrentCultureIgnoreCase);
            HtmlSource = HtmlSource.Replace("</pre></code>", "</code></pre>", StringComparison.CurrentCultureIgnoreCase);

            // <p> and </p> start on new lines
            if (ThisSegment.Split(Environment.NewLine).Length < ThisSegment.Split("<p").Length)
            {
                // we don't really need to wrap a lone <p> in a <div>
                HtmlSource = HtmlSource.Replace("<div><p></p></div>", 
                                                "<br />", 
                                                StringComparison.CurrentCultureIgnoreCase);
                HtmlSource = HtmlSource.Replace("<div><p /></div>", 
                                                "<br />", 
                                                StringComparison.CurrentCultureIgnoreCase);

                // a single, empty <p></p> will be replaced with <br />
                HtmlSource = HtmlSource.Replace("<p></p>", 
                                                "<br />", 
                                                StringComparison.CurrentCultureIgnoreCase);

                // each interesting <p.. > starts on a line by itself, as does the repective closing </p>
                HtmlSource = HtmlSource.Replace("<p", 
                                                Environment.NewLine + "<p", 
                                                StringComparison.CurrentCultureIgnoreCase);

                HtmlSource = HtmlSource.Replace("</p>", 
                                                 Environment.NewLine + "</p>" + Environment.NewLine, 
                                                 StringComparison.CurrentCultureIgnoreCase);
            }

            // <div> and </div> start on new lines
            if (ThisSegment.Split(Environment.NewLine).Length < ThisSegment.Split("<div").Length)
            {
                // we don't really need to wrap a lone <br> in a <div>
                HtmlSource = HtmlSource.Replace("<div><br></div>",
                                                "<br />", 
                                                StringComparison.CurrentCultureIgnoreCase);

                HtmlSource = HtmlSource.Replace("<div><br /></div>", 
                                                "<br />", 
                                                StringComparison.CurrentCultureIgnoreCase);

                // all <div ...> tags start on their own line
                HtmlSource = HtmlSource.Replace("<div", 
                                                Environment.NewLine + "<div", 
                                                StringComparison.CurrentCultureIgnoreCase);

                HtmlSource = HtmlSource.Replace("</div>", 
                                                Environment.NewLine + "</div>" + Environment.NewLine, 
                                                StringComparison.CurrentCultureIgnoreCase);
            }

            // <img> starts on a new line
            if (ThisSegment.Split(Environment.NewLine).Length < ThisSegment.Split("<img").Length)
            {
                HtmlSource = HtmlSource.Replace("<img", 
                                                Environment.NewLine + "<img", 
                                                StringComparison.CurrentCultureIgnoreCase);
            }

            // insert div wrapper for code copy feature
            // we formatted div's above, so be sure and add the properly formatted ones here, last.
            if (ThisSegment.Contains("{% include code_header.html %}"))
            {
                // we've probably aldready done the conversion for qall of them
            }
            else
            {
                // insert the code header that allows for the "copy to clipboard" feature
                HtmlSource = HtmlSource.Replace("<pre", NewLine
                                                       + "{% include code_header.html %}"
                                                       + NewLine
                                                       + @"<div class=""language highlighter-rouge""><div class=""highlight"">"
                                                       + NewLine
                                                       + "<pre");
                HtmlSource = HtmlSource.Replace("</pre>", "</pre>"
                                                        + NewLine
                                                        + "</div></div>"
                                                        + NewLine);
            }


            string name = ((System.Reflection.AssemblyCompanyAttribute)System.Reflection.Assembly.GetCallingAssembly().GetCustomAttribute(typeof(System.Reflection.AssemblyCompanyAttribute))).Company;

            // append info at end
            HtmlSource = HtmlSource + "<br /> Copyright (c)" + name + " all rights reserved. Blogger Image Move Cleaned: " + DateTime.Now.ToString() + "<br />" + NewLine;
            HtmlSource = HtmlSource + "<!--   Copyright (c)" + name + " all rights reserved.  -->" + NewLine;
        }

        /// <summary>
        /// Given ThisNode, ensure there's a new line before it (not currently working properly
        /// </summary>
        /// <param name="ThisNode"></param>
        static void EnsureNewLineBefore(ref HtmlNode ThisNode)
        {
            throw new Exception("EnsureNewLineBefore Not implemented");
            // enabling this code breaks things elsewhere; the collection changes in the for each...

            //HtmlNode ThisPrevious = ThisNode.PreviousSibling;
            //if (ThisPrevious == null)
            //{
            //    HtmlNode NewPreviousNode = ThisNode.OwnerDocument.CreateTextNode(NewLine);
            //    ThisNode.ParentNode.InsertBefore(NewPreviousNode, ThisNode);

            //}
            //else
            //{
            //    if (ThisPrevious.InnerText.StartsWith(NewLine))
            //    {

            //    }
            //    else
            //    {
            //        HtmlNode NewPreviousNode = ThisNode.OwnerDocument.CreateTextNode(NewLine);
            //        switch (ThisNode.PreviousSibling.Name.ToLower())
            //        {
            //            case "#Text":
            //                ThisNode.InnerHtml = appendCrLf + ThisNode.InnerHtml;
            //                break;

            //            default:
            //                ThisNode.ParentNode.InsertBefore(NewPreviousNode, ThisNode);
            //                break;
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ThisNode"></param>
        static void EnsureNewLineAfter(ref HtmlNode ThisNode)
        {
            HtmlNode ThisNext = ThisNode.NextSibling;
            if (ThisNext == null)
            {
                // If there's no next node for the last child then we definitely need to add a text element
                HtmlNode NewAfterNode = ThisNode.OwnerDocument.CreateTextNode(NewLine);
                ThisNode.ParentNode.InsertAfter(NewAfterNode, ThisNode);
            }
            else
            {
                if (ThisNext.InnerText.Replace(" ","").StartsWith(NewLine))
                {
                    // thisNext looks *after* the closing tag!
                    // if the next element alread starts with LF, there's nothing to do
                }
                else
                {

                    HtmlNode NewLineNode = ThisNode.OwnerDocument.CreateTextNode(NewLine);
                    switch (ThisNode.NextSibling.Name.ToLower())
                    {
                        case "#text":
                            ThisNode.NextSibling.InnerHtml = ThisNode.InnerHtml + appendCrLf;
                            break;

                        default:
                            ThisNode.ParentNode.InsertAfter(NewLineNode, ThisNode);
                            break;
                    }
                }

                // since the opening <div> tag might be long, we want to ensure the fird child is a newline
                if (ThisNode.ChildNodes.Count > 0)
                {
                    if (ThisNode.FirstChild.Name.ToLower() == "#text")
                    {
                        //  we have text, but is it a new line?
                        if (ThisNode.FirstChild.InnerText.StartsWith(NewLine))
                        {
                            // the first child is text and starts with a newline, so nothing to do
                        }
                        else
                        if (ThisNode.FirstChild.Name.ToLower() == "br")
                        {
                            // we won't insert InnerHtml into the <br>
                        }
                        else
                        {
                            ThisNode.FirstChild.InnerHtml = NewLine + ThisNode.FirstChild.InnerText;
                        }
                    }
                    else
                    {
                        // we certainly don't have a NewLine if the first child is not text
                        HtmlNode NewLineNode = ThisNode.OwnerDocument.CreateTextNode(NewLine);
                        ThisNode.ChildNodes.Prepend(NewLineNode);
                    }
                }
            }
        }

        /// <summary>
        /// remove <br> and other fluff from <pre> and <code> nodes
        /// </summary>
        /// <param name="ThisNode"></param>
        static void CleanCodeSegment(ref HtmlNode ThisNode)
        {
            foreach (HtmlAttribute ThisAttribute in ThisNode.Attributes)
            {
                Console.WriteLine("Removing {0} attribute: {1}:{2}", ThisNode.Name, ThisAttribute.Name, ThisAttribute.Value);
            }
            ThisNode.Attributes.RemoveAll(); // we never want additional attributes on PRE and CODE tags from blogspot (e.g. styles, classes, etc)

            int ChildNodeCount = ThisNode.ChildNodes.Count;

            //  foreach (HtmlNode CodeNode in ThisNode.ChildNodes)
            for (int i = ChildNodeCount - 1; i >= 0; i--)
            {
                if (ThisNode.ChildNodes[i] == null)
                {
                    Console.WriteLine("Null CodeNode?");
                }
                else
                {
                    if (ThisNode.ChildNodes[i].Name.ToLower() == "br")
                    {
                        Console.WriteLine("Deleting code: {0}", ThisNode.ChildNodes[i].Name);
                        // ThisNode.ChildNodes[i].Remove(); // << this does not seem to actually remove a child :/

                        // remove the undesired <br> element, typically found in <pre> and <code> segments. (otherwise causes double spacing)
                        ThisNode.RemoveChild(ThisNode.ChildNodes[i]);

                    }
                    else
                    {
                        Console.WriteLine("Keeping code: {0}", ThisNode.ChildNodes[i].Name);
                    }
                }
            }

            // EnsureNewLineBefore(ref ThisNode);
            // EnsureNewLineAfter(ref ThisNode);
        }

        static void ReplaceAnchorImage(ref HtmlNode ThisNode)
        {
            foreach (HtmlAttribute ThisAttribute in ThisNode.Attributes)
            {
                // show all attribute name/value pairs
                // Console.WriteLine("Found Attribute: {0}={1}.", ThisAttribute.Name, ThisAttribute.Value);

                if (ThisAttribute.Name == "href" && IsImageFileName(ThisAttribute.Value))
                {
                    string thisURL = ThisAttribute.Value;
                    if (thisURL == null || thisURL.Trim() == "")
                    {
                        Console.WriteLine(" Warning: Node {0} has a blank HREF url!", ThisNode.Id);
                        // TODO put in a placeholder image?
                    }
                    else
                    {
                        Console.WriteLine(" Found URL: {0}", thisURL);
                    }
                    string NewURL = SaveImageFile(ThisAttribute.Value);
                    if (NewURL == thisURL)
                    {

                    }
                    else
                    {
                        Console.WriteLine(" Modified URL to: {0}", NewURL);
                    }
                    ThisAttribute.Value = NewURL;
                }
            }
        }

        static void ReplaceImageSource(ref HtmlNode ThisNode)
        {
            foreach (HtmlAttribute ThisAttribute in ThisNode.Attributes)
            {
                // show all attribute name/value pairs
                // Console.WriteLine("Found Attribute: {0}={1}.", ThisAttribute.Name, ThisAttribute.Value);

                if (ThisAttribute.Name == "src")
                {
                    ThisAttribute.Value = SaveImageFile(ThisAttribute.Value);
                }
            }
        }

    }
}
