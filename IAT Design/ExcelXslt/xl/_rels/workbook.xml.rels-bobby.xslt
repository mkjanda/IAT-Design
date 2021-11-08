<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes" />
  <xsl:template match="/ResultDocument">
    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
      <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/summary.xml"/>
      <xsl:for-each select="distinct-values(TestResult/IATResult/IATResponse/ItemNum)">
        <xsl:sort select="xs:integer(.)" order="ascending" />
        <xsl:element name="Relationship">
          <xsl:attribute name="Id" select="concat('rId', position())"/>
          <xsl:attribute name="Type"
                         select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet'"/>
          <xsl:attribute name="Target" select="concat('worksheets/item', ., '.xml')"/>
        </xsl:element>
      </xsl:for-each>
    </Relationships>
  </xsl:template>
</xsl:stylesheet>