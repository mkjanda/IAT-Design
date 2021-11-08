<xsl:stylesheet
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0"
    exclude-result-prefixes="xs">

  <xsl:output method="xml" encoding="UTF-8" indent="yes" />

  <xsl:template match="Survey">
    <xsl:if test="count(//SurveyItem/Response[@Type eq 'Fixed Digit']) gt 0">
      <xsl:element name="SurveyCode">
        <xsl:for-each select="SurveyItem[Response/@Type eq 'Fixed Digit']">
          <xsl:variable name="precedingNodes" select="preceding-sibling::node()" />
          <xsl:variable name="precedingSurveyItems" select="$precedingNodes[compare(name(), 'SurveyItem') eq 0]" />
          <xsl:variable name="itemNum" select="count($precedingSurveyItems/Response[compare(@Type, 'Instruction') ne 0])" />
          <xsl:variable name="liNum" select="count($precedingSurveyItems)" />
          <xsl:element name="ItemFunctions">
            <xsl:attribute name="ItemNum" select="$itemNum" />
          <xsl:call-template name="BuildFixedDigitFunctions">
            <xsl:with-param name="itemNum" select="$itemNum" />
            <xsl:with-param name="liNum" select="$liNum" />
          </xsl:call-template>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="BuildFixedDigitFunctions" >
    <xsl:param name="itemNum" as="xs:integer" />
    <xsl:param name="liNum" as="xs:integer" />
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('InitializeItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'initialization'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionName">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, ');')" />
        </xsl:element>
        <xsl:element name="Code">answerInput.value = "";</xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="Function">
      <xsl:attribute name="FunctionName">
        <xsl:value-of select="concat('ValidateItem', $itemNum)" />
      </xsl:attribute>
      <xsl:attribute name="FunctionType" select="'validation'" />
      <xsl:element name="Params" />
      <xsl:element name="FunctionBody">
        <xsl:element name="Code">
          <xsl:value-of select="concat('var questionLI = document.getElementById(&quot;ItemLITag', $liNum, ');')" />
        </xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var answerInput = document.getElementById(&quot;Item', $itemNum, '&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">var answer = answerInput.value;</xsl:element>
        <xsl:element name="Code">var num = parseInt(answer, 10);</xsl:element>
        <xsl:element name="Code">var errorMsgLI, errorMsg;</xsl:element>
        <xsl:element name="Code">var bErrorInResponse = false;</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('var intExp = /[0-9]{', NumDigs, '}/;')" />
        </xsl:element>
        <xsl:element name="Code">if (!intExp.test(num))</xsl:element>
        <xsl:element name="Code">bErrorInResponse = true;</xsl:element>
        <xsl:element name="Code">if (bErrorInResponse) {</xsl:element>
        <xsl:element name="Code">if (!ForceSubmit) {</xsl:element>
        <xsl:element name="Code">errorMsgLI = document.createElement("li");</xsl:element>
        <xsl:element name="Code">errorMsgLI.className = "Error";</xsl:element>
        <xsl:element name="Code">
          <xsl:value-of select="concat('errorMsg = document.createTextNode(&quot;Please enter a numerical response of ', NumDigs, ' digits for the question below.&quot;);')" />
        </xsl:element>
        <xsl:element name="Code">errorMsgLI.appendChild(errorMsg);</xsl:element>
        <xsl:element name="Code">questionListNode.insertBefore(errorMsgLI, questionLI);</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">} else {</xsl:element>
        <xsl:element name="Code">answerInput.value = "NULL";</xsl:element>
        <xsl:element name="Code">return 1;</xsl:element>
        <xsl:element name="Code">}}</xsl:element>
        <xsl:element name="Code">return 0;</xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>