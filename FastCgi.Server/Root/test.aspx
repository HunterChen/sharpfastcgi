<%@ Page Language="C#" EnableSessionState="true" ContentType="text/html" ResponseEncoding="utf-8" %>
Hello world!<br />
Datetime here is: <% = DateTime.Now.ToString() %><br />
Path is: <% =Request.Path %><br />
PathInfo is: <% =Request.PathInfo %><br />
Session ID: <% =Session.SessionID %><br />