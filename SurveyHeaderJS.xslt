<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
								xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
								version="2.0"
								exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes" cdata-section-elements="Function Declaration FunctionConstructor"/>


  <xsl:variable name="root" select="/" />

  <xsl:variable name="variableDeclarations">
    <Declarations>
      <Declaration>var TestURL;</Declaration>
      <Declaration>var AppURL;</Declaration>
      <Declaration>var StopTimer;</Declaration>
      <Declaration>var CryptJSON;</Declaration>
      <Declaration>var UniqueResponseViolation;</Declaration>
      <Declaration>
        <xsl:value-of select="concat('var adminHost = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', //ServerPath, '/Admin&quot;;')" />
      </Declaration>
    </Declarations>
  </xsl:variable>

  <xsl:variable name="functionPrefix">
    <xsl:value-of select="'shF'"/>
  </xsl:variable>

  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'shG'"/>
  </xsl:variable>

  <xsl:variable name="globalCodePrefix">
    <xsl:value-of select="'shGC'"/>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $variableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1;'), '')" />
    <xsl:analyze-string select="$Globals" regex="([A-Za-z_][A-Za-z0-9_]*)(\s*=\s*((\[|\s+|[^,;=/&#34;\(\[\]]+|(&#34;[^&#xA;&#xD;&#34;]*?&#34;)+|\(\)||\(([^;,=&#34;]*(,)?(&#34;[^&#xA;&#xD;&#34;]*?&#34;)?)*?\)|/[^/\n]+?/|\](\s*,)?)+))?(\s*(,|;))">
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
            <xsl:value-of select="regex-group(3)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>

  <xsl:variable name="Functions">
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnRetrieveScript'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">document.getElementById("SubmitButton").disabled = false;</xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnUnload'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">CookieUtil.set("CurrentIAT", "");</xsl:element>
      </xsl:element>
    </xsl:element>


    <xsl:element name="Function">
      <xsl:attribute name="FunctionName" select="'OnAjaxSuccess'"/>
      <xsl:element name="Params">
        <xsl:element name="Param">ajaxResult</xsl:element>
      </xsl:element>
      <xsl:variable name="functionBody">
        <xsl:text>
                var decryptor;
                eval.call(window, ajaxResult);
        </xsl:text>
        <xsl:value-of select="concat('var clientID = ', //Survey/@ClientID, ';&#x0A;')" />
        <xsl:value-of select="concat('var testName = &quot;', //Survey/@IAT, '&quot;;&#x0A;')" />
        <xsl:value-of select="concat('var testElem = &quot;', //Survey/@FileName, '&quot;;&#x0A;')" />
        <xsl:value-of select="'var segmentID = CookieUtil.get(&quot;TestSegment&quot;);&#x0A;'" />
        <xsl:value-of select="concat('decryptor = new Decryptor(adminHost, &quot;', //ServerPath, '&quot;, OnRetrieveScript);&#x0A;')" />
        <xsl:value-of select="'decryptor.fetchKeys(segmentID, adminHost + &quot;/Ajax/RSA&quot;);&#x0A;'" />
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
      <xsl:attribute name="FunctionName" select="'OnLoad'"/>
      <xsl:element name="Params"/>
      <xsl:variable name="functionBody">
        <xsl:value-of select="concat('var testElem = &quot;', //Survey/@FileName, '&quot;;&#x0A;')" />
        <xsl:value-of select="concat('var requestSrc = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', $root//ServerPath, '&quot; + &quot;/', //ClientID, '/', //IATName, '/', //Survey/@FileName, '.html&quot;;&#x0A;')"/>
        <xsl:if test="//Survey/@UniqueResponseItem ne '-1'">
          <xsl:value-of select="concat('EventUtil.addHandler(document.getElementById(&quot;Item', //Survey/@UniqueResponseItem, '&quot;), &quot;onblur&quot;);&#x0A;')"/>
        </xsl:if>
        <xsl:text>
          var ctr, xmlDoc, ajaxCall;
          document.getElementById("mainContent").focus();
          CookieUtil.set("CurrentIAT", "true");
          document.getElementById("SubmitButton").disabled = true;
          var segmentID = CookieUtil.get("TestSegment");
          ajaxCall = new AjaxCall(adminHost + "/Ajax/AES", requestSrc, "", segmentID);
          xmlDoc = ajaxCall.buildRequestDocument("AES.dat", "", "File");
          ajaxCall.call(xmlDoc, OnAjaxSuccess);
        </xsl:text>
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

    <xsl:if test="//Survey/@UniqueResponseItem ne '-1'">
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'CheckUniqueResponse'" />
        <xsl:element name="Params" />
        <xsl:variable name="functionBody">
          <xsl:value-of select="concat('var testElem = &quot;', //Survey/@FileName, '&quot;;&#x0A;')" />
          <xsl:value-of select="concat('var requestSrc = window.location.protocol + &quot;//&quot; + window.location.hostname + (window.location.port ? &quot;:&quot; + window.location.port.toString() : &quot;&quot;) + &quot;', $root//ServerPath, '&quot; + &quot;/', //ClientID, '/', //IATName, '/', //Survey/@FileName, '.html&quot;;&#x0A;')"/>
          <xsl:value-of select="concat('var val = document.getElementById(&quot;Item', //Survey/@UniqueResponseItem, '&quot;).value;&#x0A;')" />
          <xsl:text>
          var ctr, xmlDoc, ajaxCall;
          var segmentID = CookieUtil.get("TestSegment");
          var ajaxURL = adminHost + "/Ajax/VerifyUniqueResponse";
          ajaxCall = new AjaxCall(ajaxURL, requestSrc, "", segmentID);
          xmlDoc = ajaxCall.buildRequestDocument("VerifyUniqueResponse", val, "VerifyUniqueResponse");
          document.getElementById("SubmitButton").enabled = false;
          ajaxCall.call(xmlDoc, OnUniqueResponseCheckReturn);
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
        <xsl:attribute name="FunctionName" select="'OnUniqueResponseCheckReturn'" />
        <xsl:element name="Params">
          <xsl:element name="Param">ajaxResult</xsl:element>
        </xsl:element>
        <xsl:variable name="functionCode">
          <xsl:text>
            UniqueResponseViolation = ajaxResult;
            document.getElementById("SubmitButton").enabled = true;
          </xsl:text>
        </xsl:variable>
        <xsl:element name="FunctionBody">
          <xsl:for-each select="tokenize($functionCode, '&#x0A;')">
            <xsl:if test="string-length(normalize-space(.)) gt 0">
              <xsl:element name="Code">
                <xsl:value-of select="normalize-space(.)" />
              </xsl:element>
            </xsl:if>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>


    </xsl:if>
  </xsl:variable>


  <xsl:variable name="GlobalCode">
  </xsl:variable>


  <xsl:template match="Survey">
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
      <xsl:element name="GlobalCode">
        <xsl:attribute name="CodePrefix" select="$globalCodePrefix" />
        <xsl:copy-of select="$GlobalCode" />
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
