﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using DataLayer.DataClasses.Concrete;

[assembly: InternalsVisibleTo("Tests")]

namespace DataLayer.Startup.Internal
{

    internal static class LoadDbDataFromXml
    {

        public static IEnumerable<Blog> FormBlogsWithPosts(string filepathWithinAssembly)
        {
            var assemblyHoldingFile = Assembly.GetExecutingAssembly();

            using (var fileStream = assemblyHoldingFile.GetManifestResourceStream(filepathWithinAssembly))
            {
                if (fileStream == null)
                    throw new NullReferenceException("Could not find the xml file you asked for. Did you remember to set properties->BuildAction to Embedded Resource?");
                var xmlData = XElement.Load(fileStream);
                
                //now decode and return
                var tagsDict = DecodeTags(xmlData.Element("Tags"));
                return DecodeBlogs(xmlData.Element("Blogs"), tagsDict);
            }
        }

        private static IEnumerable<Blog> DecodeBlogs(XElement element, Dictionary<string, Tag> tagsDict)
        {
            var result = new Collection<Blog>();
            foreach (var blogXml in element.Elements("Blog"))
            {
                var newBlogger = new Blog()
                {
                    Name = blogXml.Element("Name").Value,
                    EmailAddress = blogXml.Element("Email").Value,
                    Posts = new Collection<Post>()
                };

                foreach (var postXml in blogXml.Element("Posts").Elements("Post"))
                {
                    var newPost = new Post()
                    {
                        Blogger = newBlogger,
                        Title = postXml.Element("Title").Value,
                        Content = postXml.Element("Content").Value,
                        Tags = postXml.Element("TagSlugs").Value.Split(',').Select(x => tagsDict[x.Trim()]).ToList()
                    };
                    newBlogger.Posts.Add(newPost );
                }    
                result.Add( newBlogger);
            }
            return result;

        }

        private static Dictionary<string, Tag> DecodeTags(XElement element)
        {
            var result = new Dictionary<string, Tag>();
            foreach (var newTag in element.Elements("Tag").Select(tagXml => new Tag()
            {
                Name = tagXml.Element("Name").Value,
                Slug = tagXml.Element("Slug").Value
            }))
            {
                result[newTag.Slug] = newTag;
            }
            return result;
        }
    }
}
