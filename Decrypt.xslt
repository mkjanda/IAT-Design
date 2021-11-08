<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="text" encoding="utf-8" indent="yes"/>

  <xsl:template match="ExecutionData">
    <xsl:text>
      function decryptor()
      {
         this.B64 = new FromBase64();
         this.currLine = 0;
         this.evalLines = new Array();
    </xsl:text>
    <xsl:value-of select="concat('this.numLines = ', count(Data), ';')" />
    <xsl:for-each select="Data">
      this.evalLines.push(function(xml, crypt, b64) {
      var words = b64.decodeWords(xml);
      var dAry = new Array();
      dAry = dAry.concat(
      <xsl:variable name="lines" select="for $i in 1 to xs:integer(floor(xs:integer(@Param1) div 16)) return 'crypt.decrypt(words.splice(0, 4))'" />
      <xsl:value-of select="string-join($lines, ',&#x0A; ')" />
      <xsl:variable name="modulus" select="xs:integer(@Param1) mod 16" />
      <xsl:choose>
        <xsl:when test="($modulus gt 0) and ($modulus le 4)">
          <xsl:value-of select="',&#x0A; crypt.decrypt(new Array(words.splice(0, 1), 0, 0, 0))'"/>
        </xsl:when>
        <xsl:when test="($modulus gt 4) and ($modulus le 8)">
          <xsl:value-of select="',&#x0A; crypt.decrypt(words.splice(0, 2).concat(0, 0))'" />
        </xsl:when>
        <xsl:when test="($modulus gt 8) and ($modulus le 12)">
          <xsl:value-of select="',&#x0A; crypt.decrypt(words.splice(0, 3).concat(0))'"/>
        </xsl:when>
        <xsl:when test="($modulus gt 12) and ($modulus lt 16)">
          <xsl:value-of select="',&#x0A; crypt.decrypt(words.splice(0, 4))'"/>
        </xsl:when>
      </xsl:choose>
      );
      var str = "";
      for (var ctr = 0; ctr &lt; dAry.length; ctr++)
      {
      str += String.fromCharCode((dAry[ctr] >> 24) &amp; 255);
      str += String.fromCharCode((dAry[ctr] >> 16) &amp; 255);
      str += String.fromCharCode((dAry[ctr] >> 8) &amp; 255);
      str += String.fromCharCode(dAry[ctr] &amp; 255);
      }
      <xsl:value-of select="concat('str = str.substring(0, ', @Param1, ');')" />
      str.eval();
      });
    </xsl:for-each>
    <xsl:text>
    }


    decryptor.prototype = {
      constructor: decryptor,
      getNextLineNum: function(xml) { 


        },
      evalLine: function(lineNum, xml) { 
        var crypt = new AES(new Array(
        (new String(parseInt(xml.attributes[1].value.substring(0,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(0)))*2,2),16))),
        (new String(parseInt(xml.attributes[1].value.substring(2,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(1)))*2,2),16))),
        (new String(parseInt(xml.attributes[1].value.substring(4,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(2)))*2,2),16))), 
        (new String(parseInt(xml.attributes[1].value.substring(6,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(3)))*2,2),16))),
        (new String(parseInt(xml.attributes[1].value.substring(8,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(4)))*2,2),16))), 
        (new String(parseInt(xml.attributes[1].value.substring(10,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(5)))*2,2),16))), 
        (new String(parseInt(xml.attributes[1].value.substring(12,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(6)))*2,2),16))), 
        (new String(parseInt(xml.attributes[1].value.substring(14,2),16)^parseInt(xml.attributes[2].value.substring((new Number(CookieUtil.get("MiscData").charAt(7)))*2,2),16)))));
        this.evalLines[lineNum](xml.text, crypt, this.B64);
      }
    };
    </xsl:text>
  </xsl:template>
</xsl:stylesheet>