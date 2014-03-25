<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ForestFindr.Web.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Protected Forest</title>
   <meta name="description" content="FORESTFINDR. helps you find fantastics forests and other wild areas to visit and discover. There are thousands around the world, using geolocation we can show you where that areas are located, as well as pictures and wikipedia information to help you find your perfect Forest to visit this weekend.">
   <meta name="keywords" content="Forests, planet, protected, find, nature, water, rivers, beutiful, silverlight, ecocontest,silverlightshow, floresta, trees, arvores">

    <style type="text/css">
    html, body {
	    height: 100%;
	    overflow: auto;
    }
    body {
	    padding: 0;
	    margin: 0;
	      font-family: Verdana;
            font-size: small;
    }
    #silverlightControlHost {
	    height: 100%;
	    text-align:center;
    }
    h1
        {
            font-family: Verdana;
            font-size: large;
            color: #0D4BA0;
        }

    </style>

    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript" src="ForestFindr.js"></script>
    <script src="http://ajax.aspnetcdn.com/ajax/jquery/jquery-1.4.4.js" type="text/javascript"></script> 
 
</head>
<body >
    <form id="form1" runat="server" style="height:100%">
    <div id="silverlightControlHost">
        <object data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="100%" height="100%">
		  <param name="source" value="ClientBin/ForestFindr.xap"/>
		  <param name="onError" value="onSilverlightError" />
		  <param name="background" value="white" />
          <param name="initParams" value="IPAddress=<%=InitParam%>"/>
		  <param name="minRuntimeVersion" value="4.0.50826.0" />
          <param name="onLoad" value="pluginLoaded" />
           <param name="splashscreensource" value="ClientBin/SplashScreen.xaml" />
           <param name="onSourceDownloadProgressChanged" value="onSourceDownloadProgressChanged" />

		  <param name="autoUpgrade" value="true" />
           <div>
                <p>
                    &nbsp;</p>
                <h1>
                    Forest Findr.</h1>
                <div>
                    <p>
                        Please install Microsoft Silverlight to enjoy explore forests near you.
                        </p>
                    <a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50401.0" style="text-decoration: none">
                        <img src="ClientBin\logo.png" alt="Forest Findr. Install Microsoft Silverlight" style="border-style: none" />
                    </a>
                    <p>FORESTFINDR. helps you find fantastics forests and other wild areas to visit and discover. There are thousands around the world, using geolocation we can show you where that areas are located, 
                    as well as pictures and wikipedia information to help you find your perfect Forest to visit this weekend.</p>
                </div>
                <p>
                    &nbsp;</p>
                <p>
                    © 2011 Rui Marinho</</p>
               
            </div>

	    </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe></div>
    </form>
</body>
</html>
