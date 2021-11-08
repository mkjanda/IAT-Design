<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="xml" encoding="UTF-8" omit-xml-declaration="yes" indent="yes" />

  <xsl:variable name="root" select="/" />

  <xsl:template match="ConfigFile" >
    <html>
      <head>
        <xsl:element name="style">
          <xsl:attribute name="type" select="'text/css'" />
          <xsl:apply-templates select="DisplayItemList" />
          .outlinedDI
          {
          border: 1px solid #<xsl:value-of select="concat(./Layout/OutlineColorR, ./Layout/OutlineColorG, ./Layout/OutlineColorB), ';'" />
          }

          body {
          background: #<xsl:value-of select="concat(./Layout/PageBackColorR, ./Layout/PageBackColorG, ./Layout/PageBackColorB, ';')" />
          }

          #Message {
          width: 100%;
          text-align: center;
          }

          #IATContainerDiv {
          margin: auto;
          text-align: center;
          background: #<xsl:value-of select="concat(./Layout/PageBackColorR, ./Layout/PageBackColorG, ./Layout/PageBackColorB), ';'" />
          }

          #IATDisplayDiv {
          width: <xsl:value-of select="concat(xs:integer(./Layout/InteriorWidth) + (xs:integer(./Layout/BorderWidth) * 2), 'px;')" />
          height: <xsl:value-of select="concat(xs:integer(./Layout/InteriorHeight) + (xs:integer(./Layout/BorderWidth) * 2), 'px;')" />
          border: <xsl:value-of select="concat(./Layout/BorderWidth, 'px solid #', ./Layout/BorderColorR, ./Layout/BorderColorG, ./Layout/BorderColorB, ';')" />
          background: #<xsl:value-of select="concat(./Layout/BackColorR, ./Layout/BackColorG, ./Layout/BackColorB), ';'" />
          position: relative;
          top: 10px;
          left: 10px;
          margin: 10px auto 10px auto;
          text-align: left;
          }

          #IATDisplayDiv h3 {
          font-family: "Times New Roman", Times, serif;
          font-size: <xsl:value-of select="xs:integer(./Layout/InteriorHeight div 12)" />px;
          color: #<xsl:value-of select="concat(./Layout/BorderColorR, ./Layout/BorderColorG, ./Layout/BorderColorB, ';')" />
          margin-top: <xsl:value-of select="xs:integer(./Layout/InteriorHeight div 10)" />px;
          margin-bottom: <xsl:value-of select="xs:integer(./Layout/InteriorHeight) div 10" />px;
          text-align: center;
          font-weight: bold;
          }

          #IATDisplayDiv h4 {
          font-family: "Times New Roman", Times, serif;
          font-size: <xsl:value-of select="xs:integer(./Layout/InteriorHeight div 18)" />px;
          color: #<xsl:value-of select="concat(./Layout/BorderColorR, ./Layout/BorderColorG, ./Layout/BorderColorB, ';')" />
          margin-top: <xsl:value-of select="xs:integer(./Layout/InteriorHeight div 10)" />px;
          margin-bottom: <xsl:value-of select="xs:integer(./Layout/InteriorHeight div 10)" />px;
          text-align: center;
          }

          .AjaxErrorDiv {
          text-align: left;
          width: 980px;
          color: #CCCCCC;
          margin-top: 20px;
          margin-left: auto;
          margin-right: auto;
          }

          .AjaxErrorMsg {
          text-align: center;
          font-family: Arial, Helvetica, sans-serif;
          color: #000000;
          font-size: 32px;
          }

          .AjaxErrorDetail {
          font-family: "Times New Roman", Times, serif;
          font-size: 18px;
          color: #000000;
          }
        </xsl:element>
        <title>
          <xsl:value-of select="./IATName" />
        </title>
        <xsl:variable name="urlParts">
          <urlPart>http://</urlPart>
          <urlPart>
            <xsl:value-of select="./ServerDomain"/>
          </urlPart>
          <xsl:if test="./ServerPort eq '80'">
            <urlPart>
              <xsl:value-of select="concat(':', ./ServerPort)" />
            </urlPart>
          </xsl:if>
          <urlPart>
            <xsl:value-of select="./ServerPath"/>
          </urlPart>
        </xsl:variable>
        <xsl:variable name="url">
          <xsl:value-of select="string-join($urlParts/urlPart, '')" />
        </xsl:variable>
        <xsl:element name="script">
          <xsl:attribute name="charset" select="'UTF-8'" />
          <xsl:attribute name="type" select="'text/javascript'" />
          <xsl:attribute name="src" select="concat($url, '/SubFunct.js')" />
          <xsl:value-of select="' '"/>
        </xsl:element>
        <xsl:element name="script">
          <xsl:attribute name="charset" select="'UTF-8'" />
          <xsl:attribute name="type" select="'text/javascript'"/>
          <xsl:attribute name="src" select="concat($url, '/core_aes.js')"/>
          <xsl:value-of select="' '"/>
        </xsl:element>
        <xsl:element name="script">
          <xsl:attribute name="charset" select="'UTF-8'" />
          <xsl:attribute name="type" select="'text/javascript'"/>
          <xsl:attribute name="src" select="concat($url, '/MiscUtils.js')"/>
          <xsl:value-of select="' '"/>
        </xsl:element>
        <xsl:element name="script">
          <xsl:attribute name="charset" select="'UTF-8'" />
          <xsl:attribute name="type" select="'text/javascript'" />
          <xsl:attribute name="src" select="concat($url, '/AjaxCall.js')" />
          <xsl:value-of select="' '"/>
        </xsl:element>
        <xsl:element name="script">
          <xsl:attribute name="charset" select="'UTF-8'" />
          <xsl:attribute name="type" select="'text/javascript'" />
          <xsl:attribute name="src" select="concat($url, '/',  //ClientID, '/',//IATName, '/', //IATName, 'header.js?', $root//UploadTimeMillis)" />
          <xsl:value-of select="' '"/>
        </xsl:element>
      </head>
      <xsl:element name="body">
        <xsl:attribute name="id" select="'bodyID'" />
        <xsl:attribute name="onload" select="'OnLoad()'" />
        <xsl:attribute name="onunload" select="'OnUnload()'" />
        <xsl:element name="div">
          <xsl:attribute name="id" select="'IATContainerDiv'" />
          <xsl:element name="form" >
            <xsl:attribute name="method" select="'post'" />
            <xsl:attribute name="id" select="'IATForm'" />
            <xsl:apply-templates select="./DynamicSpecifiers" />
            <xsl:element name="input">
              <xsl:attribute name="type" select="'hidden'" />
              <xsl:attribute name="id" select="'hexLen'" />
            </xsl:element>
            <xsl:element name="input">
              <xsl:attribute name="type" select="'hidden'" />
              <xsl:attribute name="id" select="'k'" />
            </xsl:element>
            <xsl:element name="input">
              <xsl:attribute name="type" select="'hidden'" />
              <xsl:attribute name="id" select="'ek'" />
            </xsl:element>
            <xsl:for-each select="1 to 4">
              <xsl:element name="input">
                <xsl:attribute name="type" select="'hidden'" />
                <xsl:attribute name="id" select="concat('hexLine', xs:string(.))" />
              </xsl:element>
            </xsl:for-each>
            <xsl:element name="input" >
              <xsl:attribute name="type" select="'hidden'" />
              <xsl:attribute name="value" select="./IATName" />
              <xsl:attribute name="name" select="'AdministeredItem'" />
            </xsl:element>
            <xsl:element name="input" >
              <xsl:attribute name="id" select="'Alternate'" />
              <xsl:attribute name="type" select="'hidden'" />
            </xsl:element>
            <xsl:element name="input">
              <xsl:attribute name="name" select="'NumItems'" />
              <xsl:attribute name="value" select="sum(//IATEvent[@EventType eq 'BeginIATBlock']/NumPresentations)" />
              <xsl:attribute name="type" select="'hidden'" />
            </xsl:element>
            <xsl:element name="div">
              <xsl:attribute name="id" select="'IATDisplayDiv'" />
              <xsl:value-of select="' '"/>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </html>
  </xsl:template>


  <xsl:template match="DynamicSpecifier" >
    <xsl:element name="input">
      <xsl:attribute name="type" select="'hidden'" />
      <xsl:attribute name="id" select="concat('DynamicKey', ./ID)" />
      <xsl:attribute name="value" select="0" />
    </xsl:element>
  </xsl:template>

  <xsl:variable name="responseDisplayIDs" >
    <xsl:for-each select="/ConfigFile/IATEventList/IATEvent[@EventType eq 'BeginIATBlock']" >
      <xsl:element name="ResponseDisplayID">
        <xsl:value-of select="RightResponseDisplayID" />
      </xsl:element>
      <xsl:element name="ResponseDisplayID">
        <xsl:value-of select="LeftResponseDisplayID" />
      </xsl:element>
    </xsl:for-each>
  </xsl:variable>


  <xsl:template match="IATDisplayItem" >
    <xsl:value-of select="concat('#IATDI', ./ID)" />
    {
    position: absolute;
    <xsl:variable name="displayItem" select="." />
    <xsl:variable name="id" select="ID" />
    <xsl:if test="count($responseDisplayIDs[some $n in ResponseDisplayID satisfies xs:integer($n) eq xs:integer($id)]) gt 0" >
      <xsl:variable name="vertPadding" select="xs:string(ceiling((xs:integer(//Layout/ResponseHeight) - xs:integer($displayItem[$id eq ID]/Height)) div 2) - 5)" />
      <xsl:variable name="horizPadding" select="xs:string(ceiling((xs:integer(//Layout/ResponseWidth) - xs:integer($displayItem[$id eq ID]/Width)) div 2) - 5)" />
      <xsl:value-of select="concat('left: ', xs:integer(X) - xs:integer($horizPadding) + xs:integer(//Layout/BorderWidth), 'px;&#x0A;')" />
      <xsl:value-of select="concat('top: ', xs:integer(Y) - xs:integer($vertPadding) + xs:integer(//Layout/BorderWidth), 'px;&#x0A;')" />
      <xsl:value-of select="concat('padding-top: ', $vertPadding, 'px;&#x0A;')" />
      <xsl:value-of select="concat('padding-left: ', $horizPadding, 'px;&#x0A;')" />
      <xsl:value-of select="concat('padding-bottom: ', $vertPadding, 'px;&#x0A;')" />
      <xsl:value-of select="concat('padding-right: ', $horizPadding, 'px;&#x0A;')" />
    </xsl:if>
    <xsl:if test="count($responseDisplayIDs[some $n in ResponseDisplayID satisfies xs:integer($n) eq xs:integer($id)]) eq 0">
      <xsl:value-of select="concat('left: ', xs:integer(X) + xs:integer(//Layout/BorderWidth), 'px;&#x0A;')" />
      <xsl:value-of select="concat('top: ', xs:integer(Y) + xs:integer(//Layout/BorderWidth), 'px;&#x0A;')" />
      <xsl:value-of select="'padding: 0px;'"/>
    </xsl:if>
    margin: 0px;
    }
  </xsl:template>
</xsl:stylesheet>