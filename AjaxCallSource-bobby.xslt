<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="variableDeclarations">
    <Declarations>
      <Declaration>var ajaxCallListener;</Declaration>
      <Declaration>var ajaxSuccessAry = new Array();</Declaration>
      <Declaration>var ajaxCallAry = new Array();</Declaration>
      <Declaration>var ajaxCallCtr = 0;</Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'ajg'"/>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $variableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#34;\(]+?|&#34;[^&#34;\n\r]*?&#34;|\(([^;=,&#34;]*?,?(&#34;[^\n\r&#34;]*?&#34;)?)+\)|/[^/\n]+?/)*)+?)?)">
      <xsl:matching-substring>
        <xsl:element name="Entry">
          <xsl:attribute name="type" select="'global'" />
          <xsl:element name="OrigName">
            <xsl:value-of select="regex-group(1)" />
          </xsl:element>
          <xsl:element name="NewName">
            <xsl:value-of select="concat('_', $globalVariablePrefix, position())" />
          </xsl:element>
          <xsl:element name="Assign">
            <xsl:value-of select="normalize-space(regex-group(4))" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>


  <xsl:template match="/">
    <xsl:element name="CodeFile">
      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations"/>
      </xsl:element>
      <xsl:element name="Classes">
      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'AjaxCall'" />
        <xsl:attribute name="ClassPrefix" select="'_ac'" />
        <xsl:attribute name="ClassFunctionPrefix" select="'acf'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params">
            <xsl:element name="Param">destURL</xsl:element>
            <xsl:element name="Param">rootURL</xsl:element>
            <xsl:element name="Param">requestSrc</xsl:element>
            <xsl:element name="Param">testElem</xsl:element>
          </xsl:element>
          <xsl:variable name="constructorBodyElems">
            <xsl:text>
            this.DestURL = destURL;
            this.ClientID = getQueryParam("ClientID");
            this.IATName = getQueryParam("IATName");
            this.rootURL = rootURL;
            this.requestSrc = requestSrc;
            this.testElem = testElem;
            this.CallNdx = -1;
            return this;
          </xsl:text>
          </xsl:variable>
          <xsl:element name="ConstructorBody">
            <xsl:for-each select="tokenize($constructorBodyElems, '&#x0A;')">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)" />
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
        <xsl:element name="PrototypeChain">
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'buildRequestDocument'" />
            <xsl:element name="Params">
              <xsl:element name="Param">request</xsl:element>
              <xsl:element name="Param">requestData</xsl:element>
              <xsl:element name="Param">verifiableFiles</xsl:element>
              <xsl:element name="Param">requestRelTo</xsl:element>
              <xsl:element name="Param">requestType</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
      var xmlDoc = document.implementation.createDocument(&quot;&quot;, &quot;AjaxRequest&quot;, null);
      var elem = xmlDoc.createElement(&quot;Request&quot;);
      xmlDoc.documentElement.setAttribute(&quot;RequestType&quot;, requestType);
      if (requestRelTo != null)
      xmlDoc.documentElement.setAttribute(&quot;RequestRelTo&quot;, requestRelTo);
      this.requestString = request;
      elem.appendChild(xmlDoc.createTextNode(request));
      xmlDoc.documentElement.appendChild(elem);
      var elem = xmlDoc.createElement(&quot;RequestData&quot;);
      elem.appendChild(xmlDoc.createCDATASection(requestData));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;ClientID&quot;);
      elem.appendChild(xmlDoc.createTextNode(parseInt(this.ClientID, 10)));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;IATName&quot;);
      elem.appendChild(xmlDoc.createTextNode(this.IATName));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;Host&quot;);
      elem.appendChild(xmlDoc.createTextNode(window.location.hostname));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;RootContext&quot;);
      elem.appendChild(xmlDoc.createTextNode(this.rootURL));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;RequestSource&quot;);
      elem.appendChild(xmlDoc.createTextNode(this.requestSrc));
      xmlDoc.documentElement.appendChild(elem);
      elem = xmlDoc.createElement(&quot;TestElement&quot;);
      elem.appendChild(xmlDoc.createTextNode(this.testElem));
      xmlDoc.documentElement.appendChild(elem);
      var resourcesElem = xmlDoc.createElement(&quot;Resources&quot;);
      var scriptElems = document.getElementsByTagName("script");
      var currScriptElem = scriptElems[0];
      for (var ctr = 0; ctr &lt; scriptElems.length; ctr++) {
            elem = xmlDoc.createElement(&quot;Resource&quot;);
            elem.setAttribute(&quot;SourceType&quot;, &quot;URL&quot;);
            while (currScriptElem.nodeType != 1)
                currScriptElem = currScriptElem.nextSibling;
            var src = currScriptElem.getAttribute(&quot;src&quot;);
            if (src.search(&quot;/&quot; + this.ClientID + &quot;/&quot; + this.IATName + &quot;/&quot;) != -1) 
                elem.setAttribute(&quot;RelTo&quot;, &quot;Test&quot;);
            else
                elem.setAttribute(&quot;RelTo&quot;, &quot;ContextRoot&quot;);
            elem.appendChild(xmlDoc.createTextNode(src.match(/[^\/]+$/)));
            currScriptElem = currScriptElem.nextSibling;
            resourcesElem.appendChild(elem);
        }
        if (verifiableFiles) {
            for (ctr = 0; ctr &lt; verifiableFiles.length; ctr++) {
                var filepath;
                elem = xmlDoc.createElement(&quot;Resource&quot;);
                elem.setAttribute(&quot;RelTo&quot;, verifiableFiles[ctr].relTo);
                elem.setAttribute(&quot;SourceType&quot;, verifiableFiles[ctr].sourceType);
                elem.appendChild(xmlDoc.createTextNode(verifiableFiles[ctr].filename));
                resourcesElem.appendChild(elem);
            }
        }
        xmlDoc.documentElement.appendChild(resourcesElem);
        return this.getTextRepresentation(xmlDoc);
        </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="normalize-space(.)">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'recurseElement'" />
            <xsl:element name="Params">
              <xsl:element name="Param">elem</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
      var strRep = &quot;&lt;&quot; + elem.localName;
      if (elem.hasAttributes()) {
      for (var ctr = 0; ctr &lt; elem.attributes.length; ctr++) {
                var attrName = elem.attributes.item(ctr).nodeName;
                strRep += &quot; &quot; + attrName + &quot;=\&quot;&quot; + elem.getAttribute(attrName) + &quot;\&quot;&quot;;
            }
        }
        strRep += &quot;>\r\n&quot;;
        if (elem.nodeType == 1) {
            for (var ctr = 0; ctr &lt; elem.childNodes.length; ctr++)
                if (elem.childNodes[ctr].nodeType == 1)
                    strRep += this.recurseElement(elem.childNodes[ctr]);
        }
        for (var ctr = 0; ctr &lt; elem.childNodes.length; ctr++) {
            if (elem.childNodes[ctr].nodeType == 3)
                strRep += elem.childNodes[ctr].nodeValue;
            else if (elem.childNodes[ctr].nodeType == 4)
                strRep += &quot;&lt;![CDATA[&quot; + elem.childNodes[ctr].data + &quot;]]&gt;&quot;;
      }
      strRep += &quot;&lt;/&quot; + elem.localName + &quot;>\r\n&quot;;
      return strRep;
        </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getTextRepresentation'" />
            <xsl:element name="Params">
              <xsl:element name="Param">xmlDoc</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
          var strContent = &quot;&lt;?xml version=\'1.0\' encoding=\'UTF-8\' ?&gt;\r\n&quot;;
          strContent += this.recurseElement(xmlDoc.documentElement);
          return strContent;
        </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'processResponse'" />
            <xsl:element name="Params">
              <xsl:element name="Param">resp</xsl:element>
              <xsl:element name="Param">onAjaxSuccess</xsl:element>
              <xsl:element name="Param">functThis</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
          var respType = resp.documentElement.getAttribute(&quot;ResponseType&quot;);
          var elem;
          if (respType == &quot;Error&quot;) {
            var paramList = resp.documentElement.getElementsByTagName(&quot;ErrorParam&quot;);
            var queryStr = &quot;&quot;;
            for (var ctr = 0; ctr &lt; paramList.length; ctr++) {
              if (ctr != 0)
                queryStr += &quot;&amp;&quot;;
              elem = paramList[ctr].getElementsByTagName(&quot;Name&quot;)[0];
              while (elem.nodeType != 1)
                elem = elem.nextSibling;
              queryStr += elem.childNodes[0].nodeValue + &quot;=&quot;;
              elem = paramList[ctr].getElementsByTagName(&quot;Value&quot;)[0];
              while (elem.nodeType != 1)
                elem = elem.nextSibling;
              queryStr += elem.childNodes[0].nodeValue;
            }
            window.location.assign("/ServerError.html?" + queryStr);   
          } else if (respType == &quot;Text&quot;) {
            elem = resp.documentElement.getElementsByTagName(&quot;Response&quot;)[0];
            elem = elem.firstChild;
            while (elem.nodeType != 4)
              elem = elem.nextSibling;
            if (functThis)
              onAjaxSuccess.call(functThis, elem.data);
            else
              onAjaxSuccess(elem.data);
          } else if (respType == &quot;XML&quot;) {
            elem = resp.documentElement.firstChild;
            while (elem.nodeType != 1)
              elem = elem.nextSibling;
            if (functThis)
              onAjaxSuccess.call(functThis, elem);
            else
              onAjaxSuccess(elem);
          }
        </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>

          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'call'" />
            <xsl:element name="Params">
              <xsl:element name="Param">msgBody</xsl:element>
              <xsl:element name="Param">onAjaxSuccess</xsl:element>
              <xsl:element name="Param">functThis</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              if (functThis)
              this.CallbackThis = functThis;
              else
              this.CallbackThis = null;
              this.Message = msgBody;
              this.OnSuccess = onAjaxSuccess;
              var xmlhttp;
              if (window.XMLHttpRequest) {
              xmlhttp = new XMLHttpRequest();
              } else {
              xmlhttp = new ActiveXObject(&quot;Microsoft.XMLHTTP&quot;);
              }
              this.callObj = xmlhttp;
              this.CallNdx = ajaxSuccessAry.length;
              xmlhttp.onreadystatechange = function() { OnAjaxStateChange(this); };
              this.TimeoutID = window.setTimeout(timeout, 30000);
              ajaxSuccessAry.push("unanswered");
              ajaxCallAry.push(this);
              xmlhttp.open(&quot;POST&quot;, this.DestURL, true);
              xmlhttp.setRequestHeader(&quot;AjaxCall&quot;, true);
              xmlhttp.setRequestHeader(&quot;Content-Type&quot;, &quot;text/xml;charset=utf-8&quot;);
              xmlhttp.setRequestHeader(&quot;Content-length&quot;, msgBody.length);
              xmlhttp.send(msgBody);
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.))">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)" />
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getMessage'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.Message;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getCallNdx'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.CallNdx;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getOnSuccess'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.OnSuccess;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getCallbackThis'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.CallbackThis;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getTimeoutID'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.TimeoutID;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
      </xsl:element>
      <!--      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'AjaxCallListener'" />
        <xsl:attribute name="ClassPrefix" select="'_ac'" />
        <xsl:attribute name="ClassFunctionPrefix" select="'acf'" />
        <xsl:element name="Constructor">
        <xsl:element name="Params">
          <xsl:element name="Param">ajaxRequest</xsl:element>
          <xsl:element name="Param">ajaxCallObj</xsl:element>
          <xsl:element name="Param">successCallback</xsl:element>
          <xsl:element name="Param">callbackThis</xsl:element>
        </xsl:element>
        <xsl:variable name="constructorBodyElems">
          <xsl:text>
            this.AjaxRequest = ajaxRequest;
            this.AjaxCallObj = ajaxCallObj;
            this.SuccessCallback = successCallback;
            this.CallbackThis = callbackThis;    
            this.timedOut = false;
            this.callComplete = false;
          </xsl:text>
        </xsl:variable>
        <xsl:element name="ConstructorBody">
          <xsl:for-each select="tokenize($constructorBodyElems, '&#x0A;')">
            <xsl:if test="string-length(normalize-space(.))">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space(.)" />
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
        </xsl:element>
        <xsl:element name="PrototypeChain">
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'startCall'" />
            <xsl:element name="Params">
              <xsl:element name="Param">destURL</xsl:element>
              <xsl:element name="Param">msgBody</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                ajaxCallListener = this;
                this.AjaxRequest.onreadystatechange = this.onAjaxStateChange;
                this.DestURL = destURL;
                this.MessageBody = msgBody;
                window.setTimeout(this.timeout, 30000);
                this.AjaxRequest.open(&quot;POST&quot;, destURL, true);
                this.AjaxRequest.setRequestHeader(&quot;AjaxCall&quot;, true);
                this.AjaxRequest.setRequestHeader(&quot;Content-Type&quot;, &quot;text/xml;charset=utf-8&quot;);
                this.AjaxRequest.setRequestHeader(&quot;Content-length&quot;, msgBody.length);
                this.AjaxRequest.send(msgBody);
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)"/>
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'onAjaxStateChange'" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                if ((ajaxCallListener.getAjaxRequest().readyState == 4) &amp;&amp; (ajaxCallListener.getAjaxRequest().status == 200) &amp;&amp; (!ajaxCallListener.isTimedOut())) {
                  if (ajaxCallListener.getCallbackThis())
                    ajaxCallListener.getAjaxCallObj().processResponse(ajaxCallListener.getAjaxRequest().responseXML, ajaxCallListener.getSuccessCallback(), ajaxCallListener.getCallbackThis());
                  else
                    ajaxCallListener.getAjaxCallObj().processResponse(ajaxCallListener.getAjaxRequest().responseXML, ajaxCallListener.getSuccessCallback());
                  ajaxCallListener.setCallComplete(true);
                } 
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)"/>
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getAjaxRequest'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.AjaxRequest;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'isTimedOut'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.timedOut;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'setTimedOut'" />
            <xsl:element name="Params">
              <xsl:element name="Param">val</xsl:element>
            </xsl:element>
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                this.timedOut = val;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getSuccessCallback'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.SuccessCallback;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getCallbackThis'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.CallbackThis;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getAjaxCallObj'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.AjaxCallObj;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getDestURL'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.DestURL;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'getMessageBody'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.MessageBody;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'isCallComplete'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                return this.callComplete;
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space($functionBodyElems)"/>
              </xsl:element>
            </xsl:element>
          </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'setCallComplete'" />
            <xsl:element name="Params">
              <xsl:element name="Param">val</xsl:element>
              </xsl:element>
              <xsl:variable name="functionBodyElems">
                <xsl:text>
                this.callComplete = val;
              </xsl:text>
              </xsl:variable>
              <xsl:element name="FunctionBody">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space($functionBodyElems)"/>
                </xsl:element>
              </xsl:element>
            </xsl:element>
          <xsl:element name="Function">
            <xsl:attribute name="FunctionName" select="'timeout'" />
            <xsl:element name="Params" />
            <xsl:variable name="functionBodyElems">
              <xsl:text>
                if (ajaxCallListener.isCallComplete())
                  return;
                ajaxCallListener.setTimedOut(true);
                var xmlhttp;
                if (window.XMLHttpRequest) {
                  xmlhttp = new XMLHttpRequest();
                } else {
                  xmlhttp = new ActiveXObject(&quot;Microsoft.XMLHTTP&quot;);
                }
                var acl = new AjaxCallListener(xmlhttp, ajaxCallListener.getAjaxCallObj(), ajaxCallListener.getSuccessCallback(), ajaxCallListener.getCallbackThis());
                acl.startCall(ajaxCallListener.getDestURL(), ajaxCallListener.getMessageBody());
              </xsl:text>
            </xsl:variable>
            <xsl:element name="FunctionBody">
              <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
                <xsl:if test="string-length(normalize-space(.)) gt 0">
                  <xsl:element name="Code">
                    <xsl:value-of select="normalize-space(.)"/>
                  </xsl:element>
                </xsl:if>
              </xsl:for-each>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'CallMap'" />
        <xsl:attribute name="ClassPrefix" select="'_ac'" />
        <xsl:attribute name="ClassFunctionPrefix" select="'acf'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params" />
          <xsl:variable name="constructorBodyElems">
            <xsl:text>
              this.timeoutAry = new Array();
              this.completeAry = new Array();
            </xsl:text>
          </xsl:variable>
          <xsl:for-each select="tokenize($constructorBodyElems, '&#x0A;')">
            <xsl:if test="string-length(normalize-space(.)) gt 0">
              <xsl:element name="Code">
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
        <xsl:element name="PrototypeChain">
          <xsl:element name="Function">
            <xsl:attribute name="FuntionName" select="'registerCall'"/>
            <xsl:element name="Params"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>-->
      <xsl:element name="Functions">
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'timeout'" />
        <xsl:attribute name="FunctionPrefix" select="'_af'" />
        <xsl:element name="Params" />
        <xsl:variable name="functionBodyElems">
          <xsl:text>
                var ndx = ajaxSuccessAry.indexOf("unanswered");
                if (ndx == -1)  
                  return;
                ajaxSuccessAry[ndx] = "timed out";
                ajaxCallAry[ndx].call(ajaxCallAry[ndx].getMessage(), ajaxCallAry[ndx].getOnSuccess(), ajaxCallAry[ndx].getCallbackThis());
              </xsl:text>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
            <xsl:if test="string-length(normalize-space(.)) gt 0">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space(.)"/>
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'OnAjaxStateChange'" />
          <xsl:attribute name="FunctionPrefix" select="'_af'" />
          <xsl:element name="Params">
            <xsl:element name="Param">xmlhttp</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:text>
                var ajaxObjNdx = ajaxSuccessAry.indexOf("unanswered");
                if (ajaxObjNdx == -1)
                  return;
                var ajaxObj = ajaxCallAry[ajaxObjNdx];
                if ((xmlhttp.readyState == 4) &amp;&amp; (xmlhttp.status == 200)) {
                  window.clearTimeout(ajaxObj.getTimeoutID());
                  ajaxSuccessAry[ajaxObjNdx] = "answered";
                  if (ajaxObj.getCallbackThis())
                    ajaxObj.processResponse(xmlhttp.responseXML, ajaxObj.getOnSuccess(), ajaxObj.getCallbackThis());
                  else
                    ajaxObj.processResponse(xmlhttp.responseXML, ajaxObj.getOnSuccess());
                } else if (xmlhttp.readyState == 4) {
                  window.clearTimeout(ajaxObj.getTimeoutID());
                  ajaxSuccessAry[ajaxObjNdx] = "answered";
                  ajaxObj.call(ajaxObj.getMessage(), ajaxObj.getOnSuccess(), ajaxObj.getCallbackThis());
                }
              </xsl:text>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="tokenize($functionBodyElems, '&#x0A;')">
              <xsl:if test="string-length(normalize-space(.)) gt 0">
                <xsl:element name="Code">
                  <xsl:value-of select="normalize-space(.)"/>
                </xsl:element>
              </xsl:if>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>

