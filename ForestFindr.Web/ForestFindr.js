        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
                appSource = sender.getHost().Source;
            }

            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
                return;
            }

            var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

            errMsg += "Code: " + iErrorCode + "    \n";
            errMsg += "Category: " + errorType + "       \n";
            errMsg += "Message: " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
                errMsg += "File: " + args.xamlFile + "     \n";
                errMsg += "Line: " + args.lineNumber + "     \n";
                errMsg += "Position: " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {
                if (args.lineNumber != 0) {
                    errMsg += "Line: " + args.lineNumber + "     \n";
                    errMsg += "Position: " + args.charPosition + "     \n";
                }
                errMsg += "MethodName: " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
        var container = null;
        var layer = null;
        var logo = null;
        
  function onLoaded(sender, eventArgs) {
       container = sender.findName("SplashContainer");
       layer = sender.findName("layer1");
       logo = sender.findName("logo");
       if (container != null)
           CenterSetWidth();
        }

      
        function CenterSetWidth() {

            var width;
            var height;

            // Get the browser height and width.
            if (parseInt(navigator.appVersion) > 3) {
                if (navigator.appName == "Netscape") {
                    width = window.innerWidth;
                    height = window.innerHeight;
                }
            }

            if (navigator.appName.indexOf("Microsoft") != -1) {
                width = document.body.offsetWidth;
                height = document.body.offsetHeight;
            }

            layer["Width"] = width;
            container["Width"] = width;
            container["Height"] = height;
            container["Canvas.Left"] = (width - container.Width) / 2;
            container["Canvas.Top"] = (height - container.Height) / 2;
        }

        function onSourceDownloadProgressChanged(sender, eventArgs) {

            var height;
            var width;
            // Get the browser height and width.
            if (parseInt(navigator.appVersion) > 3) {
                if (navigator.appName == "Netscape") {
                    width = window.innerWidth;
                    height = window.innerHeight;
                }
            }

            if (navigator.appName.indexOf("Microsoft") != -1) {
                width = document.body.offsetWidth;
                height = document.body.offsetHeight;
            }
            if (layer == null) 
            {
            layer = sender.findName("layer1");
        }
        if (logo == null) {
            logo = sender.findName("logo");
        }

        var status = sender.findName("SplashStatus");

           layer["Canvas.Top"] = height - height * eventArgs.progress;
          
           logo["Canvas.Top"] = (height - height * eventArgs.progress)+50;
           logo["Canvas.Left"] = ((width - logo.Width) / 2) - 160;
           status["Canvas.Top"] = (height - height * eventArgs.progress)+10;
           status["Canvas.Left"] = ((width - logo.Width) / 2) - 140;

            
       
            status.Text = 'WE ARE PLANTING TREES... ' + Math.round(eventArgs.progress * 100);

        }
        window.onresize = function (event) {
            if (container != null)
                CenterSetWidth();
        }
        var slCtl = null;
        function pluginLoaded(sender, args) {
            // get Silverlight Host Control
            slCtl = sender.getHost();
        }
        function GetPanoramioPhotos(url) {
            //wanted this in silverlight, but can't get it working. JSON with padding... hummm have to read more about this.
            $.getJSON(url,
        function (json) {
            slCtl.Content.PanoramioNet.ProcessPhotosCallback(json)
        });
        }

        function GetMediumPanoramioPhotos(url) {
            $.getJSON(url,
        function (json) {
            slCtl.Content.PanoramioNet.ProcessMediumPhotosCallback(json)
        });
        }

   