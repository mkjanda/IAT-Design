<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                                                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                                                version="2.0"
                                                exclude-result-prefixes="xs">

  <xsl:output method="text" encoding="utf-8" indent="yes" media-type="text" />

  <xsl:template match="Survey">

    <xsl:variable name="Utils">
      <xsl:element name="Util">
        <xsl:attribute name="UtilName" select="'EventUtil'" />
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'addHandler'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">element</xsl:element>
            <xsl:element name="Param">type</xsl:element>
            <xsl:element name="Param">handler</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="CodeLine">if (element.addEventListener) {</xsl:element>
            <xsl:element name="CodeLine">element.addEventListener(type, handler, false);</xsl:element>
            <xsl:element name="CodeLine">} else if (element.attachEvent) {</xsl:element>
            <xsl:element name="CodeLine">element.attachEvent("on" + type, handler);</xsl:element>
            <xsl:element name="CodeLine">} else {</xsl:element>
            <xsl:element name="CodeLine">element["on" + type] = handler;</xsl:element>
            <xsl:element name="CodeLine">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/CodeLine">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getEvent'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="CodeLine">return event ? event : window.event;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/CodeLine">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getTarget'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="CodeLine">return event.target || event.srcElement;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/CodeLine">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'preventDefault'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code"> if(event.preventDefault) {</xsl:element>
            <xsl:element name="Code">event.preventDefault();</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">event.returnValue = false;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'removeHandler'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">element</xsl:element>
            <xsl:element name="Param">type</xsl:element>
            <xsl:element name="Param">handler</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (element.removeEventListener) {</xsl:element>
            <xsl:element name="Code">element.removeEventListener(type, handler, false);</xsl:element>
            <xsl:element name="Code">} else if (element.detachEvent) {</xsl:element>
            <xsl:element name="Code">element.detachEvent("on" + type, handler);</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">element["on" + type] = null;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'stopPropogation'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (event.stopPropagation) {</xsl:element>
            <xsl:element name="Code">event.stopPropagation();</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">event.cancelBubble = true;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'getCharCode'"/>
          <xsl:element name="Params">
            <xsl:element name="Param">event</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">if (typeof event.charCode == "number") {</xsl:element>
            <xsl:element name="Code">return event.charCode;</xsl:element>
            <xsl:element name="Code">} else {</xsl:element>
            <xsl:element name="Code">return event.keyCode;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())"/>
                <xsl:value-of select="."/>
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Util">
        <xsl:attribute name="UtilName" select="'CookieUtil'" />
        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'get'" />
          <xsl:element name="Params">
            <xsl:element name="Param">name</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">var cookieName = encodeURIComponent(name) + "=",</xsl:element>
            <xsl:element name="Code">cookieStart = document.cookie.indexOf(cookieName),</xsl:element>
            <xsl:element name="Code">cookieValue  = null;</xsl:element>
            <xsl:element name="Code">if (cookieStart &gt; -1) {</xsl:element>
            <xsl:element name="Code">var cookieEnd = document.cookie.indexOf(";", cookieStart);</xsl:element>
            <xsl:element name="Code">if (cookieEnd == -1)</xsl:element>
            <xsl:element name="Code">cookieEnd = document.cookie.length;</xsl:element>
            <xsl:element name="Code">}</xsl:element>
            <xsl:element name="Code">cookieValue = decodeURIComponent(document.cookie.substring(cookieStart + cookieName.length, cookieEnd));</xsl:element>
            <xsl:element name="Code">return cookieValue;</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="count(preceding-sibling::node())" />
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>

        <xsl:element name="Function">
          <xsl:attribute name="FunctionName" select="'change'" />
          <xsl:element name="Params">
            <xsl:element name="Param">name</xsl:element>
            <xsl:element name="Param">value</xsl:element>
          </xsl:element>
          <xsl:variable name="functionBodyElems">
            <xsl:element name="Code">document.cookie = encodeURIComponent(name) + "=" + encodeURIComponent(value);</xsl:element>
          </xsl:variable>
          <xsl:element name="FunctionBody">
            <xsl:for-each select="$functionBodyElems/Code">
              <xsl:element name="CodeLine">
                <xsl:attribute name="LineNum" select="preceding-sibling::node()" />
                <xsl:value-of select="." />
              </xsl:element>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:variable>

    <xsl:variable name="CodeFile">
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'getQueryString'"  />
        <xsl:element name="Params" />
        <xsl:element name="FunctionBody">
          <xsl:text>
            var result = {}, queryString = location.search.substring(1), re = /([^&amp;=]+)=([^&amp;]*)/g, m;
            while (m = re.exec(queryString)) {
            result[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
            }
            return result;
          </xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Function" >
        <xsl:attribute name="FunctionName" select="'OnRetrieveScriptFail'" />
        <xsl:element name="Params">
          <xsl:element name="Param">errorCaption</xsl:element>
          <xsl:element name="Param">errorDetail</xsl:element>
        </xsl:element>
        <xsl:element name="FunctionBody">
          <xsl:text>
          var mainContent = document.getElementById("MainContent");
          var AjaxErrorDiv = document.createElement("div");
          AjaxErrorDiv.className = "AjaxErrorDiv";
          var AjaxErrorMsgTag = document.createElement("h1");
          AjaxErrorMsg.className = "AjaxErrorMsg";
          var AjaxErrorMsg = document.createTextNode(error);
          AjaxErrorMsgTag.appendChild(AjaxErrorMsg);
          var AjaxErrorDetailTag = document.createElement("h2");
          AjaxErrorDetailTag.className = "AjaxErrorDetail";
          var AjaxErrorDetailMsg = document.createTextNode(errorDetail);
          AjaxErrorDetailTag.appendChild(AjaxErrorDetailMsg);
          AjaxErrorDiv.appendChild(AjaxErrorMsgTag);
          AjaxErrorDiv.appendChild(AjaxErrorDetailTag);
          mainContent.insertBefore(AjaxErrorDiv, mainContent.firstChild);
          </xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'OnRetrieveScript'" />
        <xsl:element name="Params" />
        <xsl:element name="FunctionBody">
          <xsl:text>
            document.getElementById("SubmitButton").disabled = false;
          </xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'OnUnload'" />
        <xsl:element name="Params" />
        <xsl:element name="FunctionBody">
          <xsl:text>
                        CookieUtil.change("CurrentIAT", "");
          </xsl:text>
        </xsl:element>
      </xsl:element>

      <xsl:element name="Class">
        <xsl:attribute name="ClassName" select="'AjaxTimer'" />
        <xsl:element name="Constructor">
          <xsl:element name="Params">
            <xsl:element name="Param">timeout</xsl:element>
          </xsl:element>
          <xsl:element name="ConstructorBody">
            <xsl:text>
                        this.stopped = false;
                        this.expired = false;
                        this.timeout = timeout;
                        this.redirectOnExpire = CookieUtil.get("RedirectOnExpire");
                  </xsl:text>
          </xsl:element>
        </xsl:element>
        <xsl:element name="PrototypeChain">
          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'start'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                  setTimeout(this.onExpired, this.timeout);
                </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'stop'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                    this.stopped = true;
                  </xsl:text>
            </xsl:element>
          </xsl:element>

          <xsl:element name="MemberFunction">
            <xsl:attribute name="FunctionName" select="'onExpired'" />
            <xsl:element name="Params" />
            <xsl:element name="FunctionBody">
              <xsl:text>
                  if (!this.stopped)
                    window.location.assign(this.redirectOnExpire);
                      this.expired = true;
                    </xsl:text>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>



      <xsl:element name="GlobalCode">
        <xsl:text>
              EventUtil.addHandler(window, "unload", OnUnload);
              var AjaxQueryTimer = new AjaxTimer(60000);
            </xsl:text>
      </xsl:element>

      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'OnLoad'" />
        <xsl:element name="Params" />
        <xsl:element name="FunctionBody">
          <xsl:text>
                  var xmlhttp, URL, testElem, decryptFileLen, xmlDoc, elem, attr, elemsToVerify;
                  
                  CookieUtil.change("CurrentIAT", "true");
                  document.getElementById("SubmitButton").disabled = true;
                  AjaxQueryTimer.start();
          </xsl:text>
          <xsl:value-of select="concat('URL = &quot;', //Survey/@ServerURL, '/&quot; + CookieUtil.get(&quot;ServletName&quot;);&#x0A;')" />
          <xsl:text>
      testElem = CookieUtil.get("TestElem");
                  htmlURL = window.location.protocol + "//" + window.location.hostname + CookieUtil.get("ServerPath") + CookieUtil.get("TestPath") + testElem + ".html";

                              if (window.XMLHttpRequest) {
                                    xmlhttp = new XMLHttpRequest;
                              } else {
                                    xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
                              }
                              xmlhttp.onreadystatechange=function()
                              {
                                    if (xmlhttp.readyState==4 &amp;&amp; xmlhttp.status==200)
                                    {
                                                eval(xmlhttp.responseText);
                                                decryptFileLen = xmlhttp.responseText.length;
                xmlhttp.responseText = "";
                xmlhttp.responseXML = "";
                                                retrieveScript(URL, testElem, decryptFileLen, OnRetrieveScript, OnRetrieveScriptFail, AjaxQueryTimer.stop);
                                    }
                                    else if (xmlhttp.readState==4 &amp;&amp; xmlhttp.status==400)
                                    {
                                                Abort(true);
                                                OnRetrieveScriptFail("Server Error", "This webpage made an invalid request on the server while attempting to load its administration code. If you own this test and this problem persists, please email this issue to admin@iatsoftware.net");
                                     }
                         };

         xmlDoc = document.implementation.createDocument("", "AjaxRequest", null);
         var elem, attr, repeatableElem, loopCtr;
</xsl:text>
          <xsl:variable name="ajaxRequest">
            <RequestElem name="Request">CODE_LOADER</RequestElem>
            <RequestElem name="ClientID">getQueryString()["ClientID"]</RequestElem>
            <RequestElem name="IATName">getQueryString()["IATName"]</RequestElem>
            <RequestElem name="Host">window.location.hostname</RequestElem>
            <RequestElem name="TestPath">"/" + getQueryString()["ClientID"] + "/" + getQueryString()["IATName"]</RequestElem>
            <VerifiableResources>
              <xsl:element name="Resource">
                <xsl:attribute name="RefType" select="'URL'" />
                <xsl:attribute name="RelTo" select="'Global'" />
                <xsl:text>htmlURL</xsl:text>
              </xsl:element>
              <xsl:element name="Resource">
                <xsl:attribute name="repeatable" select="'true'" />
                <xsl:attribute name="selector" select="'.src'" />
                <xsl:attribute name="RefType" select="'URL'" />
                <xsl:attribute name="RelTo" select="'Global'" />
                <xsl:text>document.getElementsByTagName("script")</xsl:text>
              </xsl:element>
            </VerifiableResources>

          </xsl:variable>
          <xsl:for-each select="$ajaxRequest/RequestElem">
            <xsl:value-of select="concat('elem = xmlDoc.createElement(&quot;', @name, '&quot;);&#x0A;')" />
            <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextElement(', ., '));&#x0A;')" />
            <xsl:text>xmlDoc.documentElement.appendChild(elem);&#x0A;</xsl:text>
          </xsl:for-each>
          <xsl:text>resourceNode = xmlDoc.createElement("Resources");&#x0A;</xsl:text>
          <xsl:for-each select="$ajaxRequest/VerifiableResources/Resource">
            <xsl:choose>
              <xsl:when test="@repeatable eq 'true'">
                <xsl:value-of select="concat('repeatableElem = ', ., ';&#x0A;')" />
                <xsl:text>for (loopCtr = 0; loopCtr &lt; repeatableElem.length; loopCtr++) {&#x0A;</xsl:text>
                <xsl:text>elem = xmlDoc.createElement("Resource");&#x0A;</xsl:text>
                <xsl:for-each select="attribute::node()" >
                  <xsl:if test="(name() eq 'RefType') or (name() eq 'RelTo')" >
                    <xsl:value-of select="concat('attr = xmlDoc.createAttribute(&quot;', name(), '&quot;);&#x0A;')" />
                    <xsl:value-of select="concat('attr.textContent = &quot;', ., '&quot;;&#x0A;')" />
                    <xsl:text>elem.attributes.setNamedItem(attr);&#x0A;</xsl:text>
                  </xsl:if>
                </xsl:for-each>
                <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextElement(repeatableElem.item(loopCtr)', @selector, '));&#x0A;')" />
                <xsl:text>resourceNode.appendChild(elem); }</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>elem = xmlDoc.createElement("Resource");&#x0A;</xsl:text>
                <xsl:text>attr = xmlDoc.createAttribute("RefType");&#x0A;</xsl:text>
                <xsl:value-of select="concat('attr.textContent = &quot;', @RefType, '&quot;;&#x0A;')" />
                <xsl:text>elem.attributes.setNamedItem(attr);&#x0A;</xsl:text>
                <xsl:text>attr = xmlDoc.createAttribute("RelTo");&#x0A;</xsl:text>
                <xsl:value-of select="concat('attr.textContent = &quot;', @RelTo, '&quot;;&#x0A;')" />
                <xsl:text>elem.attributes.setNamedItem(attr);&#x0A;</xsl:text>
                <xsl:value-of select="concat('elem.appendChild(xmlDoc.createTextElement(', ., '));&#x0A;')" />
                <xsl:text>resourceNode.appendChild(elem);&#x0A;</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:for-each>
          <xsl:text>
         xmlDoc.documentElement.appendChild(resourceNode);
                         xmlhttp.open("POST", URL, true);
                         xmlhttp.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
                         xmlhttp.send(getTextRepresentation(xmlDoc));
            </xsl:text>
        </xsl:element>
      </xsl:element>
    </xsl:variable>

    <xsl:element name="CodeFile">
      <xsl:for-each select="$Utils/Util">
        <xsl:value-of select="concat('var ', @UtilName, ' = { ')" />
        <xsl:for-each select="Function">
          <xsl:variable name="params" select="string-join(Params/Param, ', ')" />
          <xsl:value-of select="concat(@FunctionName, ' : function(', $params, ') { ')" />
          <xsl:call-template name="processCode">
            <xsl:with-param name="code" select="string-join(FunctionBody/CodeLine, '&#x0A;')" />
            <xsl:with-param name="type" select="'both'" />
            <xsl:with-param name="delim" select="' '" />
          </xsl:call-template>
          <xsl:if test="position() eq last()">
            <xsl:value-of select="'}};&#x0A;'"/>
          </xsl:if>
          <xsl:if test="position() ne last()">
            <xsl:value-of select="'},'"/>
          </xsl:if>
        </xsl:for-each>
      </xsl:for-each>

      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$CodeFile/GlobalCode" />
        <xsl:with-param name="type" select="'vars'" />
        <xsl:with-param name="delim" select="' '" />
      </xsl:call-template>

      <xsl:for-each select="$CodeFile/Class">
        <xsl:variable name="className" select="@ClassName" />
        <xsl:variable name="params" select="string-join(Constructor/Params/Param, ', ')" />
        <xsl:value-of select="concat('function ', @ClassName, '(', $params, ') {')" />
        <xsl:call-template name="processCode">
          <xsl:with-param name="code" select="Constructor/ConstructorBody" />
          <xsl:with-param name="type" select="'both'" />
          <xsl:with-param name="delim" select="' '" />
        </xsl:call-template>
        <xsl:value-of select="'} '" />
        <xsl:value-of select="concat($className, '.prototype.constructor = ', $className, '; ')" />
        <xsl:for-each select="PrototypeChain/MemberFunction" >
          <xsl:variable name="params" select="string-join(Params/Param, ', ')" />
          <xsl:value-of select="concat($className, '.prototype.', @FunctionName, ' = function(', $params, ') {')" />
          <xsl:call-template name="processCode">
            <xsl:with-param name="code" select="FunctionBody" />
            <xsl:with-param name="type" select="'both'" />
            <xsl:with-param name="delim" select="' '" />
          </xsl:call-template>
          <xsl:value-of select="'}; '" />
        </xsl:for-each>
        <xsl:value-of select="'&#x0A;'" />
      </xsl:for-each>

      <xsl:for-each select="$CodeFile/Function">
        <xsl:variable name="params" select="string-join(Params/Param, ', ')" />
        <xsl:value-of select="concat('function ', @FunctionName, '(', $params, ') { ')" />
        <xsl:call-template name="processCode">
          <xsl:with-param name="code" select="FunctionBody" />
          <xsl:with-param name="type" select="'both'" />
          <xsl:with-param name="delim" select="' '" />
        </xsl:call-template>
        <xsl:value-of select="'}&#x0A;'"/>
      </xsl:for-each>

      <xsl:call-template name="processCode">
        <xsl:with-param name="code" select="$CodeFile/GlobalCode" />
        <xsl:with-param name="type" select="'both'" />
        <xsl:with-param name="delim" select="' '" />
      </xsl:call-template>
    </xsl:element>
  </xsl:template>

  <xsl:template name="processCode">
    <xsl:param name="code"/>
    <xsl:param name="type"/>
    <xsl:param name="delim" />
    <xsl:variable name="codeList">
      <xsl:analyze-string select="$code" regex="(([\)\{{\}};])|else)\s*?&#x0A;" >
        <xsl:non-matching-substring>
          <xsl:variable name="line" select="normalize-space(.)" />
          <xsl:choose>
            <xsl:when test="matches($line, '^var\s+?([A-Za-z_][A-Za-z0-9_]*)(\s*=((\s+|[^;=/,&#x22;\(]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)+?)?)*')">
              <xsl:analyze-string select="replace($line, '(var\s+?)(.+)', '$2')" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*(=((\s+|[^;=/,&#x22;\(]+?|&#x22;[^&#x22;\n\r]*?&#x22;|\(([^;=,&#x22;]*?,?(&#x22;[^\n\r&#x22;]*?&#x22;)?)+\)|/[^/\n]+?/)*)+?)?)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'" />
                    <xsl:value-of select="regex-group(1)" disable-output-escaping="yes" />
                  </xsl:element>
                  <xsl:if test="string-length(regex-group(2)) gt 0">
                    <xsl:element name="code">
                      <xsl:attribute name="type" select="'varAssign'" />
                      <xsl:value-of select="regex-group(2)" disable-output-escaping="no" />
                    </xsl:element>
                  </xsl:if>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'subLineDelim'" />
                    <xsl:value-of select="';'" />
                  </xsl:element>
                </xsl:matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:when test="matches($line, '([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)')">
              <xsl:analyze-string select="$line" regex="([^A-Za-z0-9_])(var\s+)([A-Za-z0-9][A-Za-z0-9_]+)">
                <xsl:matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'" />
                    <xsl:value-of select="regex-group(1)" disable-output-escaping="yes" />
                  </xsl:element>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'varName'" />
                    <xsl:value-of select="regex-group(3)" disable-output-escaping="yes" />
                  </xsl:element>
                </xsl:matching-substring>
                <xsl:non-matching-substring>
                  <xsl:element name="code">
                    <xsl:attribute name="type" select="'code'" />
                    <xsl:value-of select="." disable-output-escaping="yes" />
                  </xsl:element>
                </xsl:non-matching-substring>
              </xsl:analyze-string>
            </xsl:when>
            <xsl:otherwise>
              <xsl:element name="code">
                <xsl:attribute name="type" select="'code'" />
                <xsl:value-of select="$line" disable-output-escaping="yes" />
              </xsl:element>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:non-matching-substring>
        <xsl:matching-substring>
          <xsl:element name="code">
            <xsl:attribute name="type" select="'subLineDelim'" />
            <xsl:value-of select="regex-group(1)" disable-output-escaping="yes" />
          </xsl:element>
        </xsl:matching-substring>
      </xsl:analyze-string>
    </xsl:variable>
    <xsl:if test="(($type eq 'vars') or ($type eq 'both')) and (count($codeList/code[@type eq 'varName']) gt 0)">
      <xsl:value-of select="'var '" />
      <xsl:for-each select="$codeList/code[((@type eq 'varName') and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies normalize-space(.) ne normalize-space($var))) or (@type eq 'varAssign')]">
        <xsl:variable name="varName" select="." />
        <xsl:if test="@type eq 'varName'">
          <xsl:choose>
            <xsl:when test="(position() eq last()) and (every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName)">
              <xsl:value-of select="." disable-output-escaping="no" />
            </xsl:when>
            <xsl:when test="(count(following-sibling::code[@type eq 'varName']) gt 0) and (some $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar ne $varName) and (every $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $precedingVar ne $followingVar)) and (following-sibling::code[1]/@type ne 'varAssign')" >
              <xsl:value-of select="concat(., ', ')" disable-output-escaping="yes" />
            </xsl:when>
            <xsl:when test="following-sibling::code[1]/@type eq 'varAssign'">
              <xsl:variable name="assign" select="following-sibling::code[1]" />
              <xsl:choose>
                <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                  <xsl:if test="position() + 1 eq last()" >
                    <xsl:value-of select="." disable-output-escaping="yes" />
                  </xsl:if>
                  <xsl:if test="position() + 1 ne last()" >
                    <xsl:value-of select="concat(., ', ')" />
                  </xsl:if>
                </xsl:when>
                <xsl:when test="(every $var in preceding-sibling::code[@type eq 'varName'] satisfies $var ne $varName) and (every $var in following-sibling::code[@type eq 'varName'] satisfies $var ne $varName)" >
                  <xsl:if test="position() + 1 eq last()" >
                    <xsl:value-of select="concat(., following-sibling::code[1])" />
                  </xsl:if>
                  <xsl:if test="position() + 1 ne last()" >
                    <xsl:value-of select="concat(., following-sibling::code[1], ', ')" />
                  </xsl:if>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="(position() + 1 eq last()) or (every $followingVar in following-sibling::code[@type eq 'varName'] satisfies ($followingVar eq $varName) or (some $precedingVar in preceding-sibling::code[@type eq 'varName'] satisfies $followingVar eq $precedingVar))" >
                      <xsl:value-of select="." disable-output-escaping="yes" />
                    </xsl:when>
                    <xsl:when test="position() + 1 ne last()" >
                      <xsl:value-of select="concat(., ', ')" />
                    </xsl:when>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
      </xsl:for-each>
      <xsl:value-of select="concat(';', $delim)" />
    </xsl:if>
    <xsl:if test="($type eq 'code') or ($type eq 'both')" >
      <xsl:for-each select="$codeList/code">
        <xsl:choose>
          <xsl:when test="@type eq 'code'" >
            <xsl:value-of select="." disable-output-escaping="yes" />
          </xsl:when>
          <xsl:when test="(@type eq 'varAssign') and (preceding-sibling::code[1]/@type eq 'varName')" >
            <xsl:variable name="assign" select="normalize-space(.)" />
            <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]" />
            <xsl:choose>
              <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" disable-output-escaping="yes" />
              </xsl:when>
              <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)" >
                <xsl:value-of select="concat(preceding-sibling::code[1], .)" disable-output-escaping="yes" />
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="(@type eq 'varName') and (position() gt 1)">
            <xsl:if test="preceding-sibling::code[1]/@type eq 'code'" >
              <xsl:value-of select="." disable-output-escaping="yes" />
            </xsl:if>
          </xsl:when>
          <xsl:when test="@type eq 'subLineDelim'">
            <xsl:if test="matches(., '^[^;]')" >
              <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes" />
            </xsl:if>
            <xsl:if test="matches(., '^;')" >
              <xsl:choose>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varAssign'" >
                  <xsl:if test="preceding-sibling::code[2]/@type eq 'varName'" >
                    <xsl:variable name="assign" select="normalize-space(preceding-sibling::code[@type eq 'varAssign'][1])" />
                    <xsl:variable name="thisVarName" select="preceding-sibling::code[@type eq 'varName'][1]" />
                    <xsl:choose>
                      <xsl:when test="some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies matches($assign, concat('[^A-Za-z0-9_]?', normalize-space($var), '[^A-Za-z0-9_]?'))">
                        <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes" />
                      </xsl:when>
                      <xsl:when test="(some $var in preceding-sibling::code[@type eq 'varName'][position() gt 1] satisfies $var eq $thisVarName) or (some $var in following-sibling::code[@type eq 'varName'] satisfies $var eq $thisVarName)">
                        <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes" />
                      </xsl:when>
                    </xsl:choose>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'varName'" >
                  <xsl:variable name="varName" select="preceding-sibling::code[1]" />
                  <xsl:if test="position() gt 2">
                    <xsl:if test="every $elem in preceding-sibling::code[@type eq 'varName'] satisfies $elem ne $varName" >
                      <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes" />
                    </xsl:if>
                  </xsl:if>
                </xsl:when>
                <xsl:when test="preceding-sibling::code[1]/@type eq 'code'">
                  <xsl:value-of select="concat(., $delim)" disable-output-escaping="yes" />
                </xsl:when>
              </xsl:choose>
            </xsl:if>
          </xsl:when>
        </xsl:choose>
      </xsl:for-each>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
