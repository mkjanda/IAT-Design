<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes"/>

  <xsl:variable name="VariableDeclarations">
    <Declarations>
      <Declaration>var RSA;</Declaration>
      <Declaration>var KeyCipherWords;</Declaration>
    </Declarations>
  </xsl:variable>
<!--
  <xsl:variable name="classPrefix">
    <xsl:value-of select="'aesC'"/>
  </xsl:variable>

  <xsl:variable name="classFunctionPrefix">
    <xsl:value-of select="'aesCF'"/>
  </xsl:variable>
-->
  <xsl:variable name="globalVariablePrefix">
    <xsl:value-of select="'_pG'"/>
  </xsl:variable>

  <xsl:variable name="functionPrefix">
    <xsl:value-of select="_pjsF"/>
  </xsl:variable>

  <xsl:variable name="GlobalAbbreviations">
    <xsl:variable name="Globals" select="string-join(for $elem in $VariableDeclarations/Declarations/Declaration return replace($elem, '^var\s+(.+);$', '$1'), ', ')" />
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
            <xsl:value-of select="regex-group(2)" />
          </xsl:element>
        </xsl:element>
      </xsl:matching-substring>
    </xsl:analyze-string>
  </xsl:variable>


  <xsl:template match="/">
    <xsl:variable name="Functions">
      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'init'" />
        <xsl:element name="Params">
          <xsl:element name="Param">requestURL</xsl:element>
        </xsl:element>
        <xsl:variable name="functionBodyElems">
          <xsl:text>
            var ajaxCall = new XMLHttpRequest();
            ajaxCall.open("GET", window.location.protocol + "//" + window.location.hostname + window.location.pathname + "?GetKeys=public", false);
            ajaxCall.send();
            

            var ajaxCall = new AjaxCall(this.AjaxURL, this.RootURL, this.RequestSrc + this.TestElem + ".html", this.TestElem);
            var verFile = new Object();
            var xmlDoc = ajaxCall.buildRequestDocument("Keys", null, new Array(), null, "PublicKey");
            ajaxCall.call(xmlDoc, retrieveKeys, this);
            var RSA = eval(ajaxCall.textResponse);
            var keyCipherString = "";
            var randHexVal, hexVal, hexStr, ctr1, ctr2, ctr3, keyStr;
            var keyBytes = new Array();
            var KeyCipherWords = new Array();
            for (ctr1 = 0; ctr1 &lt; 4; ctr1++) {
              keyStr = "";
              for (ctr2 = 0; ctr2 &lt; 4; ctr2++) {
                randHexVal = Math.floor(Math.random() * 256);
                hexStr = randHexVal.toString(16);
                while (hexStr.length &lt; 2)
                  hexStr = "0" + hexStr;
                keyStr += hexStr;
                hexVal = 1;
                for (ctr3 = 0; ctr3 &lt; this.RSA.exponent; ctr3++)
                  hexVal = (hexVal * randHexVal) % this.RSA.modulus;
                keyBytes.push(hexVal.toString(16));
              }
            KeyCipherWords.push(parseInt("0x" + keyStr, 16));
          }
          ajaxCall = new XMLHttpRequest();
          var queryStr = "?GetKeys=symmetric";
          for (ctr1 = 0; ctr1 &lt; KeyCipherWords.length; ctr1++)
            queryStr += "&amp;Key" + ctr1.toString() + KeyCipherWords[ctr1];
          ajaxCall.open("GET", window.location.protocol + "//" + window.location.hostname + window.location.pathname + queryStr, false);
          ajaxCall.send();

          </xsl:text>
        </xsl:variable>
      </xsl:element>

      <xsl:element name="Function">
        <xsl:attribute name="FunctionName" select="'retrieveKeys'" />
        <xsl:element name="Params">
          <xsl:element name="Param">ajaxResponse</xsl:element>
        </xsl:element>
        <xsl:variable name="functionBodyElems">
          <xsl:text>
          </xsl:text>
        </xsl:variable>
      </xsl:element>

    </xsl:variable>
  </xsl:template>
</xsl:stylesheet>
