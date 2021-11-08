<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="text" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" encoding="UTF-8"
               indent="yes" />

  <xsl:template match="ConfigFile">

    <xsl:variable name="CodeFile">
      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'ExpirationTimer'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params">
            <xsl:element name="Param">timeout</xsl:element>
            <xsl:element name="Param">codeList</xsl:element>
          </xsl:element>
          <xsl:element name="ConstructorBody">
            <xsl:text>
              this.refreshed = false;
              this.timeout = timeout;
              this.failed = false;
              this.codeList = codeList
            </xsl:text>
          </xsl:element>
        </xsl:element>

        <xsl:element name="PrototypeChain">
          <xsl:element name="MemberFunction">
            <xsl:attribute name="'expired'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                if (this.mutateRefresh(false) == true) {
                  setTimeout(this.expired, this.timeout);
                } else {
                  this.failed = true;
                  while (this.codeList.hasChildNodes())
                    thisCodeList.removeChildNode(this.codeList.firstChild);
                }
              </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'start'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                setTimeout(this.expired, this.timeout);
              </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'mutateRefresh'" />
            <xsl:element name="Params">
              <xsl:element name="Param">newValue</xsl:element>
              <xsl:element name="Param">newTimeout</xsl:element>
            </xsl:element>
            <xsl:element name="FunctionBody">
              <xsl:text>
                var oldVal = this.refreshed;
                if (newTimeout)
                  this.timeout = newTimeout;
                this.refreshed = newValue;
                return oldVal;
              </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'refresh'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                this.mutateRefresh(true);
                return !this.failed;
              </xsl:text>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'Decryptor'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params">
            <xsl:element name="Param">codeList</xsl:element>
            <xsl:element name="Param">expirationTimer</xsl:element>
            <xsl:element name="Param">aes</xsl:element>
          </xsl:element>
        </xsl:element>
        <xsl:element name="ConstructorBody">
          <xsl:text>
            this.B64 = new FromBase64();
            this.decryptor = decryptor;
            this.codeList = codeList;
            this.expirationTimer = new ExpirationTimer(3000, codeList);
          </xsl:text>
        </xsl:element>

        <xsl:element name="PrototypeChain">
          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'EvalCode'" />
            <xsl:element name="FunctionBody">
              var ctr1, ctr2;
              for (var ctr1 = 0; ctr1 &lt; this.codeList.childNodes.length; ctr1++) {
                if (!this.expirationTimer.refresh())
                  return;
                codeXML = codeList.childNodes[ctr1];
                var attr5 = new Number(codeXML.attributes.item(5).value);
                var attr4 = new Number(codeXML.attributes.item(4).value);
                var attr3 = new Number(codeXML.attributes.item(3).value);
                var nodeNum = Math.round(attr5 + Math.sqrt(Math.pow((attr4 + attr5 * (-attr4 / attr3)) / ((attr3 / attr4) - (-attr4 / attr3)), 2)
                  + Math.pow((attr3 / attr4) * (attr4 + attr5 * (-attr4 / attr3)) / ((attr3 / attr4) - (-attr4 / attr3)), 2)
                  - Math.pow((attr4 + attr5 * (-attr4 / attr3)) / ((attr3 / attr4) - (-attr4 / attr3)), 2)
                  + Math.pow((attr3 / attr4) * (attr4 + attr5 * (-attr4 / attr3)) / ((attr3 / attr4) - (-attr4 / attr3)), 2) - Math.pow(attr5, 2)));
                var nChars = new Number(codeList.attributes.item(0).value);
                var words = this.B64.decodeWords(codeList.childNodes[nodeNum].nodeValue);
                var decryptedWords = new Array();
                for (ctr2 = 0; ctr2 < words.length / 4; ctr2++)
                  decryptedWords.concat(this.decryptor.decrypt(words.splice(0, 4)));
                if (words.length == 1)
                  decryptedWords.concat(this.decryptor.decrypt(new Array(words.splice(0, 1), 0, 0, 0)));
                else if (words.length == 2)
                  decryptedWords.concat(this.decryptor.decrypt(new Array(words.splice(0, 2), 0, 0)));
                else if (words.length == 3)
                  decryptedWords.concat(this.decryptor.decrypt(new Array(words.splice(0, 3), 0)));
                this.codeList.childNode[nodeNum].nodeValue = "";
                var codeStr = new String();
                for (ctr2 = 0; ctr2 &lt; decryptedWords.length; ctr2++) {
                  codeStr += String.fromCharCode((decryptedWords[ctr2] >> 26) &amp; 255);
                  codeStr += String.fromCharCode((decryptedWords[ctr2] >> 16) &amp; 255);
                  codeStr += String.fromCharCode((decryptedWords[ctr2] >> 8) &amp; 255);
                  codeStr += String.fromCharCode(decryptedWords[ctr2] &amp; 255);
                }
                eval(codeStr.substring(0, nChars));
              }
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'CodeRetriever'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params">
            <xsl:element name="Param">testElem</xsl:element>
            <xsl:element name="Param">onLoadComplete</xsl:element>
            <xsl:element name="Param">onAjaxError</xsl:element>
            <xsl:element name="Param">stopTimer</xsl:element>
            <xsl:element name="Param">keys</xsl:element>
          </xsl:element>
          <xsl:element name="ConstructorBody">
            <xsl:value-of select="concat('var path = window.location.pathname.substr(0, window.location.pathname.lastIndexOf(CookieUtil.get(&quot;ServletContext&quot;))) + &quot;', ClientID, '/', IATName, '/&quot;')" />
            <xsl:value-of select="concat('var this.ClientID = ', ClientID, ';&#x0A;')" />
            <xsl:value-of select="concat('var this.IATName = &quot;', IATName, '&quot;;&#x0A;')" />
            <xsl:text>  
              this.PageURL = window.location.protocol + "//" + window.location.hostname + path + testElem + ".html";
              this.TestElem = testElem;
              this.OnLoadComplete = onLoadComplete;
              this.OnAjaxError = onAjaxError;
              this.StopTimer = stopTimer;
              this.Keys = keys;
            </xsl:text>
          </xsl:element>
        </xsl:element>

        <xsl:element name="MemberFunction">
          <xsl:attribute name="FunctionName" select="'GetKey'" />
          <xsl:element name="Constructor">
            <xsl:element name="Params">
              <xsl:element name="Param">keyXml</xsl:element>
            </xsl:element>
            <xsl:element name="FunctionBody">
              var KeyAES = new AES(this.Keys);
              var keyData = xmlDoc.getElementsByTagName("JSKeyEntry");
              var nodeTestElem;
              var ctr = 0;
              do {
              nodeTestElem = keyData.item(ctr++).getAttribute("TestElem");
              } while (nodeTestElem != testElem);
              var keyElem = keyData.item(--ctr);
              var keyParts = keyData.getElementsByTagName("KeyWord");
              var keyWords = new Array();
              for (ctr = 0; ctr < keyParts.length; ctr++)
            keyWords.push(parseInt(keyParts.item(ctr)[0], 16));
        var key = KeyAES.decrypt(keyWords.splice(0, 4));
        key = key.concat(KeyAES.decrypt(keyWords.splice(0, 4)));
        return key;

            </xsl:element>
          </xsl:element>
        </xsl:element>
        
        <xsl:element name="MemberFunction">
          <xsl:attribute name="FunctionName" select="'BeginKeyLoad'" />
          <xsl:element name="Params" />
          <xsl:element name="FunctionBody">
            var xmlhttp;
            if (window.XMLHttpRequest) {
              xmlhttp=new XMLHttpRequest();
            } else {
              xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
            }
            xmlhttp.onreadystatechange = function() {
              if ((xmlhttp.readyState == 4) &amp;&amp; (xmlhttp.status == 200)) {
                var AESKeyDecrytor = new AES(this.GetKey.call(this, this.TestElem, xmlhttp.responseXML));
              } else if ((xmlhttp.readyState == 4) &amp;&amp; (xmlhttp.status != 200)) {
                this.onAjaxError.call();
              }
            };
            <xsl:variable name="ResourceList">
              <Resource SourceType="URL">this.PageURL</Resource>
            </xsl:variable>
            <xsl:variable name="AjaxCallShell">
              <xsl:call-template name="BuildAjaxCallVariable">
                <xsl:with-param name="RequestType" select="'Keys'" />
                <xsl:with-param name="Request" select="'&quot;Keys&quot;'" />
                <xsl:with-param name="RequestSource" select="'&quot;Decryptor.dat&quot;'" />
                <xsl:with-param name="RequestSourceType" select="'File'" />
                <xsl:with-param name="NonScriptVerifiableResources" select="$ResourceList" />
              </xsl:call-template>
            </xsl:variable>
            <xsl:call-template name="GenereateAjaxXML">
              <xsl:with-param name="RequestXML" select="$AjaxCallShell/RequestXML" />
            </xsl:call-template>
            xmlhttp.open("POST", ajaxURL, true);
            xmlhttp.setRequestHeader("AjaxCall", "true");
            xmlhttp.setRequestHeader("Content-type","text/xml;charset=utf-8");
            xmlhttp.send(getTextRepresentation(xmlDoc));
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:variable>
  </xsl:template>

  <xsl:template name="GenerateAjaxXML">
    <xsl:param name="RequestXML" />
    <xsl:text>
      var elem, loopCtr, resourceNodeList, resourceNode, xmlDoc = document.implementation.createDocument("", "AjaxRequest", null);
    </xsl:text>
    <xsl:value-of select="concat('xmlDoc.documentElement.setAttribute(&quot;RequestType&quot;, &quot;', $RequestXML/@RequestType, '&quot;);&#x0A;')" />
    <xsl:if test="$RequestXML/RequestSource eq '-ThisURL-'">
      <xsl:text>
        var path;
        if (CookieUtil.get("ServletContext") == "__ROOT__")
      </xsl:text>
      <xsl:value-of select="concat('path = window.location.pathname + &quot;/', //Survey/@ClientID, '/', //Survey/@IAT, '/', //Survey/@Name, '.html&quot;;&#x0A;')" />
      <xsl:text>
      else {
        path = window.location.pathname.substr(0, window.location.pathname.lastIndexOf(CookieUtil.get("ServletContext")));
      </xsl:text>
      <xsl:value-of select="concat('path = path + &quot;', //Survey/@ClientID, '/', //Survey/@IAT, '/', //Survey/@Name, '.html&quot;;&#x0A;')" />
      <xsl:text>
        }
        var RequestSource = window.location.protocol + "//" + window.location.hostname + path;
        var RequestSourceType = "URL";
      </xsl:text>
    </xsl:if>
    <xsl:if test="$RequestXML/RequestSource ne '-ThisURL-'">
      <xsl:value-of select="concat('var RequestSource = ', $RequestXML/RequestSource, ');&#x0A;')" />
      <xsl:value-of select="concat('var RequestSourceType = &quot;', $RequestXML/RequestSource/@SourceType, '&quot;;&#x0A;')" />
    </xsl:if>
    <xsl:for-each select="$RequestXML/RequestElem">
      <xsl:value-of select="concat('elem = xmlDoc.createElement(&#34;', @name, '&#34;);&#xA;')"/>
      <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextNode(', ., '));&#xA;')"/>
      <xsl:text>xmlDoc.documentElement.appendChild(elem);</xsl:text>
    </xsl:for-each>
    <xsl:text>
      elem = xmlDoc.createElement("RequestSource");
      elem.appendChild(xmlDoc.createTextNode(RequestSource));
      elem.setAttribute("SourceType", RequestSourceType);
      xmlDoc.documentElement.appendChild(elem);
      resourceNode = xmlDoc.createElement("Resources");
      elem.appendChild(resourceNode);
    </xsl:text>
    <xsl:for-each select="$RequestXML/VerifiableResources/Resource">
      <xsl:choose>
        <xsl:when test="@repeatable eq 'true'">
          <xsl:value-of select="concat('resourceNodeList = ', ., ';')"/>
          <xsl:text>for (loopCtr = 0; loopCtr &lt; resourceNodeList.length; loopCtr++) {</xsl:text>
          <xsl:value-of select="concat('if (resourceNodeList.item(loopCtr).hasAttribute(', @selector, ')) {&#x0A;')"/>
          <xsl:text>elem = xmlDoc.createElement("Resource");</xsl:text>
          <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextNode(resourceNodeList.item(loopCtr).getAttribute(', @selector, ')));&#x0A;')"/>
          <xsl:value-of select="concat('elem.setAttribute(&#34;SourceType&#34;, &#34;', @SourceType, '&#34;);&#x0A;')"/>
          <xsl:text>resourceNode.appendChild(elem); } }</xsl:text>
        </xsl:when>

        <xsl:otherwise>
          <xsl:text>elem = xmlDoc.createElement("Resource");</xsl:text>
          <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextNode(', ., '));&#xA;')"/>
          <xsl:value-of select="concat('elem.setAttribute(&#34;SourceType&#34;, ', @SourceType, ');&#x0A;')"/>
          <xsl:text>resourceNode.appendChild(elem);</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="BuildAjaxCallVariable">
      <xsl:param name="RequestType" />
      <xsl:param name="Request" />
      <xsl:param name="RequestSource" />
      <xsl:param name="RequestSourceType" />
      <xsl:param name="NonScriptVerifiableResources" />
      <xsl:element name="RequestXML">
        <xsl:attribute name="RequestType" select="$RequestType" />
        <xsl:element name="RequestElem">
          <xsl:attribute name="name" select="'Request'" />
          <xsl:value-of select="$Request" />
        </xsl:element>
        <xsl:element name="RequestElem">
          <xsl:attribute name="name" select="'ClientID'" />
          <xsl:text>getQueryString()["ClientID"]</xsl:text>
        </xsl:element>
        <xsl:element name="RequestElem">
          <xsl:attribute name="name" select="'IATName'" />
          <xsl:text>getQueryString()["IATName"]</xsl:text>
        </xsl:element>
        <xsl:element name="RequestElem">
          <xsl:attribute name="name" select="'Host'" />
          <xsl:text>window.location.hostname</xsl:text>
        </xsl:element>
        <xsl:element name="RequestSource">
          <xsl:attribute name="SourceType" select="$RequestSourceType" />
          <xsl:value-of select="$RequestSource" />
        </xsl:element>
        <xsl:element name="VerifiableResources">
          <xsl:element name="Resource">
            <xsl:attribute name="repeatable" select="'true'" />
            <xsl:attribute name="selector" select="'&quot;src&quot;'" />
            <xsl:attribute name="SourceType" select="'URL'" />
            <xsl:text>document.getElementsByTagName("script")</xsl:text>
          </xsl:element>
        </xsl:element>
        <xsl:if test="$NonScriptVerifiableResources ne '-NONE-'">
          <xsl:copy-of select="NonScriptVerifiableResources" />
        </xsl:if>
      </xsl:element>
    </xsl:template>
    BuildXMLRequestDoc : function(request) {
    var xmlDoc = document.implementation.createDocument("", "AjaxRequest", null);
    var elem = xmlDoc.createElement("Request");
    elem.appendChild(xmlDoc.createTextNode(request));
    xmlDoc.documentElement.appendChild(elem);
    
    elem = xmlDoc.createElement("ClientID");
    elem.appendChild(xmlDoc.createTextNode(parseInt(clientID, 10));
    xmlDoc.documentElement.appendChild(elem);

    elem = xmlDoc.createElement("IATName");
    elem.appendChild(xmlDoc.createTextNode(iatName));
    xmlDoc.documentElement.appendChild(elem);

    elem = xmlDoc.createElement("Host");
    elem.appendChild(xmlDoc.createTextNode(hostName));
    xmlDoc.documentElement.appendChild(elem);

    elem = xmlDoc.createElement("RequestSource");
    elem.appendChild(xmlDoc.createTextNode(requestSrc));
    elem.setAttribute("SourceType", requestSrcType);
    xmlDoc.documentElement.appendChild(elem);

    var resourcesElem = xmlDoc.createElement("Resources");
    elem = xmlDoc.createElement("Resource");
    elem.setAttribute("SourceType", "URL");
    elem.appendChild(xmlDoc.createTextNode(window.location.protocol + "//" + window.location.hostname + window.location.pathname.substring(0, window.location.pathname.lastIndexOf(CookieUtil.get("ServletContext"))) + clientID.toString() + "/" + testName + "/" + testElem + ".html"));
    resourcesElem.appendChild(elem);

    var scriptElems = document.getElementsByTagName("script");
    var currScriptElem = scriptElems.childNodes.item(0);
    for (var ctr = 0; ctr < scriptElems.childNodes.length; ctr++) {
        elem = xmlDoc.createElement("Resource");
        elem.setAttribute("SourceType", "URL");
        while (currScriptElem.nodeType != 1)
            currScriptElem = currScriptElem.nextSibling;
        elem.appendChild(xmlDoc.createTextNode(currScriptElem.getAttribute("src"));
        currScriptElem = currScriptElem.nextSibling;
        resourcesNode.appendChild(elem);
    }
    xmlDoc.documentElement.appendChild(resourcesNode);
    return xmlDoc;

    }

    BeginKeyLoad : function() {
  },

  function retrieveCode(URL, testElem, ClientID, IATName, onLoadComplete, onAjaxError, stopTimer, clientID, testName) {
  var key = retrieveKey(testElem, ClientID, IATName);
  var requestPath = "/" + parseInt(clientID, 10) + "/" + testName + "/";
  var xmlhttp;
  if (window.XMLHttpRequest)
  {// code for IE7+, Firefox, Chrome, Opera, Safari
  xmlhttp=new XMLHttpRequest();
  }
  else
  {// code for IE6, IE5
  xmlhttp=new ActiveXObject("Microsoft.XMLHTTP");
  }
  xmlhttp.onreadystatechange = function() {
  if ((xmlhttp.readyState == 4) && (xmlhttp.status == 200)) {
  stopTimer();
  var codeElems = xmlhttp.responseXML.documentElement.childNodes;
  var expirationTimer = new ExpirationTimer(3000);
  var decryptor = new Decryptor(codeElems, expirationTimer, new AES(key));
  expirationTimer.start();
  decryptor.evalCode();
  onLoadComplete();
  } else if ((xmlhttp.readyState == 4) && (xmlhttp.status != 200)) {
  onAjaxError();
  }
  }
  var xmlDoc = buildXMLRequestDoc(testElem, "TimerDecryptor.dat", "File", ClientID, IATName, testElem);
  xmlhttp.open("POST",URL,true);
  xmlhttp.setRequestHeader("AjaxCall", "true");
  xmlhttp.setRequestHeader("Content-type","text/xml;charset=utf-8");
  xmlhttp.send(getTextRepresentation(xmlDoc));
  }

  function buildXMLRequestDoc(request, requestSrc, requestSrcType, clientID, testName, testElem) {
  }

  function getKey(testElem) {
  var KeyAES = new AES(k1, k2, k3, k4);
  var parser, xmlDoc;
  if (window.DOMParser) {
  parser = new DOMParser();
  xmlDoc = parser.parseFromString(xmlText, "text/xml");
  } else {
  xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
  xmlDoc.async = false;
  xmlDoc.loadXML(xmlText);
  }
  var keyData = xmlDoc.getElementsByTagName("JSKeyEntry");
  var nodeTestElem;
  var ctr = 0;
  do {
  nodeTestElem = keyData.item(ctr++).getAttribute("TestElem");
  } while (nodeTestElem != testElem);
  var keyElem = keyData.item(--ctr);
  var keyParts = keyData.getElementsByTagName("KeyWord");
  var keyWords = new Array();
  for (ctr = 0; ctr < keyParts.length; ctr++)
        keyWords.push(parseInt(keyParts.item(ctr)[0], 16));
    var key = KeyAES.decrypt(keyWords.splice(0, 4));
    key = key.concat(KeyAES.decrypt(keyWords.splice(0, 4)));
    return key;
}
   
function retrieveKey(ajaxURL, testElem, ClientID, IATName, onRetrieveKeyComplete)
{
}

</xsl:stylesheet>