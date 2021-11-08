<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                version="2.0"
                exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="utf-8" indent="yes" cdata-section-elements="CodeLine GlobalVarDecl GlobalCode" />

  <xsl:template match="//FunctionDescriptors">
    <xsl:for-each select="FunctionDescriptor">
      <xsl:variable name="className">
        <xsl:if test="string-length(@ClassName) eq 0">
          <xsl:value-of select="''" />
        </xsl:if>
        <xsl:if test="string-length(@ClassName) gt 0">
          <xsl:value-of select="replace(@ClassName, '_(.+)', '$1')"/>
        </xsl:if>
      </xsl:variable>
      <xsl:variable name="functionName">
        <xsl:if test="string-length(@FunctionName) gt 0">
          <xsl:if test="string-length($className) eq 0">
            <xsl:value-of select="concat('.', @FunctionName)" />
          </xsl:if>
        </xsl:if>
        <xsl:if test="string-length(@FunctionName) eq 0">
          <xsl:value-of select="''" />
        </xsl:if>
      </xsl:variable>
      <xsl:if test="position() eq 1">
        <xsl:value-of select="'var '" />
      </xsl:if>
      <xsl:for-each select="Segments/Segment">
        <xsl:if test="position() eq 1">
          <xsl:value-of select="concat('_', $className, $functionName, ' = new UnencSubFunct(', $className, $functionName, '); ')" />
        </xsl:if>
        <xsl:if test="position() gt 1">
          <xsl:variable name="depth" select="xs:integer(position()) - 1" />
          <xsl:for-each select="1 to xs:integer(.)">
            <xsl:value-of select="concat($className, $functionName, '.s', $depth, xs:string(.), ' = new UnencSubFunct(', $className, $functionName, '.s', $depth, xs:string(.), '); ')" />
          </xsl:for-each>
        </xsl:if>
      </xsl:for-each>
    </xsl:for-each>
    <xsl:value-of select="'&#x0A;'" />
  </xsl:template>

  <xsl:template match="//Functions">
    <xsl:for-each select="Function">
      <xsl:if test="position() eq 1">
        <xsl:value-of select="'var '" />
      </xsl:if>
      <xsl:value-of select="concat(replace(@Name, '_?(.+)', '_$1'), ' = function(', @Param, ') { ', ., ' };&#x0A;')" />
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="/CodeFile">
    <xsl:element name="CodeFile">
      <xsl:attribute name="ElementName" select="@ElementName" />
      <xsl:element name="GlobalVarDecl">
        <xsl:value-of select="concat(//GlobalDecl, '&#x0A;')" />
      </xsl:element>
      <xsl:element name="GlobalCode">
        <xsl:value-of select="GlobalCode/Code" />
      </xsl:element>
      <xsl:for-each select="ProcessedCode">
        <xsl:element name="ProcessedCode">
          <xsl:attribute name="EntityName" select="@EntityName" />
          <xsl:element name="Constructor">
            <xsl:variable name="conElems">
              <xs:sequence />
              <xsl:for-each select="Functions/Function">
                <xsl:if test="position() eq 1">
                  <xsl:value-of select="concat('var ', @Name , ' = new SubFunct(&quot;_', @Name, '&quot;, false); ')" />
                </xsl:if>
                <xsl:if test="position() gt 1">
                  <xsl:if test="@Param eq '_p'">
                    <xsl:value-of select="concat(@Name, ' = new SubFunct(&quot;_', @Name, '&quot;, false); ')" />
                  </xsl:if>
                  <xsl:if test="@Param eq '_l'">
                    <xsl:value-of select="concat(@Name, ' = new SubFunct(&quot;_', @Name, '&quot;, true); ')" />
                  </xsl:if>
                </xsl:if>
              </xsl:for-each>
            </xsl:variable>
            <xsl:value-of select="$conElems" />
          </xsl:element>
          <xsl:element name="Declaration">
            <xsl:value-of select="Declaration" />
          </xsl:element>
          <xsl:for-each select="Functions/Function">
            <xsl:element name="CodeLine">
              <xsl:attribute name="Name" select="concat('_', @Name)" />
              <xsl:value-of select="." />
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
