<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes"/>
  <xsl:template match="/ResultDocument">
    <xsl:element name="XMLWrapper">
      <xsl:variable name="rd" select="." />
      <xsl:for-each select="distinct-values(TestResult/IATResult/IATResponse/ItemNum))">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="WrapperElement">
          <xsl:attribute name="concat('xl\drawings\rels\drawing', ., '.xml.rel')" />
          <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
            <xsl:element name="Relationship">
              <xsl:attribute name="Id" select="'rId1'"/>
              <xsl:attribute name="Type"
                             select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/image'"/>
              <xsl:attribute name="Target" select="concat('../media/image', ., '.jpg')"/>
            </xsl:element>
          </Relationships>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>