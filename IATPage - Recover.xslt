﻿<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="html" encoding="UTF-8" doctype-public="-//W3C//DTD XHTML 1.0 Strict//EN" />

  <xsl:variable name="root" select="/" />

  <xsl:template match="ConfigFile" >
    <html>
      <xsl:comment>
        This IAT was generated with software distributed during the IATSoftware.net beta program.
      </xsl:comment>
      <head>
        <xsl:element name="style">
          <xsl:attribute name="type" select="'text/css'" />
          <xsl:apply-templates select="DisplayItemList" />
          .outlinedDI
          {
          border: 1px solid #<xsl:value-of select="concat(./IATLayout/OutlineColorR, ./IATLayout/OutlineColorG, ./IATLayout/OutlineColorB), ';'" />
          }

          body {
          background: #<xsl:value-of select="concat(./IATLayout/BackColorR, ./IATLayout/BackColorG, ./IATLayout/BackColorB, ';')" />
          }

          #IATContainerDiv {
          text-align: center;
          }

          #IATDisplayDiv {
          width: <xsl:value-of select="concat(xs:integer(./IATLayout/InteriorWidth) + (xs:integer(./IATLayout/BorderWidth) * 2), 'px;')" />
          height: <xsl:value-of select="concat(xs:integer(./IATLayout/InteriorHeight) + (xs:integer(./IATLayout/BorderWidth) * 2), 'px;')" />
          border: <xsl:value-of select="concat(./IATLayout/BorderWidth, 'px solid #', ./IATLayout/BorderColorR, ./IATLayout/BorderColorG, ./IATLayout/BorderColorB, ';')" />
          position: relative;
          top: 10px;
          left: 10px;
          margin: 10px auto 10px auto;
          text-align: left;
          }

          #IATDisplayDiv h3 {
          font-family: "Times New Roman", Times, serif;
          font-size: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight div 12)" />px;
          color: #<xsl:value-of select="concat(./IATLayout/BorderColorR, ./IATLayout/BorderColorG, ./IATLayout/BorderColorB, ';')" />
          margin-top: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight div 10)" />px;
          margin-bottom: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight) div 10" />px;
          text-align: center;
          font-weight: bold;
          }

          #IATDisplayDiv h4 {
          font-family: "Times New Roman", Times, serif;
          font-size: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight div 18)" />px;
          color: #<xsl:value-of select="concat(./IATLayout/BorderColorR, ./IATLayout/BorderColorG, ./IATLayout/BorderColorB, ';')" />
          margin-top: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight div 10)" />px;
          margin-bottom: <xsl:value-of select="xs:integer(./IATLayout/InteriorHeight div 10)" />px;
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
          <xsl:value-of select="./Name" />
        </title>
        <xsl:element name="script" >
          <xsl:attribute name="type" select="'text/javascript'" />
          <xsl:attribute name="src" select="concat(//ServerURL, 'core_aes.js')" />
        </xsl:element>
        <xsl:element name="script" >
          <xsl:attribute name="type" select="'text/javascript'" />
          <xsl:attribute name="src" select="concat(//ServerURL, //ClientID, '/', //TestName, '/', //TestName, '.js')" />
        </xsl:element>
      </head>
      <xsl:element name="body">
        <xsl:attribute name="id" select="'bodyID'" />
        <xsl:attribute name="onload" select="'OnPageLoadComplete()'" />
        <xsl:attribute name="onunload" select="'OnUnload()'" />
        <xsl:apply-templates select="./DynamicSpecifiers" />
        <xsl:element name="div">
          <xsl:attribute name="id" select="'IATContainerDiv'" />
          <xsl:element name="form" >
            <xsl:attribute name="method" select="'post'" />
            <xsl:attribute name="id" select="'IATForm'" />
            <xsl:element name="input" >
              <xsl:attribute name="type" select="'hidden'" />
              <xsl:attribute name="value" select="./IATName" />
              <xsl:attribute name="name" select="'AdministeredItem'" />
            </xsl:element>
            <xsl:element name="div">
              <xsl:attribute name="id" select="'IATDisplayDiv'" />
            </xsl:element>
          </xsl:element>
        </xsl:element>
        <xsl:element name="input">
          <xsl:attribute name="id" select="'Alternate'" />
          <xsl:attribute name="type" select="'hidden'" />
          <xsl:attribute name="value" select="'__ALTERNATION_VALUE__'" />
        </xsl:element>
        <xsl:element name="script">
          <xsl:attribute name="type" select="'text/javascript'" />
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
      <xsl:element name="ReponseDisplayID">
        <xsl:value-of select="RightResponseDisplayID" />
      </xsl:element>
      <xsl:element name="ResponseDisplayID">
        <xsl:value-of select="LeftResponseDisplayID" />
      </xsl:element>
    </xsl:for-each>
  </xsl:variable>


  <xsl:template match="IATDisplayItem" >
    <xsl:value-of select="concat('#IATDisplayItem', ./ID)" />
    {
    position: absolute;
    <xsl:variable name="displayItem" select="." />
    left: <xsl:value-of select="xs:integer(./X) + xs:integer(//IATLayout/BorderWidth)" />px;
    top: <xsl:value-of select="xs:integer(./Y) + xs:integer(//IATLayout/BorderWidth)" />px;
    <xsl:variable name="id" select="./ID" />
    <xsl:if test="count($responseDisplayIDs[some $n in ResponseDisplayID satisfies xs:integer($n) eq xs:integer($id)]) gt 0" >
      <xsl:variable name="vertPadding" select="xs:string((xs:integer(//IATLayout/ResponseHeight) - xs:integer($displayItem[$id eq ID]/Width)) div 2)" />
      <xsl:variable name="horizPadding" select="xs:string((xs:integer(//IATLayout/ResponseWidth) - xs:integer($displayItem[$id eq ID]/Height)) div 2)" />
      <xsl:value-of select="concat('padding-top: ', $vertPadding, 'px;')" />
      <xsl:value-of select="concat('paddig-left: ', $horizPadding, 'px;')" />
      <xsl:value-of select="concat('padding-bottom: ', $vertPadding, 'px;')" />
      <xsl:value-of select="concat('padding-right: ', $horizPadding, 'px;')" />
    </xsl:if>
    <xsl:if test="count($responseDisplayIDs[some $n in ResponseDisplayID satisfies xs:integer($n) eq xs:integer($id)]) eq 0">
      <xsl:value-of select="'padding: 0px;'"/>
    </xsl:if>
    margin: 0px;
    }
  </xsl:template>

</xsl:stylesheet>  