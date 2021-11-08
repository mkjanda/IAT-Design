<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
                version="2.0"
                exclude-result-prefixes="xs">


  <xsl:output method="xml" encoding="utf-8" indent="yes" standalone="yes"/>

  <xsl:template match="ResultDocument">
    <xsl:variable name="rd" select="." />
    <xsl:element name="Relationships">
      <xsl:element name="Relationship">
        <xsl:attribute name="Id" select="'rId1'" />
        <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/printerSettings'" />
        <xsl:attribute name="Target" select="../printerSettings/printerSettings1.bin" />
      </xsl:element>
      <xsl:variable name="itemNums" select="1 to @NumIATItems" />
      <xsl:variable name="shownItems" select="distinct-values(TestResult/IATResult/IATResponse/ItemNum)" />
      <xsl:for-each select="distinct-values($itemNums[.=$shownItems])">
        <xsl:element name="Relationship">
          <xsl:attribute name="Id" select="concat('rId', 1 + position())" />
          <xsl:attribute name="Type" select="'http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing'" />
          <xsl:attribute name="Target" select="concat('../drawings/drawing', position(), '.xml')" />
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>