﻿<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes"/>
  <xsl:template match="/ResultDocument">
    <xsl:element name="XMLWrapper">
      <xsl:variable name="rd" select="."/>
      <xsl:for-each select="distinct-values(//IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending"/>
        <xsl:variable name="currItemNum" select="xs:integer(.)"/>
        <xsl:variable name="currPos" select="position()" />
        <xsl:element name="WrappedElement">
          <xsl:attribute name="Path" select="concat('xl\drawings\drawing', ., '.xml')" />

          <xdr:wsDr xmlns:xdr="http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing" xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">
            <xdr:twoCellAnchor editAs="oneCell">
              <xdr:from>
                <xdr:col>1</xdr:col>
                <xdr:colOff>0</xdr:colOff>
                <xdr:row>4</xdr:row>
                <xdr:rowOff>28575</xdr:rowOff>
              </xdr:from>
              <xdr:to>
                <xdr:col>9</xdr:col>
                <xdr:colOff>76200</xdr:colOff>
                <xdr:row>30</xdr:row>
                <xdr:rowOff>28575</xdr:rowOff>
              </xdr:to>
              <xdr:pic>
                <xdr:nvPicPr>
                  <xdr:cNvPr id="2" name="Picture 1"/>
                  <xdr:cNvPicPr>
                    <a:picLocks noChangeAspect="1"/>
                  </xdr:cNvPicPr>
                </xdr:nvPicPr>
                <xdr:blipFill>
                  <xsl:element name="a:blip">
                    <xsl:namespace name="r" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships'" />
                    <xsl:attribute name="r:embed" select="concat('rId', $currPos)" />
                    <a:extLst>
                      <a:ext uri="{{28A0092B-C50C-407E-A947-70E740481C1C}}">
                        <a14:useLocalDpi xmlns:a14="http://schemas.microsoft.com/office/drawing/2010/main" val="0"/>
                      </a:ext>
                    </a:extLst>
                  </xsl:element>
                  <a:stretch>
                    <a:fillRect/>
                  </a:stretch>
                </xdr:blipFill>
                <xdr:spPr>
                  <a:xfrm>
                    <a:off x="609600" y="790575"/>
                    <a:ext cx="4953000" cy="4953000"/>
                  </a:xfrm>
                  <a:prstGeom prst="rect">
                    <a:avLst/>
                  </a:prstGeom>
                </xdr:spPr>
              </xdr:pic>
              <xdr:clientData/>
            </xdr:twoCellAnchor>
            <xdr:oneCellAnchor>
              <xdr:from>
                <xdr:col>1</xdr:col>
                <xdr:colOff>0</xdr:colOff>
                <xdr:row>2</xdr:row>
                <xdr:rowOff>0</xdr:rowOff>
              </xdr:from>
              <xdr:ext cx="4876800" cy="264560"/>
              <xdr:sp macro="" textlink="">
                <xdr:nvSpPr>
                  <xdr:cNvPr id="3" name="TextBox 2"/>
                  <xdr:cNvSpPr txBox="1"/>
                </xdr:nvSpPr>
                <xdr:spPr>
                  <a:xfrm>
                    <a:off x="609600" y="381000"/>
                    <a:ext cx="4876800" cy="264560"/>
                  </a:xfrm>
                  <a:prstGeom prst="rect">
                    <a:avLst/>
                  </a:prstGeom>
                  <a:solidFill>
                    <a:sysClr val="window" lastClr="FFFFFF"/>
                  </a:solidFill>
                  <a:ln>
                    <a:solidFill>
                      <a:schemeClr val="tx1"/>
                    </a:solidFill>
                  </a:ln>
                </xdr:spPr>
                <xdr:style>
                  <a:lnRef idx="0">
                    <a:scrgbClr r="0" g="0" b="0"/>
                  </a:lnRef>
                  <a:fillRef idx="0">
                    <a:scrgbClr r="0" g="0" b="0"/>
                  </a:fillRef>
                  <a:effectRef idx="0">
                    <a:scrgbClr r="0" g="0" b="0"/>
                  </a:effectRef>
                  <a:fontRef idx="minor">
                    <a:schemeClr val="tx1"/>
                  </a:fontRef>
                </xdr:style>
                <xdr:txBody>
                  <a:bodyPr vertOverflow="clip" horzOverflow="clip" wrap="square" rtlCol="0" anchor="ctr">
                    <a:spAutoFit/>
                  </a:bodyPr>
                  <a:lstStyle/>
                  <a:p>
                    <a:pPr algn="ctr"/>
                    <a:r>
                      <a:rPr lang="en-US" sz="1100"/>
                      <xsl:element name="a:t">
                        <xsl:value-of select="concat('Item #', $currItemNum)" />
                      </xsl:element>
                    </a:r>
                  </a:p>
                </xdr:txBody>
              </xdr:sp>
              <xdr:clientData/>
            </xdr:oneCellAnchor>
          </xdr:wsDr>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>