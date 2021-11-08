<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="text" encoding="utf-8" indent="no" />

  <xsl:template match="CodeFile">
    <xsl:value-of select="//GlobalVarDecl" />
    <xsl:apply-templates select="ProcessedCode" />
  </xsl:template>

  <xsl:template match="ProcessedCode">
    <xsl:value-of select="concat(Declaration, '&#x0A;')" />
    <xsl:apply-templates select="Functions/Function" />
    <xsl:value-of select="'var '" />
    <xsl:for-each select="Functions/Function">
      <xsl:value-of select="concat(replace(@Name, '_(.+)', '$1'), ' = new UnencSubFunct(', @Name, '); ')" />
    </xsl:for-each>
    <!--    <xsl:variable name="functDecl">
      <xsl:value-of select="'var '" />
      <xsl:for-each select="//FunctionDescriptor/Segments">
        <xsl:apply-templates select="Segment" />
      </xsl:for-each>

      <xsl:value-of select="'&#x0A;'" />
    </xsl:variable>
    <xsl:value-of select="string-join(concat(normalize-space($functDecl), ' '), '')" /> -->
  </xsl:template>

  <xsl:template match="Segment">
    <xsl:variable name="segPos">
      <xsl:value-of select="position()" />
    </xsl:variable>
    <xsl:variable name="numSegs">
      <xsl:value-of select="." />
    </xsl:variable>
    <xsl:variable name="subFuncts">
      <xsl:if test="xs:integer($segPos) gt 1">
        <xsl:for-each select="1 to xs:integer($numSegs)">
          <xsl:element name="subFunctName">
            <xsl:value-of select="concat('s', xs:integer($segPos) - 1, '_', .)" />
          </xsl:element>
        </xsl:for-each>
      </xsl:if>
      <xsl:if test="xs:integer($segPos) eq 1">
        <xsl:sequence select="''" />
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="functionName">
      <xsl:value-of select="replace(concat(ancestor::FunctionDescriptor/@ClassName, if (not(empty(ancestor::FunctionDescriptor/@FunctionName))) then (concat('.', ancestor::FunctionDescriptor/@FunctionName)) else ('')),  '_([.-[\\._]]+)(\\.((_)([.-[\\._]]+)))?', '$1$2')" />
    </xsl:variable>
    <xsl:if test="empty($subFuncts/subFunctName)">
      <xsl:value-of select="concat($functionName, ' = new UnencSubFunct(', replace($functionName, '_', ''), '); ')" />
    </xsl:if>
    <xsl:if test="not(empty($subFuncts))">

      <xsl:for-each select="$subFuncts/subFunctName">
        <xsl:value-of select="concat($functionName, '.', ., ' = new UnencSubFunct(', replace($functionName, '_', ''), '.', ., '); ')" />
      </xsl:for-each>
    </xsl:if>
  </xsl:template>

  <xsl:template match="//Function">
    <xsl:if test="not(contains(@Name, '.'))">
      <xsl:value-of select="concat('var ', replace(@Name, '_(.+)', '$1'), ' = function(', @Param, ') { ', ., ' };&#x0A;')" />
    </xsl:if>
    <xsl:if test="contains(@Name, '.')" >
      <xsl:value-of select="concat(replace(@Name, '_(.+)\._(.+)', '$1.$2'), ' = function(', @Param, ') { ', ., ' };&#x0A;')" />
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>