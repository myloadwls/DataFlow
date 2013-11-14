﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PixelFlow
{
    public class Image : IHttpHandler
    {
        private static string connString = ConfigurationManager
            .ConnectionStrings["Database"].ConnectionString;

        public void ProcessRequest(HttpContext context)
        {
            WriteResponse(context);

            if (context.Request.ServerVariables["HTTP_REFERER"] == null) return;

            SavePixelRequest(context);
        }

        private static void WriteResponse(HttpContext context)
        {
            if (!String.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]))
            {
                context.Response.StatusCode = 304;
                context.Response.StatusDescription = "Not Modified";
            }
            else
            {
                context.Response.Cache.SetCacheability(HttpCacheability.Public);
                context.Response.Cache.SetLastModified(DateTime.MinValue);
                context.Response.ContentType = "image/png";
                context.Response.WriteFile("pixel.png");
            }
            context.Response.Flush();
        }

        private void SavePixelRequest(HttpContext context)
        {
            // An exercise for the reader    
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }

    public class Request
    {
        
        public Request()
        {
            
        }
    }
}