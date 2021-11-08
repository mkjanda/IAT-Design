<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">
  <xsl:output method="text" encoding="utf-8" indent="no" />

  <xsl:variable name="root" select="*" />

  <xsl:variable name="itemTypeValues" >
    <ResponseTypes>
      <ResponseType>Instruction</ResponseType>
      <ResponseType>Bounded Length</ResponseType>
      <ResponseType>Bounded Number</ResponseType>
      <ResponseType>Date</ResponseType>
      <ResponseType>Fixed Digit</ResponseType>
      <ResponseType>Fixed Length</ResponseType>
      <ResponseType>Instruction</ResponseType>
      <ResponseType>Likert</ResponseType>
      <ResponseType>Maximum Length</ResponseType>
      <ResponseType>Multiple Selection</ResponseType>
      <ResponseType>Multiple Choice</ResponseType>
      <ResponseType>Regular Expression</ResponseType>
      <ResponseType>Weighted Multiple Choice</ResponseType>
    </ResponseTypes>
  </xsl:variable>

  <xsl:template match="Survey" >
    <xsl:variable name="surveyItemTypes" >
      <xsl:if test="count(ResponseTypes[ResponseType ne 'Instruction']/ResponseType) eq 0" >
        <xsl:value-of select="0" />
      </xsl:if>
      <xsl:if test="count(ResponseTypes[ResponseType ne 'Instruction']/ResponseType) ne 0" >
        <xsl:for-each select="ResponseTypes[ResponseType ne 'Instruction']/ResponseType" >
          <xsl:variable name="type" select = "." />
          <xsl:value-of select="count($itemTypeValues/ResponseTypes/ResponseType[. eq $type]/preceding-sibling::*)" />
        </xsl:for-each>
      </xsl:if>
    </xsl:variable>
    <xsl:value-of select="$surveyItemTypes" />
  </xsl:template>

  <xsl:template match="ConfigFile" >
    <xsl:for-each select="BeforeSurvey" >
      <xsl:apply-templates select="./Survey" />
    </xsl:for-each>
    <xsl:variable name="presentationsList" >
      <xsl:for-each select="//IATEvent[@EventType eq 'BeginIATBlock']" >
        <xsl:value-of select="NumPresentations" />
      </xsl:for-each>
    </xsl:variable>
    <xsl:value-of select="$presentationsList" />
    <xsl:for-each select="AfterSurvey" >
      <xsl:apply-templates select="./Survey" />
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>

