// loads javascript in XML format from server -- obfuscated in 
var xmlhttp;
if (window.XMLHttpRequest)
  {
  xmlhttp=new XMLHttpRequest();
  }
else
  {
  xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
  }
xmlhttp.onreadystatechange=function()
  {
  if (xmlhttp.readyState==4 && xmlhttp.status==200)
    {
        var s = xmlhttp.getElementsByTagName("Statement");
        for (var ctr = 0; ctr < s.length; ctr++)
          s.childNodes[ctr].nodeValue.eval();
        BeginIATLoad();
    }
  }
var ajaxData = location.search.substring(1).split("&");
var ajaxMsg = "<JSRequest>";
for (var ctr = 0; ctr < ajaxData.length; ctr++)
  ajaxMsg += "<QueryVariable" + (ctr + 1).toString() + ">" + ajaxData[ctr] + "</QueryVariable>";
ajaxMsg += "<AdminPhase>" + CookieUtil.get("AdminPhase") + "<AdminPhase>";
ajaxMsg += "<AlternationValue>" + CookieUtil.get("AlternationValue") + "</AlternationValue>";
ajaxMsg += "</JSRequest>";
xmlhttp.open("POST", iatServer + "/JSProvider",true);
xmlhttp.setRequestHeader("Content-type", "text/html; charset=UTF-8");
xmlhttp.setRequestHeader("Content-Length", ajaxMsg.length.toString());
xmlhttp.send(ajaxMsg);

