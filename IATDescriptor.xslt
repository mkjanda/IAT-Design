<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="text" encoding="utf-8" indent="no" />
  <xsl:variable name="root" select="/" />

  <xsl:template match="ConfigFile" >
    <xsl:variable name="presentationsList" >
      <xsl:for-each select="//IATEvent[@EventType eq 'BeginIATBlock']" >
        <xsl:value-of select="NumPresentations" />
      </xsl:for-each>
    </xsl:variable>
    <xsl:value-of select="$presentationsList" />
  </xsl:template>
</xsl:stylesheet>