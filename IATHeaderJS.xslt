<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="root" select="/" />

  <xsl:variable name="serverURLParts">
    <xsl:analyze-string select="//ServerURL" regex="[^/]+">
      <xsl:matching-substring>
        <xsl:element name="serverURLPart">
          <xsl:if test="contains(., '.')">
            <xsl:if test="$root//ServerPort eq '80'">
              <xsl:value-of select="." />
            </xsl:if>
            <xsl:if test="$root//ServerPort ne '80'">
              <xsl:value-of select="concat(., ':', $root//ServerPort)" />
            </xsl:if>
          </xsl:if>
          <xsl:if test="not(contains(., '.'))">
            <xsl:value-of select="." />
          </xsl:if>
        </xsl:element>
      </xsl:matching-substring>
      <xsl:non-matching-substring>
        <xsl:element name="serverURLPart">
          <xsl:value-of select="." />
        </xsl:element>
      </xsl:non-matching-substring>
    </xsl:analyze-string>
  </xsl:variable>
  <xsl:variable name="serverURL">
    <xsl:value-of select="string-join($serverURLParts/serverURLPart, '')" />
  </xsl:variable>
  <xsl:variable name="testURL">
    <xsl:variable name="host">
      <xsl:if test="$root//ServerPort eq '80'">
        <xsl:value-of select="concat('http://', $root//ServerDomain, $root//ServerPath)" />
      </xsl:if>
      <xsl:if test="$root//ServerPort ne '80'">
        <xsl:value-of select="concat('http://', $root//ServerDomain, ':', $root//ServerPort, $root//ServerPath)" />
      </xsl:if>
    </xsl:variable>
    <xsl:value-of select="concat($host, '/', //ClientID, '/', //IATName)" />
  </xsl:variable>

  <xsl:variable name="variableDeclarations">
    <Declarations>
      <xsl:element name="Declaration">
        <xsl:value-of select="concat('var NumImages = ', count(distinct-values(//IATDisplayItem/Filename)), ';')"/>
      </xsl:element>
      <xsl:for-each select="//IATDisplayItem">
        <xsl:element name="Declaration">
          <xsl:value-of select="concat('var img', ID, ';')"/>
        </xsl:element>
      </xsl:for-each>
      <Declaration>var imgTable;</Declaration>
      <Declaration>var ImageLoadComplete = false;</Declaration>
      <Declaration>var CodeProcessingComplete = false;</Declaration>
      <Declaration>var ImageLoadCtr = 0;</Declaration>
      <Declaration>var ImageLoadStatusTextElement;</Declaration>
      <Declaration>var ClickToStartElement;</Declaration>
      <Declaration>var ClickToStartText;</Declaration>
      <Declaration>var abort = false;</Declaration>
      <Declaration>var AllImagesLoaded = false;</Declaration>
      <xsl:if test="count(//DynamicSpecifier) gt 0">
        <Declaration>var DynamicSpecValues;</Declaration>
        <Declaration>var DynamicSpecValuesLoaded = false;</Declaration>
      </xsl:if>
      <Declaration>
        <xsl:value-of select="concat('var adminHost = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', $root//ServerPath, '/Admin&quot;;')" />
      </Declaration>
    </Declarations>
  </xsl:variable>


  <xsl:variable name="functionPrefix">
    <xsl:value-of select="'ihF'"/>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'ihG'"/>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $variableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1,,'), '')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*(\[|\s+|[^,;=/&#34;\(\[\]]+|(&#34;[^&#xA;&#xD;&#34;]*?&#34;)+|\(([^;,=&#34;]*(,)?(&#34;[^&#xA;&#xD;&#34;]*?&#34;)?)*?\)|/[^/\n]+?/|\](\s*,)?)+)?,,">
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
            <xsl:value-of select="regex-group(3)"/>
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:variable name="Functions">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnImageLoad'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBody">
        <xsl:text>
              ImageLoadCtr++;
                ImageLoadStatusTextElement.nodeValue = "Loading image #" + (ImageLoadCtr + 1).toString() + " of " + NumImages.toString();
              if (ImageLoadCtr == NumImages) 
                ImageLoadCompleted();
     </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')" >
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnImageLoadError'" />
      <xsl:element name="Params">
        <xsl:element name="Param">event</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBody">
        <xsl:text>
            var e = EventUtil.getEvent(event); 
            var img = e.currentTarget;
            imgTable[img.src] = new Image();
            EventUtil.addHandler(imgTable[img.src], 'load', OnImageLoad);
            EventUtil.addHandler(imgTable[img.src], 'error', OnImageLoadError);
            imgTable[img.src].src = img.src;
     </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')" >
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>


    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'StartImageLoad'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBody">
        <xsl:variable name="imgAry">
          <xsl:for-each select="distinct-values(//IATDisplayItem/Filename)">
            <xsl:element name="ImageTableEntry">
              <xsl:value-of select="concat('&quot;', $testURL, '/', ., '&quot; : new Image()')"/>
            </xsl:element>
          </xsl:for-each>
        </xsl:variable>
        <xsl:value-of select="concat('imgTable = { ', string-join($imgAry/ImageTableEntry, ', '), ' };')" />
        <xsl:text>
              var LoadingImagesElement = document.createElement("h3");
              var LoadingImagesText = document.createTextNode("Please Wait");
              LoadingImagesElement.appendChild(LoadingImagesText);
              var LoadingImagesProgressElement = document.createElement("h4");
              ImageLoadStatusTextElement = document.createTextNode("Loading image #1 of " + NumImages.toString());
              LoadingImagesProgressElement.appendChild(ImageLoadStatusTextElement);
              var displayDiv = document.getElementById("IATDisplayDiv");
              displayDiv.appendChild(LoadingImagesElement);
              displayDiv.appendChild(LoadingImagesProgressElement);
          </xsl:text>
        <xsl:for-each select="//IATDisplayItem">
          <xsl:value-of select="concat('EventUtil.addHandler(imgTable[&quot;', $testURL, '/', Filename, '&quot;], &quot;load&quot;, OnImageLoad);&#x0A;')" />
          <xsl:value-of select="concat('EventUtil.addHandler(imgTable[&quot;', $testURL, '/', Filename, '&quot;], &quot;error&quot;, OnImageLoadError);&#x0A;')"/>
          <xsl:value-of select="concat('imgTable[&quot;', $testURL, '/', Filename, '&quot;].src = &quot;', $testURL, '/',  Filename, '?', $root//UploadTimeMillis, '&quot;;&#x0A;')" />
        </xsl:for-each>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)"/>
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'ImageLoadCompleted'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBodyCode">
        <xsl:for-each select="//IATDisplayItem">
          <xsl:value-of select="concat('img', ID, ' = imgTable[&quot;', $testURL, '/', Filename, '&quot;];&#x0A;')" />
        </xsl:for-each>
        <xsl:text>
          TestReady("Images");
        </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBodyCode, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'CodeLoadCompleted'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBodyCode">
        <xsl:text>
            TestReady("Code");
        </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBodyCode, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>


    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'TestReady'" />
      <xsl:element name="Params">
        <xsl:element name="Param">elem</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBody">
        <xsl:text>
            var displayDiv = document.getElementById("IATDisplayDiv");
              if (elem == "Images")
                ImageLoadComplete = true;
              else if (elem == "Code")
                CodeProcessingComplete = true;
        </xsl:text>
        <xsl:if test="count(//DynamicSpecifier) gt 0">
          <xsl:text>
            else if (elem == "DynamicSpecifiers")
               DynamicSpecValuesLoaded = true;
            if (ImageLoadComplete &amp;&amp; CodeProcessingComplete &amp;&amp; DynamicSpecValuesLoaded) {
          </xsl:text>
        </xsl:if>
        <xsl:if test="count(//DynamicSpecifier) eq 0">
          <xsl:value-of select="'if (ImageLoadComplete &amp;&amp; CodeProcessingComplete) {&#x0A;'"/>
        </xsl:if>
        <xsl:text>
                while (displayDiv.firstChild)
                  displayDiv.removeChild(displayDiv.firstChild);
                ClickToStartElement = document.createElement("h4");
                ClickToStartText = document.createTextNode("Click Here to Begin");
                ClickToStartElement.appendChild(ClickToStartText);
                displayDiv.appendChild(ClickToStartElement);
                EventUtil.addHandler(window, "click", BeginIAT);
              } else if (ImageLoadComplete) {
                while (displayDiv.firstChild)
                  displayDiv.removeChild(displayDiv.firstChild);
                var PleaseWaitElement = document.createElement("h3");
                var PleaseWaitText = document.createTextNode("Preparing Administration");
                PleaseWaitElement.appendChild(PleaseWaitText);
                displayDiv.appendChild(PleaseWaitElement);
              }
        </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'BeginIAT'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">BeginIAT();</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnAjaxSuccess'" />
      <xsl:element name="Params">
        <xsl:element name="Param">ajaxResult</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBody">
        <xsl:text>
                var decryptor;
                eval.call(window, ajaxResult);
        </xsl:text>
        <xsl:value-of select="concat('var clientID = ', //ClientID, ';&#x0A;')" />
        <xsl:value-of select="concat('var iatName = &quot;', //IATName, '&quot;;&#x0A;')" />
        <xsl:value-of select="concat('var testElem = &quot;', //IATName, '&quot;;&#x0A;')" />
        <xsl:value-of select="concat('decryptor = new Decryptor(adminHost, &quot;', $root//ServerPath, '&quot;, CodeLoadCompleted);&#x0A;')" />
        <xsl:value-of select="'decryptor.fetchKeys(CookieUtil.get(&quot;TestSegment&quot;), adminHost + &quot;/Ajax/RSA&quot;);'" />
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>



    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnUnload'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">CookieUtil.set("CurrentIAT", "");</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:if test="count(//DynamicSpecifier)">
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'OnDynamicSpecLoadRSChange'" />
        <xsl:element name="Params">
          <xsl:element name="Param">xhttp</xsl:element>
        </xsl:element>
        <xsl:variable name="functionBody">
          <xsl:text>
            if ((xhttp.readyState == 4) &amp;&amp; (xhttp.status == 200)) {
              DynamicSpecValues = JSON.parse(xhttp.responseText);
              TestReady("DynamicSpecifiers");
            }
          </xsl:text>
        </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
      </xsl:element>
    </xsl:if>
    
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnLoad'" />
      <xsl:element name="Params" />
      <xsl:variable name="functionBody">
        <xsl:if test="count(//DynamicSpecifier) gt 0">
          <xsl:text>
            var xhttp = new XMLHttpRequest();
            xhttp.open("GET", adminHost + "/Ajax/DynamicSpecifiers.json?" + (new Date()).getTime(), true);
            xhttp.setRequestHeader("Accept", "application/json");
            xhttp.onreadystatechange = function() { OnDynamicSpecLoadRSChange(this); };
            xhttp.send();
          </xsl:text>
        </xsl:if>
        <xsl:value-of select="concat('var requestSrc = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', $root//ServerPath, '&quot; + &quot;/', //ClientID, '/', //IATName, '/', //IATName, '.html&quot;;&#x0A;')"/>
        <xsl:value-of select="concat('var testElem = &quot;', //IATName, '&quot;;&#x0A;')" />
        <xsl:text>
          var testSegment = CookieUtil.get("TestSegment");
          var ajaxCall;
          DisplayDiv = document.getElementById("IATDisplayDiv");
          StartImageLoad();
          CookieUtil.set("CurrentIAT", "true");
          var alternateTag = document.getElementById("Alternate");
          alternateTag.setAttribute("value", CookieUtil.get("Alternate"));
          ajaxCall = new AjaxCall(adminHost + "/Ajax/AES", requestSrc, "", testSegment);
          var xmlDoc = ajaxCall.buildRequestDocument("AES.dat", "", "File");
          ajaxCall.call(xmlDoc, OnAjaxSuccess);
        </xsl:text>
      </xsl:variable>
      <xsl:element name="FunctionBody">
        <xsl:for-each select="tokenize($functionBody, '&#x0A;')">
          <xsl:if test="string-length(normalize-space(.)) gt 0">
            <xsl:element name="Code">
              <xsl:value-of select="normalize-space(.)" />
            </xsl:element>
          </xsl:if>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnRetrieveScript'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">TestElemReady("Code");</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:variable>
  <!--
  <xsl:variable name="GlobalCode">
    <xsl:element name="Code">EventUtil.addHandler(body, "unload", OnUnload);</xsl:element>
  </xsl:variable>
-->
  <xsl:template match="ConfigFile">
    <xsl:element name="CodeFile">

      <xsl:element name="VarEntries">
        <xsl:copy-of select="$GlobalAbbreviations" />
      </xsl:element>
      <xsl:element name="Functions">
        <xsl:for-each select="$Functions/Function">
          <xsl:variable name="nodeName" select="name()" />
          <xsl:element name="{$nodeName}">
            <xsl:for-each select="attribute::*">
              <xsl:copy-of select="."/>
            </xsl:for-each>
            <xsl:attribute name="FunctionPrefix" select="$functionPrefix" />
            <xsl:copy-of select="child::*" />
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
      <!--<xsl:copy-of select="$GlobalCode"/>-->
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>

